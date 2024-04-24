using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FlagpoleCRM.Models
{
    public partial class Account
    {
        public Account()
        {
            Websites = new HashSet<Website>();
        }

        [JsonProperty("id")]
        public string Id { get; set; } = null!;
        [JsonProperty("password")]
        public string Password { get; set; } = null!;
        [JsonProperty("fullName")]
        public string? FullName { get; set; }
        [JsonProperty("avatar")]
        public string? Avatar { get; set; }
        [JsonProperty("timezone")]
        public string? Timezone { get; set; }
        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }
        [JsonProperty("isDeleted")]
        public bool IsDeleted { get; set; }
        [JsonProperty("emailConfirmed")]
        public bool EmailConfirmed { get; set; }
        [JsonProperty("phoneNumber")]
        public string? PhoneNumber { get; set; }
        [JsonProperty("email")]
        public string? Email { get; set; }
        [JsonProperty("phoneNumberConfirmed")]
        public bool? PhoneNumberConfirmed { get; set; }
        [JsonProperty("secretKey")]
        public string? SecretKey { get; set; }
        [JsonProperty("salt")]
        public string? Salt { get; set; }
        [JsonProperty("phoneAccounts")]
        public virtual ICollection<Website> Websites { get; set; }
    }
}
