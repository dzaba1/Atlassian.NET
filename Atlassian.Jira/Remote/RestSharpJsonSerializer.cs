//   Copyright (c) .NET Foundation and Contributors
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 

using Newtonsoft.Json;
using RestSharp;
using RestSharp.Serializers;
using System;
using System.IO;

namespace Atlassian.Jira.Remote;

/// <summary>
/// Taken from https://github.com/restsharp/RestSharp/blob/603cb85911b4db8ce952d3a332d654caf1bf9f59/src/RestSharp.Serializers.NewtonsoftJson/JsonNetSerializer.cs
/// </summary>
public class RestSharpJsonSerializer : IRestSerializer, ISerializer, IDeserializer
{
    private readonly JsonSerializer _serializer;

    /// <summary>
    /// Default serializer
    /// </summary>
    public RestSharpJsonSerializer()
    {
        _serializer = new JsonSerializer
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Include,
            DefaultValueHandling = DefaultValueHandling.Include
        };
    }

    /// <summary>
    /// Default serializer with overload for allowing custom Json.NET settings
    /// </summary>
    public RestSharpJsonSerializer(JsonSerializer serializer)
    {
        _serializer = serializer;
    }

    /// <summary>
    /// Serialize the object as JSON
    /// </summary>
    /// <param name="obj">Object to serialize</param>
    /// <returns>JSON as String</returns>
    public string Serialize(object obj)
    {
        if (obj == null) return null;

        using (var stringWriter = new StringWriter())
        {
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonTextWriter.Formatting = Formatting.Indented;
                jsonTextWriter.QuoteChar = '"';

                _serializer.Serialize(jsonTextWriter, obj);

                var result = stringWriter.ToString();
                return result;
            }
        }
    }

    public string Serialize(Parameter parameter)
    {
        return Serialize(parameter.Value);
    }

    public T Deserialize<T>(RestResponse response)
    {
        if (response.Content == null)
        {
            throw new DeserializationException(response, new InvalidOperationException("Response content is null"));
        }

        using var reader = new JsonTextReader(new StringReader(response.Content)) { CloseInput = true };

        return _serializer.Deserialize<T>(reader);
    }

    /// <summary>
    /// Unused for JSON Serialization
    /// </summary>
    public string DateFormat { get; set; }

    /// <summary>
    /// Unused for JSON Serialization
    /// </summary>
    public string RootElement { get; set; }

    /// <summary>
    /// Unused for JSON Serialization
    /// </summary>
    public string Namespace { get; set; }

    /// <summary>
    /// Content type for serialized content
    /// </summary>
    public ContentType ContentType { get; set; } = ContentType.Json;

    public ISerializer Serializer => this;

    public IDeserializer Deserializer => this;

    public string[] AcceptedContentTypes => ContentType.JsonAccept;

    public SupportsContentType SupportsContentType => contentType => contentType.Value.Contains("json");

    public DataFormat DataFormat => DataFormat.Json;
}
