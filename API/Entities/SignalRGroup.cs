using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class SignalRGroup
    {
        [Key]
        public string Name { get; set; }

        public ICollection<SignalRConnection> Connections { get; set; }
    }
}
