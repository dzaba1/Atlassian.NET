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

        public static void ThrowIfNullOrWhiteSpace(string argument, string paramName = null)
        {
            if (argument is null)
            {
                throw new System.ArgumentNullException(paramName);
            }

            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new System.ArgumentException("The value cannot be an empty string or composed entirely of whitespace.", paramName);
            }
        }
    }
}

#endif
