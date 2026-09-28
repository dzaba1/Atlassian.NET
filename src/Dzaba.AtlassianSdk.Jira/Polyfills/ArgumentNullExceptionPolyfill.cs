#if NETSTANDARD2_0

namespace Dzaba.AtlassianSdk.Jira;

/// <summary>
/// Polyfill for <c>ArgumentNullException.ThrowIfNull</c>, which is not available on netstandard2.0.
/// </summary>
internal static class ArgumentNullExceptionPolyfill
{
    extension(System.ArgumentNullException)
    {
        public static void ThrowIfNull(object argument, string paramName = null)
        {
            if (argument is null)
            {
                throw new System.ArgumentNullException(paramName);
            }
        }
    }
}

#endif
