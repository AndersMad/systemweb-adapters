using System;
using System.Web;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Primitives;

namespace Microsoft.AspNetCore.SystemWebAdapters.Internal;

internal class ParamsCollection : NoGetByIntNameValueCollection
{
    private readonly HttpRequest _request;

    public ParamsCollection(HttpRequest request)
    {
        _request = request;

        IsReadOnly = true;
    }

    public override string?[] AllKeys => throw new PlatformNotSupportedException("Enumerating all keys for parameters is not possible.");

    public override int Count => throw new PlatformNotSupportedException("Count of all parameters is not possible.");

    public override string? Get(string? name) => GetStringValues(name) is { Count: > 0 } result ? result.ToString() : null;

    public override string[]? GetValues(string? name) => GetStringValues(name);

    private StringValues GetStringValues(string? key)
    {
        if (key is null)
        {
            return StringValues.Empty;
        }

        if (_request.Request.Query.TryGetValue(key, out var query))
        {
            return query;
        }

        if (_request.Request.HasFormContentType && _request.Request.Form.TryGetValue(key, out var form))
        {
            return form;
        }

        if (_request.Cookies[key]?.Value is { } cookie)
        {
            return cookie;
        }

        if (_request.Request.HttpContext.Features.Get<IServerVariablesFeature>() is { } serverVariables && serverVariables[key] is { } server)
        {
            return server;
        }

        return StringValues.Empty;
    }
}
