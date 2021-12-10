using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public MessageRepository(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        public void AddMessage(Message message)
        {
            _dataContext.Messages.Add(message);
        }

        public void RemoveSignalRConnection(SignalRConnection connection)
        {
            _dataContext.SignalRConnections.Remove(connection);
        }

        public void AddSignalRGroup(SignalRGroup group)
        {
            _dataContext.SignalRGroups.Add(group);
        }

        public void DeleteMessage(Message message)
        {
            _dataContext.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _dataContext.Messages.FindAsync(id);
        }

        public async Task<IList<Message>> GetMessages(IEnumerable<int> messageIds)
        {
            return await _dataContext.Messages.Where(message => messageIds.Contains(message.Id))
                .ToListAsync();
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            IQueryable<Message> query = _dataContext.Messages;

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(m => m.RecipientId == messageParams.UserId && !m.RecipientDeleted),
                "Outbox" => query.Where(m => m.SenderId == messageParams.UserId && !m.SenderDeleted),
                _ => query.Where(m => m.RecipientId == messageParams.UserId && !m.RecipientDeleted && m.DateRead == null) // Unread
            };

            var resultQuery = query
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(u => u.MessageSent)
                .AsNoTracking();

            return await PagedList<MessageDto>.CreateAsync(resultQuery, messageParams.CurrentPage, messageParams.PageSize);
        }

        public async Task<IList<Message>> GetMessagesThread(int senderId, int recipientId)
        {
            var messages = await _dataContext.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m =>
                    (m.SenderId == senderId && m.RecipientId == recipientId && !m.SenderDeleted) ||
                    (m.RecipientId == senderId && m.SenderId == recipientId && !m.RecipientDeleted)
                )
                .OrderBy(u => u.MessageSent)
                .ToListAsync();

            return messages;
        }

        public IList<Message> MarkUnreadMessagesAsRead(IList<Message> messages, int userId, DateTimeOffset now)
        {
            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientId == userId).ToList();
            if (unreadMessages.Count > 0)
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = now;
                }
            }

            return unreadMessages;
        }

        public async Task<SignalRConnection> GetSignalRConnection(string connectionId)
        {
            return await _dataContext.SignalRConnections.FirstOrDefaultAsync(x => x.ConnectionId == connectionId);
        }

        public async Task<SignalRGroup> GetSignalRGroup(string groupName)
        {
            return await _dataContext.SignalRGroups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }
    }
}
