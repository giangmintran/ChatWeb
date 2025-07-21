using ChatApp.Data;
using ChatApp.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Services
{
    public interface IChatService
    {
        Task<List<ChatMessage>> GetMessagesAsync(string roomName, int take = 50, int skip = 0);
        Task<ChatMessage> GetMessageAsync(int messageId);
        Task SaveMessageAsync(ChatMessage message);
        Task DeleteMessageAsync(int messageId);
        Task<List<ChatRoom>> GetRoomsAsync();
        Task<ChatRoom> GetRoomAsync(string roomName);
        Task CreateRoomAsync(ChatRoom room);
        Task JoinRoomAsync(string roomName, string userName);
        Task LeaveRoomAsync(string roomName, string userName);
    }

    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _context;

        public ChatService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ChatMessage>> GetMessagesAsync(string roomName, int take = 50, int skip = 0)
        {
            return await _context.ChatMessages
                .Where(m => m.RoomName == roomName && !m.IsDeleted)
                .OrderByDescending(m => m.CreatedAt)
                .Skip(skip)
                .Take(take)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<ChatMessage> GetMessageAsync(int messageId)
        {
            return await _context.ChatMessages
                .FirstOrDefaultAsync(m => m.Id == messageId && !m.IsDeleted);
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMessageAsync(int messageId)
        {
            var message = await GetMessageAsync(messageId);
            if (message != null)
            {
                message.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ChatRoom>> GetRoomsAsync()
        {
            return await _context.ChatRooms
                .OrderBy(r => r.Name)
                .ToListAsync();
        }

        public async Task<ChatRoom> GetRoomAsync(string roomName)
        {
            return await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.Name == roomName);
        }

        public async Task CreateRoomAsync(ChatRoom room)
        {
            var existingRoom = await GetRoomAsync(room.Name);
            if (existingRoom != null)
            {
                throw new InvalidOperationException("Phòng đã tồn tại");
            }

            _context.ChatRooms.Add(room);
            await _context.SaveChangesAsync();
        }

        public async Task JoinRoomAsync(string roomName, string userName)
        {
            var room = await GetRoomAsync(roomName);
            if (room != null && !room.Members.Contains(userName))
            {
                room.Members.Add(userName);
                await _context.SaveChangesAsync();
            }
        }

        public async Task LeaveRoomAsync(string roomName, string userName)
        {
            var room = await GetRoomAsync(roomName);
            if (room != null && room.Members.Contains(userName))
            {
                room.Members.Remove(userName);
                await _context.SaveChangesAsync();
            }
        }
    }
}