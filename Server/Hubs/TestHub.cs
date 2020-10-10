using Microsoft.AspNetCore.SignalR;
using Server.Services;
using System;
using System.Threading.Tasks;

namespace Server.Hubs
{
    public class TestHub : Hub
    {
        private ContextCollection<string> _users;

        public TestHub(ContextCollection<string> users)
        {
            _users = users;
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"[S] User {Context.ConnectionId} connected");

            if (_users.ContainsKey(Context.UserIdentifier) && !Context.Items.ContainsKey("IsAdded"))
            {
                Console.WriteLine($"User {Context.UserIdentifier} already connect");
                await Clients.Caller.SendAsync("SessionError", "User with this id already connected");
                return;
            }

            _users.Add(Context.UserIdentifier, Context);

            await Clients.Caller.SendAsync("Notify", $"Connected ConnectionId: {Context.ConnectionId} UserId: {Context.UserIdentifier}");
            await base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            Console.WriteLine($"[S] User {Context.ConnectionId} disconnected");

            _users.Remove(Context.UserIdentifier, Context);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task CloseOtherSessions()
        {
            if (_users.ContainsKey(Context.UserIdentifier))
                foreach (var context in _users.GetContexts(Context.UserIdentifier))
                    context.Abort();

            _users.RemoveAll(Context.UserIdentifier);
            await OnConnectedAsync();
        }

        public async Task AddSession()
        {
            Context.Items.Add("IsAdded", true);
            await OnConnectedAsync();
        }
    }
}