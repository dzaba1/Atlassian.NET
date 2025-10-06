using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Atlassian.Jira.Remote;
using Moq;
using Moq.Language.Flow;

namespace Atlassian.Jira.Test;

public static class MoqExtensions
{
    public static void SetupIssues(this Mock<IIssueService> mock, Jira jira, params RemoteIssue[] remoteIssues)
    {
        var issues = remoteIssues.Select(i => i.ToLocal(jira));

        mock.Setup(s => s.GetIssuesFromJqlAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(issues.ToAsyncEnumerable());
    }

    public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
       params TResult[] results) where T : class
    {
        setup.Returns(new Queue<TResult>(results).Dequeue);
    }

    public static void ReturnsInOrder<T, TResult>(this ISetup<T, TResult> setup,
      params object[] results) where T : class
    {
        var queue = new Queue(results);
        setup.Returns(() =>
        {
            var result = queue.Dequeue();
            if (result is Exception)
            {
                throw result as Exception;
            }
            return (TResult)result;
        });
    }
}
