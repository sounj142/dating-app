using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.SignalR
{
    [Authorize]
    public class MessageHub : Hub
    {
        private readonly IServiceProvider _serviceProvider;

        public MessageHub(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async Task OnConnectedAsync()
        {
            var userName = Context.User.GetUserName();
            await Groups.AddToGroupAsync(Context.ConnectionId, userName);

            await base.OnConnectedAsync();
        }

        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            int messageId;
            using (var scope = _serviceProvider.CreateScope())
            {
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

                var currentUser = await userRepository.GetCurrentUserAsync(Context.User);
                var recipient = await userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

                var result = new SendMessageResult();

                if (recipient == null)
                {
                    result.Error = "User not found";
                    // send result to sender's client
                    await Clients.Group(currentUser.UserName).SendAsync("SendMessageResult", result);
                    return;
                }

                if (currentUser.Id == recipient.Id)
                {
                    result.Error = "You cannot send message to yourself!";
                    // send result to sender's client
                    await Clients.Group(currentUser.UserName).SendAsync("SendMessageResult", result);
                    return;
                }

                var clientInformation = scope.ServiceProvider.GetRequiredService<ClientInformation>();
                clientInformation.SetTimeZoneOffset(createMessageDto.ClientTimezoneOffset);

                var now = clientInformation.GetClientNow();
                var message = new Message
                {
                    Content = createMessageDto.Content,
                    MessageSent = now,
                    RecipientId = recipient.Id,
                    RecipientUserName = recipient.UserName,
                    SenderId = currentUser.Id,
                    SenderUserName = currentUser.UserName,
                };
                messageRepository.AddMessage(message);

                if (!await messageRepository.SaveAllAsync())
                {
                    result.Error = "Failed to store message into DB!";
                    // send result to sender's client
                    await Clients.Group(currentUser.UserName).SendAsync("SendMessageResult", result);
                    return;
                }

                var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
                result.Succeeded = true;
                result.Message = mapper.Map<MessageDto>(message);

                messageId = message.Id;

                await Clients.Group(currentUser.UserName).SendAsync("SendMessageResult", result);

                await Clients.Group(recipient.UserName).SendAsync("ReceivedMessage", result.Message);
            }


            // if after a delay time, recipient doesn't read message, we need to notify them

            // rất tệ. Thao tác delay 5s này làm cho hàm sendmessage bị chặn phai client. User ko thể chat câu tiếp theo cho
            // đến khi hết 5s. Nếu cài đặt lại nên cài lại theo cách của tác giả =]] (chương 17), tức là cần tạo hub connection
            // dựa trên group, và khi người dùng vào tab "Mesage" của trandetail thì mới mở hub này
            await Task.Delay(5000);

            using (var scope = _serviceProvider.CreateScope())
            {
                var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var message = await messageRepository.GetMessage(messageId);
                var user = await userRepository.GetUserByIdAsync(message.SenderId);

                if (message != null && message.DateRead == null)
                {
                    await Clients.Group(message.RecipientUserName).SendAsync("MessageNotification",
                        new { user.UserName, user.KnownAs });
                }
            }
        }

        public async Task MarkMessageAsRead(MarkMessageAsReadDto dto)
        {
            using var scope = _serviceProvider.CreateScope();

            var messageRepository = scope.ServiceProvider.GetRequiredService<IMessageRepository>();

            var currentUserId = Context.User.GetUserId();
            var currentUserName = Context.User.GetUserName();
            var messages = await messageRepository.GetMessages(dto.MessageIds);

            var clientInformation = scope.ServiceProvider.GetRequiredService<ClientInformation>();
            clientInformation.SetTimeZoneOffset(dto.ClientTimezoneOffset);
            var now = clientInformation.GetClientNow();

            var result = new List<DateReadDto>();
            foreach (var message in messages)
            {
                if (message.RecipientId == currentUserId && message.DateRead == null && message.SenderId == messages[0].SenderId)
                {
                    message.DateRead = now;
                    result.Add(new DateReadDto { MessageId = message.Id, DateRead = message.DateRead });
                }
            }

            if (result.Count > 0)
            {
                await messageRepository.SaveAllAsync();

                await Clients
                    .Groups(new[] { currentUserName, messages[0].SenderUserName })
                    .SendAsync("MessageIsRead", result);
            }
        }
    }
}
