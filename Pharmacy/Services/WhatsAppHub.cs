using Microsoft.AspNetCore.SignalR;

namespace Pharmacy.API.Services
{
    public class WhatsAppHub : Hub
    {
        // phoneBook: [Username -> ConnectionId]
        private static readonly Dictionary<string, string> OnlineUsers = new();
        // Simple Admin tracker: stores the username of the Admin
        private static string? AdminName = null;

        public override async Task OnConnectedAsync()
        {
            var user = Context.GetHttpContext().Request.Query["username"];
            if (!string.IsNullOrEmpty(user))
            {
                OnlineUsers[user] = Context.ConnectionId;

                // If no one is admin yet, the first person to join becomes Admin
                if (AdminName == null) AdminName = user;

                // Broadcast updated user list and who the current admin is
                await Clients.All.SendAsync("UpdateState", OnlineUsers.Keys.ToList(), AdminName);
            }
            await base.OnConnectedAsync();
        }

        // --- GROUP LOGIC ---
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMsg", "System", $"{GetSender()} joined {groupName}");
        }

        // --- ADMIN FUNCTIONALITY: KICK FROM GROUP ---
        public async Task KickUserFromGroup(string targetUser, string groupName)
        {
            var sender = GetSender();
            // SECURITY CHECK: Only the Admin can kick people
            if (sender == AdminName)
            {
                if (OnlineUsers.TryGetValue(targetUser, out string? targetId))
                {
                    // 1. Physically remove their connection from the SignalR group
                    await Groups.RemoveFromGroupAsync(targetId, groupName);

                    // 2. Notify the group that the user was kicked
                    await Clients.Group(groupName).SendAsync("ReceiveMsg", "System", $"{targetUser} was kicked by Admin.");

                    // 3. Notify the specific user they are no longer in the group
                    await Clients.Client(targetId).SendAsync("KickedAlert", groupName);
                }
            }
        }

        public async Task SendToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMsg", GetSender(), message);
        }

        private string GetSender() => OnlineUsers.FirstOrDefault(x => x.Value == Context.ConnectionId).Key ?? "Unknown";
    }
}
