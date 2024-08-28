using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class NotificationHub : Hub
{
    public async Task SendCartUpdate(string userId, string message)
    {
        await Clients.User(userId).SendAsync("ReceiveCartUpdate", message);
    }
}
