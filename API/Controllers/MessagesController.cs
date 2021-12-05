using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly ClientInformation _clientInformation;
        private readonly IMapper _mapper;

        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository, 
            ClientInformation clientInformation, IMapper mapper)
        {
            _userRepository = userRepository;
            _messageRepository = messageRepository;
            _clientInformation = clientInformation;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> SendMessage(CreateMessageDto createMessageDto)
        {
            var currentUser = await _userRepository.GetCurrentUserAsync(User);
            var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipientUserName);

            if (recipient == null)
                return NotFound();

            if (currentUser.Id == recipient.Id)
                return BadRequest("You cannot send message to yourself!");

            var message = new Message
            {
                Content = createMessageDto.Content,
                MessageSent = _clientInformation.GetClientNow(),
                RecipientId = recipient.Id,
                RecipientUserName = recipient.UserName,
                SenderId = currentUser.Id,
                SenderUserName = currentUser.UserName
            };
            _messageRepository.AddMessage(message);

            if (!await _messageRepository.SaveAllAsync()) return BadRequest("Failed to send message!");

            return Ok(_mapper.Map<MessageDto>(message));
        }

        [HttpGet]
        public async Task<IList<MessageDto>> GetMessages([FromQuery] MessageParams messageParams)
        {
            messageParams.UserId = User.GetUserId();

            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages);
            return messages;
        }

        [HttpGet("thread/{userId}")]
        public async Task<IList<MessageDto>> GetMessagesThread(int userId)
        {
            var messages = await _messageRepository.GetMessagesThread(User.GetUserId(), userId);
            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var user = await _userRepository.GetCurrentUserAsync(User);

            var message = await _messageRepository.GetMessage(id);

            if (message == null) return NotFound();
            if (user.Id != message.SenderId && user.Id != message.RecipientId) return Unauthorized();

            if (user.Id == message.SenderId) message.SenderDeleted = true;
            if (user.Id == message.RecipientId) message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted) _messageRepository.DeleteMessage(message);

            if (!await _messageRepository.SaveAllAsync()) return BadRequest("Failed to delete message!");

            return Ok();
        }
    }
}
