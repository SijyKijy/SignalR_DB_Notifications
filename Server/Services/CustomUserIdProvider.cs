using Microsoft.AspNetCore.SignalR;

namespace Server.Services
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection) => connection.GetHttpContext().Request.Query["token"];
    }
}
