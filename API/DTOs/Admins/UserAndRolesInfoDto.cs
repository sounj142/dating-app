using System.Collections.Generic;

namespace API.DTOs.Admins
{
    public class UserAndRolesInfoDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string KnownAs { get; set; }
        public IList<string> Roles { get; set; }
    }
}
