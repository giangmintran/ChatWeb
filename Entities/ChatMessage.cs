using System.ComponentModel.DataAnnotations;

namespace ChatApp.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        public string RoomName { get; set; } = "General"; // Room mặc định

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsDeleted { get; set; } = false;
    }
}
