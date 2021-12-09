using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ClientInformation _clientInformation;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly IPresenceTracker _presenceTracker;

        public MessageHub(IUserRepository userRepository, IMessageRepository messageRepository, 
            ClientInformation clientInformation, IMapper mapper, IHubContext<PresenceHub> presenceHub, 
            IPresenceTracker presenceTracker)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _clientInformation = clientInformation;
            _mapper = mapper;
            _presenceHub = presenceHub;
            _presenceTracker = presenceTracker;
        }

        private void InitializecCientInformation()
        {
            _clientInformation.SetTimeZoneOffset(int.Parse(Context.GetHttpContext().Request.Query["ClientTimezoneOffset"]));
        }

        public override async Task OnConnectedAsync()
        {
            InitializecCientInformation();

            var userName = Context.User.GetUserName();
            var userId = Context.User.GetUserId();
            var recipientUserName = Context.GetHttpContext().Request.Query["Recipient"].ToString();
            
            var recipient = await _userRepository.GetUserByUserNameAsync(recipientUserName);
            if (recipient == null)
            {
                await Clients.Caller.SendAsync("SendServerErrorMessage", "Recipient not found");
                return;
            }

            var groupName = GenerateGroupName(userName, recipient.UserName);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await AddConnectionToGroup(groupName);

            var messages = await _messageRepository.GetMessagesThread(userId, recipient.Id);
            var readMessages = await _messageRepository.MarkUnreadMessagesAsRead(messages, userId, 
                _clientInformation.GetClientNow());

            await Clients.Caller.SendAsync("ReceiveMessageThread", _mapper.Map<List<MessageDto>>(messages));

            if (readMessages.Any())
            {
                await Clients.OthersInGroup(groupName).SendAsync("MessagesIsRead",
                    readMessages.Select(x => new DateReadDto
                    {
                        DateRead = x.DateRead,
                        MessageId = x.Id
                    }).ToList());
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await RemoveConnectionFromGroup();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            InitializecCientInformation();

            var currentUser = await _userRepository.GetCurrentUserAsync(Context.User);
            var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

            if (recipient == null)
            {
                await Clients.Caller.SendAsync("SendServerErrorMessage", "User not found");
                return;
            }

            if (currentUser.Id == recipient.Id)
            {
                await Clients.Caller.SendAsync("SendServerErrorMessage", "You cannot send message to yourself!");
                return;
            }

            var message = new Message
            {
                Content = createMessageDto.Content,
                MessageSent = _clientInformation.GetClientNow(),
                RecipientId = recipient.Id,
                RecipientUserName = recipient.UserName,
                SenderId = currentUser.Id,
                SenderUserName = currentUser.UserName,
            };
            var groupName = GenerateGroupName(currentUser.UserName, recipient.UserName);
            var group = await _messageRepository.GetSignalRGroup(groupName);

            bool recipientIsInChatGroup = 
                group != null && group.Connections.Any(c => c.UserName == recipient.UserName);
            if (recipientIsInChatGroup)
            {
                message.DateRead = _clientInformation.GetClientNow();
            }

            _messageRepository.AddMessage(message);

            if (!await _messageRepository.SaveAllAsync())
            {
                await Clients.Caller.SendAsync("SendServerErrorMessage", "Failed to store message into DB!");
                return;
            }

            await Clients.Group(groupName).SendAsync("ReceivedMessage", _mapper.Map<MessageDto>(message));

            if (!recipientIsInChatGroup)
            {
                var recipientConnections = _presenceTracker.GetConnections(recipient.UserName);
                if (recipientConnections.Any())
                {
                    await _presenceHub.Clients.Clients(recipientConnections).SendAsync("NewMessageNotification",
                        new { currentUser.UserName, currentUser.KnownAs });
                }
            }
        }

        private string GenerateGroupName(string sender, string recipient)
        {
            return string.Compare(sender, recipient, StringComparison.OrdinalIgnoreCase) < 0
                ? $"{sender}-{recipient}"
                : $"{recipient}-{sender}";
        }

        private async Task<SignalRGroup> AddConnectionToGroup(string groupName)
        {
            var group = await _messageRepository.GetSignalRGroup(groupName);

            if (group == null)
            {
                group = new SignalRGroup
                {
                    Name = groupName,
                    Connections = new List<SignalRConnection>()
                };

                _messageRepository.AddSignalRGroup(group);
            }

            group.Connections.Add(new SignalRConnection
            {
                ConnectionId = Context.ConnectionId,
                UserName = Context.User.GetUserName()
            });

            if (!await _messageRepository.SaveAllAsync())
                throw new HubException("Error when create SignalR Group");

            return group;
        }

        private async Task RemoveConnectionFromGroup()
        {
            var connection = await _messageRepository.GetSignalRConnection(Context.ConnectionId);
            if (connection != null)
            {
                _messageRepository.RemoveSignalRConnection(connection);
                await _messageRepository.SaveAllAsync();
            }
        }
    }
}
