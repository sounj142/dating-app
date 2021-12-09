using System;

namespace API.DTOs
{
    public class CreateMessageDto
    {
        public string RecipientUserName { get; set; }
        public string Content { get; set; }
    }

    public class SendMessageResult
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public MessageDto Message { get; set; }
    }

    public class MarkMessageAsReadDto
    {
        public int[] MessageIds { get; set; }
        public int ClientTimezoneOffset { get; set; }
    }

    public class DateReadDto
    {
        public int MessageId { get; set; }
        public DateTimeOffset? DateRead { get; set; }
    }
}
