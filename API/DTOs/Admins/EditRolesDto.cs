namespace API.DTOs.Admins
{
    public class EditRolesDto
    {
        public string UserName { get; set; }
        public string[] Roles { get; set; }
    }

    public class UserUpdateNoticationData
    {
        public UserUpdateNoticationData(string photoUrl, string knownAs, string gender)
        {
            PhotoUrl = photoUrl;
            KnownAs = knownAs;
            Gender = gender;
        }

        public string PhotoUrl { get; set; }
        public string KnownAs { get; set; }
        public string Gender { get; set; }
    }
}
