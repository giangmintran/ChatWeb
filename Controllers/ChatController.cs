using ChatApp.Entities;
using ChatApp.Hubs;
using ChatApp.Models;
using ChatApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly IHubContext<ChatHub> _hubContext;
        private readonly IChatService _chatService;

        public ChatController(IHubContext<ChatHub> hubContext, IChatService chatService)
        {
            _hubContext = hubContext;
            _chatService = chatService;
        }

        public async Task<IActionResult> Index(string room = "General")
        {
            var viewModel = new ChatViewModel
            {
                Messages = await _chatService.GetMessagesAsync(room, 50),
                Rooms = await _chatService.GetRoomsAsync(),
                CurrentRoom = room,
                CurrentUser = User.Identity.Name
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, error = "Tin nhắn không hợp lệ" });
            }

            try
            {
                var message = new ChatMessage
                {
                    Content = model.Message,
                    UserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value,
                    UserName = User.Identity.Name,
                    RoomName = model.RoomName,
                    CreatedAt = DateTime.Now
                };

                // Lưu tin nhắn vào database
                await _chatService.SaveMessageAsync(message);

                // Gửi tin nhắn qua SignalR
                await _hubContext.Clients.Group(model.RoomName)
                    .SendAsync("ReceiveMessage", message.UserName, message.Content, message.CreatedAt);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Có lỗi xảy ra khi gửi tin nhắn" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string room = "General", int skip = 0, int take = 20)
        {
            var messages = await _chatService.GetMessagesAsync(room, take, skip);
            return Json(messages.Select(m => new
            {
                userName = m.UserName,
                content = m.Content,
                createdAt = m.CreatedAt.ToString("HH:mm dd/MM/yyyy")
            }));
        }

        public class CreateRoomRequest
        {
            public string RoomName { get; set; }
            public string Description { get; set; }
            public bool IsPrivate { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest input)
        {
            if (string.IsNullOrWhiteSpace(input.RoomName))
            {
                return Json(new { success = false, error = "Tên phòng không được để trống" });
            }

            try
            {
                var room = new ChatRoom
                {
                    Name = input.RoomName,
                    Description = input.Description,
                    CreatedBy = User.Identity.Name,
                    IsPrivate = input.IsPrivate
                };

                await _chatService.CreateRoomAsync(room);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Có lỗi xảy ra khi tạo phòng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> JoinRoom(string roomName)
        {
            try
            {
                await _chatService.JoinRoomAsync(roomName, User.Identity.Name);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Không thể tham gia phòng" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> LeaveRoom(string roomName)
        {
            try
            {
                await _chatService.LeaveRoomAsync(roomName, User.Identity.Name);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Không thể rời khỏi phòng" });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteMessage(int messageId)
        {
            try
            {
                var message = await _chatService.GetMessageAsync(messageId);
                if (message == null || message.UserId != User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
                {
                    return Json(new { success = false, error = "Không có quyền xóa tin nhắn này" });
                }

                await _chatService.DeleteMessageAsync(messageId);

                // Thông báo xóa tin nhắn qua SignalR
                await _hubContext.Clients.Group(message.RoomName)
                    .SendAsync("MessageDeleted", messageId);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Có lỗi xảy ra khi xóa tin nhắn" });
            }
        }
    }
}
