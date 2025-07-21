using ChatApp.Entities;

namespace ChatApp.Models
{
    public class ChatViewModel
    {
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public List<ChatRoom> Rooms { get; set; } = new List<ChatRoom>();
        public string CurrentRoom { get; set; } = "General";
        public string? CurrentUser { get; set; }
    }
}
