using Atlassian.Jira.OAuth;
using FluentAssertions;
using Newtonsoft.Json;
using System;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class OAuthAccessTokenTest
{
    [Test]
    public void OAuthAccessToken_CanDeserialize()
    {
        // Arrange
        var accessToken = new OAuthAccessToken(
            "oauth_token",
            "oauth_token_secret",
            DateTimeOffset.Now);
        var json = JsonConvert.SerializeObject(accessToken);

        // Act
        var deserializedAccessToken = JsonConvert.DeserializeObject<OAuthAccessToken>(json);

        // Assert
        deserializedAccessToken.OAuthToken.Should().Be(accessToken.OAuthToken);
        deserializedAccessToken.OAuthTokenSecret.Should().Be(accessToken.OAuthTokenSecret);
        deserializedAccessToken.OAuthTokenExpiry.Should().Be(accessToken.OAuthTokenExpiry);
    }
}
