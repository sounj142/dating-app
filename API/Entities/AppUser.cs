using API.Extensions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Entities
{
    public class AppUser : IdentityUser<int>
    {
        [DataType(DataType.Date)]
        public DateTime DateOfBirth { get; set; }
        public string KnownAs { get; set; }
        public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset LastActive { get; set; } = DateTimeOffset.Now;
        public string Gender { get; set; }
        public string Introduction { get; set; }
        public string LookingFor { get; set; }
        public string Interests { get; set; }
        public string City { get; set; }
        public string Country { get; set; }

        public ICollection<Photo> Photos { get; set; }
        public ICollection<UserLike> LikedByUsers { get; set; }
        public ICollection<UserLike> LikedUsers { get; set; }
        public ICollection<Message> MessagesSent { get; set; }
        public ICollection<Message> MessagesReceived { get; set; }

        public int GetAge()
        {
            return DateOfBirth.CalculateAge();
        }
    }
}
