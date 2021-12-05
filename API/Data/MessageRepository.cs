using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly ClientInformation _clientInformation;

        public MessageRepository(DataContext dataContext, IMapper mapper, ClientInformation clientInformation)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _clientInformation = clientInformation;
        }

        public void AddMessage(Message message)
        {
            _dataContext.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _dataContext.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _dataContext.Messages.FindAsync(id);
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

        public async Task<IList<MessageDto>> GetMessagesThread(int userId, int otherUserId)
        {
            var messages = await _dataContext.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(m =>
                    (m.SenderId == userId && m.RecipientId == otherUserId && !m.SenderDeleted) ||
                    (m.RecipientId == userId && m.SenderId == otherUserId && !m.RecipientDeleted)
                )
                .OrderBy(u => u.MessageSent)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null && m.RecipientId == userId).ToList();
            if (unreadMessages.Count > 0)
            {
                var now = _clientInformation.GetClientNow();
                foreach (var message in unreadMessages)
                {
                    message.DateRead = now;
                }
                await _dataContext.SaveChangesAsync();
            }

            return _mapper.Map<IList<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _dataContext.SaveChangesAsync() > 0;
        }
    }
}
