using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace MVP.WebAPI.Services;

public class CustomUserIdProvider : IUserIdProvider
{
    public virtual string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? connection.User?.FindFirst("sub")?.Value
            ?? connection.User?.FindFirst("nameid")?.Value
            ?? connection.User?.FindFirst("uid")?.Value;
    }
}
