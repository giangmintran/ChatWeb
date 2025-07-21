using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace ChatApp.Hubs
{
    [Authorize] // Yêu cầu đăng nhập để sử dụng chat
    public class ChatHub : Hub
    {
        // Gửi tin nhắn đến tất cả clients
        public async Task SendMessage(string user, string message)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, message, DateTime.Now);
        }

        // Gửi tin nhắn đến một room cụ thể
        public async Task SendMessageToRoom(string roomName, string user, string message)
        {
            await Clients.Group(roomName).SendAsync("ReceiveMessage", user, message, DateTime.Now);
        }

        // Join vào một room
        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).SendAsync("UserJoined", Context.User.Identity.Name, roomName);
        }

        // Leave khỏi room
        public async Task LeaveRoom(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
            await Clients.Group(roomName).SendAsync("UserLeft", Context.User.Identity.Name, roomName);
        }

        // Thông báo user typing
        public async Task UserTyping(string roomName, string user)
        {
            await Clients.OthersInGroup(roomName).SendAsync("UserTyping", user);
        }

        // Thông báo user stop typing
        public async Task UserStoppedTyping(string roomName, string user)
        {
            await Clients.OthersInGroup(roomName).SendAsync("UserStoppedTyping", user);
        }

        // Khi user connect
        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.Identity.Name;
            await Clients.All.SendAsync("UserConnected", userName);
            await base.OnConnectedAsync();
        }

        // Khi user disconnect
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userName = Context.User.Identity.Name;
            await Clients.All.SendAsync("UserDisconnected", userName);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
