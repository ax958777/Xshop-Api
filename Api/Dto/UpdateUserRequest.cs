using System.ComponentModel.DataAnnotations;

namespace Api.Dto
{
    public class UpdateUserRequest
    {
        public string? Email { get; set; }

        public string? Avatar { get; set; }

        public string? Name { get; set; }
    }
}
