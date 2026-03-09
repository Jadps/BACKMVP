using Microsoft.AspNetCore.Http;
using System;

namespace MVP.WebAPI.Extensions;

public static class HttpResponseExtensions
{
    public static void AppendSecureCookie(this HttpResponse response, string key, string value, DateTime? expires = null)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expires
        };
        response.Cookies.Append(key, value, options);
    }

    public static void DeleteSecureCookie(this HttpResponse response, string key)
    {
        var options = new CookieOptions
        {
            Secure = true,
            SameSite = SameSiteMode.None
        };
        response.Cookies.Delete(key, options);
    }
}