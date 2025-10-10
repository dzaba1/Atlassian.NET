﻿using RestSharp;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Atlassian.Jira.Remote;

internal class ScreenService : IScreenService
{
    private readonly Jira _jira;

    public ScreenService(Jira jira)
    {
        _jira = jira;
    }

    public async IAsyncEnumerable<ScreenField> GetScreenAvailableFieldsAsync(string screenId, [EnumeratorCancellation] CancellationToken token = default)
    {
        var resource = $"rest/api/2/screens/{screenId}/availableFields";

        var remoteScreenFields = await _jira.RestClient.ExecuteRequestAsync<IEnumerable<RemoteScreenField>>(Method.Get, resource, null, token).ConfigureAwait(false);

        var screenFields = remoteScreenFields.Select(x => new ScreenField(x));
        foreach (var value in screenFields)
        {
            yield return value;
        }
    }

    public async IAsyncEnumerable<ScreenTab> GetScreenTabsAsync(string screenId, string projectKey = null, [EnumeratorCancellation] CancellationToken token = default)
    {
        var resource = $"rest/api/2/screens/{screenId}/tabs";
        if (!string.IsNullOrWhiteSpace(projectKey))
        {
            resource += $"?projectKey={projectKey}";
        }

        var remoteScreenTabs = await _jira.RestClient.ExecuteRequestAsync<IEnumerable<RemoteScreenTab>>(Method.Get, resource, null, token).ConfigureAwait(false);

        var screenTabs = remoteScreenTabs.Select(x => new ScreenTab(x));
        foreach (var value in screenTabs)
        {
            yield return value;
        }
    }

    public async IAsyncEnumerable<ScreenField> GetScreenTabFieldsAsync(string screenId, string tabId, string projectKey = null, [EnumeratorCancellation] CancellationToken token = default)
    {
        var resource = $"rest/api/2/screens/{screenId}/tabs/{tabId}/fields";
        if (!string.IsNullOrWhiteSpace(projectKey))
        {
            resource += $"?projectKey={projectKey}";
        }

        var remoteScreenFields = await _jira.RestClient.ExecuteRequestAsync<IEnumerable<RemoteScreenField>>(Method.Get, resource, null, token).ConfigureAwait(false);

        var screenFields = remoteScreenFields.Select(x => new ScreenField(x));
        foreach (var value in screenFields)
        {
            yield return value;
        }
    }
}
