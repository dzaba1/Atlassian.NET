#if NETSTANDARD2_0

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Polyfill for <c>ArgumentException.ThrowIfNullOrEmpty</c>, which is not available on netstandard2.0.
/// </summary>
internal static class ArgumentExceptionPolyfill
{
    extension(System.ArgumentException)
    {
        public static void ThrowIfNullOrEmpty(string argument, string paramName = null)
        {
            if (argument is null)
            {
                throw new System.ArgumentNullException(paramName);
            }

            if (argument.Length == 0)
            {
                throw new System.ArgumentException("The value cannot be an empty string.", paramName);
            }
        }
    }
}

#endif
