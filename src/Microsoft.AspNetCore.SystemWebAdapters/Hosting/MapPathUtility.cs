using System.IO;
using Microsoft.AspNetCore.SystemWebAdapters;
using Microsoft.Extensions.Options;

namespace System.Web.Hosting;

internal sealed class MapPathUtility : IMapPathUtility
{
    private readonly IOptions<SystemWebAdaptersOptions> _options;
    private readonly VirtualPathUtilityImpl _pathUtility;

    public MapPathUtility(IOptions<SystemWebAdaptersOptions> options, VirtualPathUtilityImpl pathUtility)
    {
        _options = options;
        _pathUtility = pathUtility;
    }

    public string MapPath(string requestPath, string? path)
    {
        requestPath = TrimQueryAndFragment(requestPath);
        path = TrimQueryAndFragment(path);

        var appPath = string.IsNullOrEmpty(path)
            ? VirtualPathUtilityImpl.GetDirectory(requestPath)
            : _pathUtility.Combine(VirtualPathUtilityImpl.GetDirectory(requestPath) ?? "/", path);

        var rootPath = _options.Value.AppDomainAppPath;

        if (string.IsNullOrEmpty(appPath))
        {
            return rootPath;
        }

        var hasTrailingSlash = !string.IsNullOrEmpty(path) && (path.EndsWith('/') || path.EndsWith('\\'));

        var combined = Path.Combine(
            rootPath,
            appPath[1..]
            .Replace('/', Path.DirectorySeparatorChar));

        // mirror the input to include or exclude a trailing slash.
        if (hasTrailingSlash && !combined.EndsWith(Path.DirectorySeparatorChar))
            combined += Path.DirectorySeparatorChar;
        else if (!hasTrailingSlash && combined.EndsWith(Path.DirectorySeparatorChar))
            combined = combined.TrimEnd(Path.DirectorySeparatorChar);

        return combined;
    }

    private static string? TrimQueryAndFragment(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        var separatorIndex = path.IndexOfAny(['?', '#']);
        return separatorIndex >= 0 ? path[..separatorIndex] : path;
    }
}
