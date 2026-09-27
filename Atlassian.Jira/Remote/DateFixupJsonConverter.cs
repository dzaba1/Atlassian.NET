using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Atlassian.Jira.Remote;

/// <summary>
/// Converts <see cref="DateTime"/> values to and from the date string format expected by JIRA's REST API.
/// </summary>
public class DateFixupJsonConverter : JsonConverter
{
    /// <inheritdoc/>
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(DateTime);
    }

    /// <inheritdoc/>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return JToken.ReadFrom(reader)?.ToObject(objectType);
    }

    /// <inheritdoc/>
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // For some reason, the dates default serialization format of JSON.NET is not understood by some
        //  JIRA rest end points, this converter fixes the date strings.
        //
        //  By default JSON.NET serializes dates as: "2015-07-16T23:02:56.153121-07:00"
        //  But JIRA expects them as: "2015-07-16T22:57:45.846-07:00"
        var dateJson = JsonConvert.SerializeObject(value, new JsonSerializerSettings()
        {
            DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffzzz"
        });
        dateJson = dateJson.Remove(dateJson.LastIndexOf(":"), 1);

        writer.WriteRawValue(dateJson);
    }
}
