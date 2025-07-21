using System.ComponentModel.DataAnnotations;

namespace ChatApp.Entities
{
    public class ChatRoom
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPrivate { get; set; } = false;

        public List<string> Members { get; set; } = new List<string>();
    }
}
