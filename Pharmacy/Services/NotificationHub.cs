using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Pharmacy.API.Services
{
    public class NotificationHub : Hub
    {
        // 1. BROADCAST: Everyone connected to this Hub sees this
        public async Task SendToEveryone(string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", "System", message);
        }

        // 2. GROUP: Only users in a specific "Room" see this
        public async Task JoinGroup(string groupName)
        {
            // Adds the current connection to a group (e.g., "AdminOnly" or "ChatRoom1")
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "System", $"{Context.ConnectionId} joined {groupName}");
        }

        public async Task SendToGroup(string groupName, string message)
        {
            await Clients.Group(groupName).SendAsync("ReceiveMessage", "Group Alert", message);
        }

        // 3. SINGLE USER: Only one specific user sees this
        // Note: SignalR maps 'User' to the ClaimTypes.NameIdentifier by default
        public async Task SendToUser(string userId, string message)
        {
            await Clients.User(userId).SendAsync("ReceiveMessage", "Private", message);
        }

        // 4. SINGLE CONNECTION: Sending to just one specific browser tab
        public async Task SendToConnection(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendAsync("ReceiveMessage", "Private Tab", message);
        }
    }
}
