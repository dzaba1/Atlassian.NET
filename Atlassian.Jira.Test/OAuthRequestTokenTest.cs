using Atlassian.Jira.OAuth;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Atlassian.Jira.Test;

public class OAuthRequestTokenTest
{
    [Test]
    public void OAuthRequestToken_CanDeserialize()
    {
        // Arrange
        var requestToken = new OAuthRequestToken(
            "authorize_uri",
            "oauth_token",
            "oauth_token_secret",
            "oauth_callback_confirmation");
        var json = JsonConvert.SerializeObject(requestToken);

        // Act
        var deserializedRequestToken = JsonConvert.DeserializeObject<OAuthRequestToken>(json);

        // Assert
        deserializedRequestToken.AuthorizeUri.Should().Be(requestToken.AuthorizeUri);
        deserializedRequestToken.OAuthToken.Should().Be(requestToken.OAuthToken);
        deserializedRequestToken.OAuthTokenSecret.Should().Be(requestToken.OAuthTokenSecret);
        deserializedRequestToken.OAuthCallbackConfirmation.Should().Be(requestToken.OAuthCallbackConfirmation);
    }
}
