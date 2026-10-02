using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Dzaba.AtlassianSdk.Jira.Model;
using Dzaba.AtlassianSdk.Jira.Model.V3;
using Microsoft.Extensions.Logging;

namespace Dzaba.AtlassianSdk.Jira.Services;

/// <summary>
/// Represents the operations on the issue fields of jira.
/// </summary>
public interface IIssueFieldService
{
    /// <summary>
    /// Returns all custom fields within JIRA.
    /// </summary>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<FieldDetails> GetCustomFieldsAsync(CancellationToken token = default);

    /// <summary>
    /// Returns custom fields within JIRA given the options specified.
    /// </summary>
    /// <param name="options">Options to fetch custom fields.</param>
    /// <param name="token">Cancellation token for this operation.</param>
    IAsyncEnumerable<FieldDetails> GetCustomFieldsAsync(CustomFieldFetchOptions options, CancellationToken token = default);
}

internal sealed class IssueFieldService : IIssueFieldService
{
    private const string CustomFieldPrefix = "customfield_";
    private const int MaxCreateMetaResults = 50;

    private readonly IClient _clientV3;
    private readonly ILogger<IssueFieldService> _logger;
    private readonly JiraCache _cache;

    public IssueFieldService(IClient clientV3, ILogger<IssueFieldService> logger, JiraCache cache)
    {
        ArgumentNullException.ThrowIfNull(clientV3);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(cache);

        _clientV3 = clientV3;
        _logger = logger;
        _cache = cache;
    }

    public async IAsyncEnumerable<FieldDetails> GetCustomFieldsAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        if (!_cache.CustomFields.Any())
        {
            _logger.LogInformation("Getting all custom fields");

            var fields = await _clientV3.GetFieldsAsync(token).ConfigureAwait(false);
            if (fields != null)
            {
                _cache.CustomFields.TryAdd(fields.Where(f => f.Custom));
            }
        }

        var values = _cache.CustomFields.Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }

    public async IAsyncEnumerable<FieldDetails> GetCustomFieldsAsync(CustomFieldFetchOptions options, [EnumeratorCancellation] CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.ProjectKeys.Count == 0 && options.IssueTypeIds.Count == 0 && options.IssueTypeNames.Count == 0)
        {
            await foreach (var value in GetCustomFieldsAsync(token))
            {
                yield return value;
            }

            yield break;
        }

        if (options.ProjectKeys.Count == 0)
        {
            throw new ArgumentException("At least one project key is required when filtering custom fields by issue type.", nameof(options));
        }

        var cacheKey = GetCacheKey(options);

        if (!_cache.ProjectCustomFields.ContainsKey(cacheKey))
        {
            _logger.LogInformation("Getting custom fields for the key {CacheKey}", cacheKey);

            var customFields = new JiraEntityDictionary<FieldDetails>();

            foreach (var projectKey in options.ProjectKeys)
            {
                await foreach (var issueType in GetCreateMetaIssueTypesAsync(projectKey, token).WithCancellation(token).ConfigureAwait(false))
                {
                    if (!MatchesIssueType(issueType, options))
                    {
                        continue;
                    }

                    await foreach (var field in GetCreateMetaFieldsAsync(projectKey, issueType.Id, token).WithCancellation(token).ConfigureAwait(false))
                    {
                        if (field.FieldId.StartsWith(CustomFieldPrefix, StringComparison.OrdinalIgnoreCase))
                        {
                            customFields.TryAdd(ToFieldDetails(field));
                        }
                    }
                }
            }

            _cache.ProjectCustomFields.TryAdd(cacheKey, customFields);
        }

        var values = _cache.ProjectCustomFields[cacheKey].Values;
        foreach (var value in values)
        {
            yield return value;
        }
    }

    private IAsyncEnumerable<IssueTypeIssueCreateMetadata> GetCreateMetaIssueTypesAsync(string projectKey, CancellationToken token)
    {
        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.GetCreateIssueMetaIssueTypesAsync(projectKey, (int)startAt, maxResults, t),
            page => page?.IssueTypes ?? page?.CreateMetaIssueType,
            page => IsLastPage(page?.IssueTypes ?? page?.CreateMetaIssueType, page?.StartAt, page?.MaxResults, page?.Total),
            MaxCreateMetaResults,
            token);
    }

    private IAsyncEnumerable<FieldCreateMetadata> GetCreateMetaFieldsAsync(string projectKey, string issueTypeId, CancellationToken token)
    {
        return PageExpander.ExpandAsync(
            (startAt, maxResults, t) => _clientV3.GetCreateIssueMetaIssueTypeIdAsync(projectKey, issueTypeId, (int)startAt, maxResults, t),
            page => page?.Fields ?? page?.Results,
            page => IsLastPage(page?.Fields ?? page?.Results, page?.StartAt, page?.MaxResults, page?.Total),
            MaxCreateMetaResults,
            token);
    }

    private static bool IsLastPage<T>(ICollection<T> items, long? startAt, int? maxResults, long? total)
    {
        return items == null || items.Count == 0 || startAt + maxResults >= total;
    }

    private static bool MatchesIssueType(IssueTypeIssueCreateMetadata issueType, CustomFieldFetchOptions options)
    {
        if (options.IssueTypeIds.Count == 0 && options.IssueTypeNames.Count == 0)
        {
            return true;
        }

        return options.IssueTypeIds.Contains(issueType.Id)
            || options.IssueTypeNames.Contains(issueType.Name, StringComparer.OrdinalIgnoreCase);
    }

    private static string GetCacheKey(CustomFieldFetchOptions options)
    {
        return $"{string.Join(",", options.ProjectKeys)}::{string.Join(",", options.IssueTypeIds)}::{string.Join(",", options.IssueTypeNames)}";
    }

    private static FieldDetails ToFieldDetails(FieldCreateMetadata metadata)
    {
        return new FieldDetails
        {
            Id = metadata.FieldId,
            Key = metadata.Key ?? metadata.FieldId,
            Name = metadata.Name,
            Custom = true,
            Schema = metadata.Schema
        };
    }
}
