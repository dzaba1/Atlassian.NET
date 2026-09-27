using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atlassian.Jira.Remote;

/// <summary>
/// Wraps a <see cref="RemoteIssue"/> together with the key of its parent issue, for use with <see cref="RemoteIssueJsonConverter"/>.
/// </summary>
public class RemoteIssueWrapper
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteIssueWrapper"/> class.
    /// </summary>
    /// <param name="remoteIssue">The remote issue being wrapped.</param>
    /// <param name="parentIssueKey">The key of the parent issue, if the issue is a sub-task.</param>
    public RemoteIssueWrapper(RemoteIssue remoteIssue, string parentIssueKey = null)
    {
        RemoteIssue = remoteIssue;
        ParentIssueKey = parentIssueKey;
    }

    /// <summary>
    /// Gets the wrapped remote issue.
    /// </summary>
    public RemoteIssue RemoteIssue { get; private set; }

    /// <summary>
    /// Gets the key of the parent issue, if the issue is a sub-task.
    /// </summary>
    public string ParentIssueKey { get; private set; }
}

/// <summary>
/// Converts a <see cref="RemoteIssueWrapper"/> to and from the JSON format used by JIRA's issue REST endpoints,
/// including custom field values.
/// </summary>
public class RemoteIssueJsonConverter : JsonConverter
{
    private readonly IEnumerable<RemoteField> _remoteFields;
    private readonly IDictionary<string, ICustomFieldValueSerializer> _customFieldSerializers;

    /// <summary>
    /// Initializes a new instance of the <see cref="RemoteIssueJsonConverter"/> class.
    /// </summary>
    /// <param name="remoteFields">The fields defined on the JIRA instance, used to resolve custom field types.</param>
    /// <param name="customFieldSerializers">The serializers used to convert custom field values, keyed by custom field type.</param>
    public RemoteIssueJsonConverter(IEnumerable<RemoteField> remoteFields, IDictionary<string, ICustomFieldValueSerializer> customFieldSerializers)
    {
        _remoteFields = remoteFields;
        _customFieldSerializers = customFieldSerializers;
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(RemoteIssueWrapper);
    }

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var issueObj = JObject.Load(reader);
        var fields = issueObj["fields"] as JObject;

        // deserialize the RemoteIssue from the fields json.
        var textReader = new JsonTextReader(new StringReader(fields.ToString()));
        var remoteIssue = serializer.Deserialize<RemoteIssue>(textReader);

        // set the id and key of the remoteissue.
        remoteIssue.id = (string)issueObj["id"];
        remoteIssue.key = (string)issueObj["key"];

        // load identifiers of JiraUsers
        remoteIssue.assignee = remoteIssue.assigneeJiraUser?.InternalIdentifier;
        remoteIssue.reporter = remoteIssue.reporterJiraUser?.InternalIdentifier;

        // load the custom fields
        var customFields = GetCustomFieldValuesFromObject(fields);
        remoteIssue.customFieldValues = customFields.Any() ? customFields.ToArray() : null;

        // save fields dictionary
        remoteIssue.fieldsReadOnly = fields;

        return new RemoteIssueWrapper(remoteIssue);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var issueWrapper = value as RemoteIssueWrapper;
        if (issueWrapper == null)
        {
            throw new InvalidOperationException(string.Format("value must be of type {0}.", typeof(RemoteIssueWrapper)));
        }

        var issue = issueWrapper.RemoteIssue;

        // prepare the JiraUser identifiers
        issue.assigneeJiraUser = string.IsNullOrEmpty(issue.assignee) ? null : new JiraUser() { InternalIdentifier = issue.assignee };
        issue.reporterJiraUser = string.IsNullOrEmpty(issue.reporter) ? null : new JiraUser() { InternalIdentifier = issue.reporter };

        // Round trip the remote issue to get a JObject that has all the fields in the proper format.
        var issueJsonBuilder = new StringBuilder();
        var textWriter = new JsonTextWriter(new StringWriter(issueJsonBuilder));
        serializer.Serialize(textWriter, issue);

        var issueJson = issueJsonBuilder.ToString();
        var fields = JObject.Parse(issueJson);

        // Add the custom fields as additional JProperties.
        AddCustomFieldValuesToObject(issue, fields);

        // Add a field for the parent issue if this is a sub-task
        if (!string.IsNullOrEmpty(issueWrapper.ParentIssueKey))
        {
            fields.Add("parent", JObject.FromObject(new
            {
                key = issueWrapper.ParentIssueKey
            }));
        }

        var wrapper = new JObject(new JProperty("fields", fields));
        wrapper.WriteTo(writer);
    }

    private string GetCustomFieldType(string customFieldId)
    {
        var remoteField = _remoteFields.FirstOrDefault(f => f.id.Equals(customFieldId, StringComparison.InvariantCultureIgnoreCase));

        return remoteField != null && remoteField.Schema.Custom != null ? remoteField.Schema.Custom : "SDK-Unknown-Field-Type";
    }

    private void AddCustomFieldValuesToObject(RemoteIssue remoteIssue, JObject jObject)
    {
        if (remoteIssue.customFieldValues != null)
        {
            foreach (var customField in remoteIssue.customFieldValues)
            {
                if (customField.values != null)
                {
                    var customFieldType = GetCustomFieldType(customField.customfieldId);
                    JToken jToken;

                    if (_customFieldSerializers.ContainsKey(customFieldType))
                    {
                        jToken = _customFieldSerializers[customFieldType].ToJson(customField.values);
                    }
                    else if (customField.serializer != null)
                    {
                        jToken = customField.serializer.ToJson(customField.values);
                    }
                    else if (customField.values.Length > 0)
                    {
                        jToken = JValue.CreateString(customField.values[0]);
                    }
                    else
                    {
                        jToken = new JArray();
                    }

                    jObject.Add(customField.customfieldId, jToken);
                }
            }
        }
    }

    private IEnumerable<RemoteCustomFieldValue> GetCustomFieldValuesFromObject(JObject jObject)
    {
        return jObject.Values<JProperty>()
            .Where(field => field.Name.StartsWith("customfield", StringComparison.InvariantCulture) && field.Value.Type != JTokenType.Null)
            .Select(field =>
            {
                var customFieldType = GetCustomFieldType(field.Name);
                var remoteCustomFieldValue = new RemoteCustomFieldValue()
                {
                    customfieldId = field.Name,
                    rawValue = field.Value
                };

                if (_customFieldSerializers.ContainsKey(customFieldType))
                {
                    remoteCustomFieldValue.values = _customFieldSerializers[customFieldType].FromJson(field.Value);
                }
                else if (field.Value.Type == JTokenType.Array)
                {
                    var serializer = new MultiStringCustomFieldValueSerializer();
                    try
                    {
                        remoteCustomFieldValue.values = serializer.FromJson(field.Value);
                    }
                    catch (JsonReaderException)
                    {
                        // If deserialization failed, then it is not an array of strings, it is not known how to
                        //    deserialize this field and treat is a black box and dump the json into the property.
                        remoteCustomFieldValue.values = new string[1] { field.Value.ToString() };
                    }
                }
                else
                {
                    remoteCustomFieldValue.values = new string[1] { field.Value.ToString() };
                }

                return remoteCustomFieldValue;
            });
    }
}
