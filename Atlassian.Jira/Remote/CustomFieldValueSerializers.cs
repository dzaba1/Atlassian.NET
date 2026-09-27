using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atlassian.Jira.Remote;

/// <summary>
/// Serializes and deserializes a custom field value that is represented as a single JSON object with one property of interest (e.g. a "select" field).
/// </summary>
public class SingleObjectCustomFieldValueSerializer : ICustomFieldValueSerializer
{
    private readonly string _propertyName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleObjectCustomFieldValueSerializer"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the JSON property holding the value.</param>
    public SingleObjectCustomFieldValueSerializer(string propertyName)
    {
        _propertyName = propertyName;
    }

    /// <inheritdoc/>
    public string[] FromJson(JToken json)
    {
        return new string[1] { json[_propertyName]?.ToString() };
    }

    /// <inheritdoc/>
    public JToken ToJson(string[] values)
    {
        return new JObject(new JProperty(_propertyName, values[0]));
    }
}

/// <summary>
/// Serializes and deserializes a custom field value that is represented as a JSON array of objects, each with one property of interest (e.g. a "multi-select" field).
/// </summary>
public class MultiObjectCustomFieldValueSerializer : ICustomFieldValueSerializer
{
    private readonly string _propertyName;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiObjectCustomFieldValueSerializer"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the JSON property holding each value.</param>
    public MultiObjectCustomFieldValueSerializer(string propertyName)
    {
        _propertyName = propertyName;
    }

    /// <inheritdoc/>
    public string[] FromJson(JToken json)
    {
        return ((JArray)json).Select(j => j[_propertyName].ToString()).ToArray();
    }

    /// <inheritdoc/>
    public JToken ToJson(string[] values)
    {
        return JArray.FromObject(values.Select(v => new JObject(new JProperty(_propertyName, v))).ToArray());
    }
}

/// <summary>
/// Serializes and deserializes a custom field value that is represented as a floating-point number.
/// </summary>
public class FloatCustomFieldValueSerializer : ICustomFieldValueSerializer
{
    /// <inheritdoc/>
    public string[] FromJson(JToken json)
    {
        return new string[1] { json.ToObject<string>() };
    }

    /// <inheritdoc/>
    public JToken ToJson(string[] values)
    {
        return float.Parse(values[0], CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// Serializes and deserializes a custom field value that is represented as a JSON array of strings.
/// </summary>
public class MultiStringCustomFieldValueSerializer : ICustomFieldValueSerializer
{
    /// <inheritdoc/>
    public string[] FromJson(JToken json)
    {
        return JsonConvert.DeserializeObject<string[]>(json.ToString());
    }

    /// <inheritdoc/>
    public JToken ToJson(string[] values)
    {
        return JArray.FromObject(values);
    }
}

/// <summary>
/// Serializes and deserializes the value of a cascading select custom field, which has a parent value and an optional child value.
/// </summary>
public class CascadingSelectCustomFieldValueSerializer : ICustomFieldValueSerializer
{
    /// <inheritdoc/>
    public string[] FromJson(JToken json)
    {
        var parentOption = json["value"];
        var childOption = json["child"];

        if (parentOption == null)
        {
            throw new InvalidOperationException(string.Format(
                "Unable to deserialize custom field as a cascading select list. The parent value is required. Json: {0}",
                json.ToString()));
        }
        else if (childOption == null || childOption["value"] == null)
        {
            return new string[] { parentOption.ToString() };
        }
        else
        {
            return new string[2] { parentOption.ToString(), childOption["value"].ToString() };
        }
    }

    /// <inheritdoc/>
    public JToken ToJson(string[] values)
    {
        if (values == null || values.Length < 1)
        {
            throw new InvalidOperationException("Unable to serialize the custom field as a cascading select list. At least the parent value is required.");
        }
        else if (values.Length == 1)
        {
            return JToken.FromObject(new { value = values[0] });
        }
        else
        {
            return JToken.FromObject(new
            {
                value = values[0],
                child = new
                {
                    value = values[1]
                }
            });
        }
    }
}

/// <summary>
/// Serializes and deserializes the value of the GreenHopper/Jira Software "Sprint" custom field from its legacy,
/// malformed <c>toString()</c> representation (see https://ecosystem.atlassian.net/browse/ACJIRA-918).
/// </summary>
public class GreenhopperSprintCustomFieldValueSerialiser : ICustomFieldValueSerializer
{
    private readonly string _propertyName;

    /// <summary>
    /// Initializes a new instance of the <see cref="GreenhopperSprintCustomFieldValueSerialiser"/> class.
    /// </summary>
    /// <param name="propertyName">The name of the malformed property holding the sprint value (e.g. "id" or "name").</param>
    public GreenhopperSprintCustomFieldValueSerialiser(string propertyName)
    {
        _propertyName = propertyName;
    }

    /// <inheritdoc/>
    // Sprint field is malformed
    // See https://ecosystem.atlassian.net/browse/ACJIRA-918 for more information
    public string[] FromJson(JToken json)
    {
        return json.ToString()
            .Split(new char[] { '{', '}', '[', ']', ',' })
            .Where(x => x.StartsWith(_propertyName))
            .Select(x => x.Split(new char[] { '=' })[1])
            .ToArray();
    }

    /// <inheritdoc/>
    public JToken ToJson(string[] values)
    {
        string val = values != null ? values.FirstOrDefault() : null;
        int id = 0;

        if (int.TryParse(val, out id))
        {
            return id;
        }

        return val;
    }
}

/// <summary>
/// Serializes and deserializes the value of the GreenHopper/Jira Software "Sprint" custom field from its proper JSON representation.
/// </summary>
public class GreenhopperSprintJsonCustomFieldValueSerialiser : ICustomFieldValueSerializer
{
    /// <inheritdoc/>
    public string[] FromJson(JToken json)
    {
        return JsonConvert
            .DeserializeObject<List<Sprint>>(json.ToString())
            .OrderByDescending(x => x.endDate)
            .Select(x => x.name)
            .ToArray();
    }

    /// <inheritdoc/>
    public JToken ToJson(string[] values)
    {
        var val = values?.FirstOrDefault();

        if (int.TryParse(val, out var id))
        {
            return id;
        }

        return val;
    }
}

internal class Sprint
{
    public int id { get; set; }
    public string name { get; set; }
    public string state { get; set; }
    public int boardId { get; set; }
    public string goal { get; set; }
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }
    public DateTime completeDate { get; set; }
}
