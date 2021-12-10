using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public MessagesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IList<MessageDto>> GetMessages([FromQuery] MessageParams messageParams)
        {
            messageParams.UserId = User.GetUserId();

            var messages = await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages);
            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var user = await _unitOfWork.UserRepository.GetCurrentUserAsync(User);

            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if (message == null) return NotFound();
            if (user.Id != message.SenderId && user.Id != message.RecipientId) return Unauthorized();

            if (user.Id == message.SenderId) message.SenderDeleted = true;
            if (user.Id == message.RecipientId) message.RecipientDeleted = true;
            if (message.SenderDeleted && message.RecipientDeleted) _unitOfWork.MessageRepository.DeleteMessage(message);

            if (!await _unitOfWork.Complete()) return BadRequest("Failed to delete message!");

            return Ok();
        }
    }
}
