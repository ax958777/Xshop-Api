using Microsoft.AspNetCore.Identity;

namespace Api.Model
{
    public class AppUser:IdentityUser
    {
        public string? FullName { get; set; }

        public int Credits { get; set; } = 0;

        public List<Order> Orders { get; set; } = [];
    }
}
