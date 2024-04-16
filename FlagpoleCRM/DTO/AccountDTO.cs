using FlagpoleCRM.Models;
using Microsoft.AspNetCore.Identity;

namespace FlagpoleCRM.DTO
{
    public class AccountDTO : IdentityUser
    {
        public string? RetypedPassword { get; set; }
        public string Password { get; set; }
        public string? Timezone { get; set; }
        public string? Avatar { get; set; }
    }
}
