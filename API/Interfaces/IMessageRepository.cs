using API.DTOs;
using API.Entities;
using API.Helpers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id);
        Task<IList<Message>> GetMessages(IEnumerable<int> messageIds);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IList<MessageDto>> GetMessagesThread(int userId, int otherUserId);
        Task<bool> SaveAllAsync();
    }
}
