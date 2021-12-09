using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class SignalRConnection
    {
        [Key]
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
    }
}