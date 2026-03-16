using Microsoft.AspNetCore.SignalR;

namespace Pharmacy.API.Services
{
    public class ChatHub : Hub
    {

        // Static dictionary to track: [Username -> ConnectionId]
        private static readonly Dictionary<string, string> UserList = new();

        public override async Task OnConnectedAsync()
        {
            // We get the username from the query string sent by the client
            var username = Context.GetHttpContext().Request.Query["username"];

            if (!string.IsNullOrEmpty(username))
            {
                UserList[username] = Context.ConnectionId;
                // Broadcast the updated list to everyone so they can see who is online
                await Clients.All.SendAsync("UpdateUserList", UserList.Keys.ToList());
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var item = UserList.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (item.Key != null)
            {
                UserList.Remove(item.Key);
                await Clients.All.SendAsync("UpdateUserList", UserList.Keys.ToList());
            }
            await base.OnDisconnectedAsync(exception);
        }

        // ONE-TO-ONE: Send to a specific person by their Username
        public async Task SendPrivateMessage(string targetUsername, string message)
        {
            if (UserList.ContainsKey(targetUsername))
            {
                string targetId = UserList[targetUsername];
                var senderName = UserList.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;

                // Send only to the target connection
                await Clients.Client(targetId).SendAsync("ReceivePrivateMessage", senderName, message);
            }
        }

        // BROADCAST: Send to everyone
        public async Task SendToAll(string message)
        {
            var senderName = UserList.FirstOrDefault(x => x.Value == Context.ConnectionId).Key;
            await Clients.All.SendAsync("ReceiveMessage", senderName, message);
        }
    }
}