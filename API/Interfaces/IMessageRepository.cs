using API.DTOs;
using API.Entities;
using API.Helpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IMessageRepository
    {
        void AddSignalRGroup(SignalRGroup group);
        void RemoveSignalRConnection(SignalRConnection connection);
        Task<SignalRConnection> GetSignalRConnection(string connectionId);
        Task<SignalRGroup> GetSignalRGroup(string groupName);
        void AddMessage(Message message);
        void DeleteMessage(Message message);
        Task<Message> GetMessage(int id);
        Task<IList<Message>> GetMessages(IEnumerable<int> messageIds);
        Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams);
        Task<IList<Message>> GetMessagesThread(int senderId, int recipientId);
        IList<Message> MarkUnreadMessagesAsRead(IList<Message> messages, int userId, DateTimeOffset now);
    }
}
