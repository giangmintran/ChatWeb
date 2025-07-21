using System.ComponentModel.DataAnnotations;

namespace ChatApp.Models
{
    public class SendMessageViewModel
    {
        [Required]
        [StringLength(500, ErrorMessage = "Tin nhắn không được vượt quá 500 ký tự")]
        public string Message { get; set; } = string.Empty;

        public string RoomName { get; set; } = "General";
    }
}
