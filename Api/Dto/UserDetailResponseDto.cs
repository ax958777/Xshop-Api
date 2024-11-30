namespace Api.Dto
{
    public class UserDetailResponseDto
    {
        public string Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;   
        public string Role { get; set; } = string.Empty;
        public string? avatar { get; set; }

        public DateTime joinDate { get; set; }

        public DateTime lastActive { get; set; }

        public string status { get; set; }

        public List<string> projects { get; set; }

        public List<string> skills { get;set; }


    }
}
