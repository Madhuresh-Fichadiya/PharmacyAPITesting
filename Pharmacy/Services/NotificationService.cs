using Microsoft.AspNetCore.SignalR;

namespace Pharmacy.API.Services
{
    public class NotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task DemoTargets()
        {
            // TARGET 1: All active users
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", "All", "Server is restarting!");

            // TARGET 2: Users in the 'Premium' group
            await _hubContext.Clients.Group("Premium").SendAsync("ReceiveMessage", "Exclusive", "New content available!");

            // TARGET 3: A specific User (by their login ID)
            await _hubContext.Clients.User("user123@email.com").SendAsync("ReceiveMessage", "Personal", "Your order shipped.");
        }
    }
}
