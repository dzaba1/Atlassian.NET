using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace Atlassian.Jira.Remote;

internal class IssueFieldService : IIssueFieldService
{
    private readonly Jira _jira;

    public IssueFieldService(Jira jira)
    {
        _jira = jira;
    }

    public async IAsyncEnumerable<CustomField> GetCustomFieldsAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        var cache = _jira.Cache;

        if (!cache.CustomFields.Any())
        {
            var remoteFields = await _jira.RestClient.ExecuteRequestAsync<RemoteField[]>(Method.Get, "rest/api/2/field", null, token).ConfigureAwait(false);
            var results = remoteFields.Where(f => f.IsCustomField).Select(f => new CustomField(f));
            cache.CustomFields.TryAdd(results);
        }

        foreach (var value in cache.CustomFields.Values)
        {
            yield return value;
        };
    }

    public async IAsyncEnumerable<CustomField> GetCustomFieldsAsync(CustomFieldFetchOptions options, [EnumeratorCancellation] CancellationToken token = default)
    {
        var cache = _jira.Cache;
        var projectKey = options.ProjectKeys.FirstOrDefault();
        var issueTypeId = options.IssueTypeIds.FirstOrDefault();
        var issueTypeName = options.IssueTypeNames.FirstOrDefault();

        if (!string.IsNullOrEmpty(issueTypeId) || !string.IsNullOrEmpty(issueTypeName))
        {
            projectKey = $"{projectKey}::{issueTypeId}::{issueTypeName}";
        }
        else if (string.IsNullOrEmpty(projectKey))
        {
            await foreach (var value in GetCustomFieldsAsync(token))
            {
                yield return value;
            }
        }

        if (!cache.ProjectCustomFields.TryGetValue(projectKey, out JiraEntityDictionary<CustomField> fields))
        {
            var resource = $"rest/api/2/issue/createmeta?expand=projects.issuetypes.fields";

            if (options.ProjectKeys.Any())
            {
                resource += $"&projectKeys={string.Join(",", options.ProjectKeys)}";
            }

            if (options.IssueTypeIds.Any())
            {
                resource += $"&issuetypeIds={string.Join(",", options.IssueTypeIds)}";
            }

            if (options.IssueTypeNames.Any())
            {
                resource += $"&issuetypeNames={string.Join(",", options.IssueTypeNames)}";
            }

            var jObject = await _jira.RestClient.ExecuteRequestAsync(Method.Get, resource, null, token).ConfigureAwait(false);
            var jProject = jObject["projects"].FirstOrDefault();

            if (jProject == null)
            {
                throw new InvalidOperationException($"Project with key '{projectKey}' was not found on the Jira server.");
            }

            var serializerSettings = _jira.RestClient.Settings.JsonSerializerSettings;
            var customFields = jProject["issuetypes"].SelectMany(issueType => GetCustomFieldsFromIssueType(issueType, serializerSettings));
            var distinctFields = customFields.GroupBy(c => c.Id).Select(g => g.First());

            cache.ProjectCustomFields.TryAdd(projectKey, new JiraEntityDictionary<CustomField>(distinctFields));
        }

        foreach (var value in cache.ProjectCustomFields[projectKey].Values)
        {
            yield return value;
        }
    }

    public IAsyncEnumerable<CustomField> GetCustomFieldsForProjectAsync(string projectKey, CancellationToken token = default)
    {
        var options = new CustomFieldFetchOptions();
        options.ProjectKeys.Add(projectKey);

        return GetCustomFieldsAsync(options, token);
    }

    private static IEnumerable<CustomField> GetCustomFieldsFromIssueType(JToken issueType, JsonSerializerSettings serializerSettings)
    {
        return ((JObject)issueType["fields"]).Properties()
            .Where(f => f.Name.StartsWith("customfield_", StringComparison.OrdinalIgnoreCase))
            .Select(f => JsonConvert.DeserializeObject<RemoteField>(f.Value.ToString(), serializerSettings))
            .Select(remoteField => new CustomField(remoteField));
    }
}
