using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atlassian.Jira.Remote;

/// <summary>
/// Converts a value to and from a JSON object with a single nested property (e.g. <c>{ "key": "value" }</c>).
/// </summary>
public class NestedValueJsonConverter : JsonConverter
{
    private readonly string _innerProperty;

    /// <summary>
    /// Initializes a new instance of the <see cref="NestedValueJsonConverter"/> class.
    /// </summary>
    /// <param name="innerProperty">The name of the nested JSON property that holds the value.</param>
    public NestedValueJsonConverter(string innerProperty)
    {
        _innerProperty = innerProperty;
    }

    /// <inheritdoc/>
    public override bool CanConvert(Type objectType)
    {
        return true;
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var outerObject = new JObject(new JProperty(_innerProperty, value));
        outerObject.WriteTo(writer);
    }

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var outerObject = JObject.Load(reader);
        return outerObject[_innerProperty]?.ToObject(objectType);
    }
}
