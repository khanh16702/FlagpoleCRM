using System;
using System.Collections.Generic;

namespace FlagpoleCRM.Models
{
    public partial class EmailAccount
    {
        public EmailAccount()
        {
            Campaigns = new HashSet<Campaign>();
        }

        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public string Password { get; set; } = null!;
        public string? WebsiteId { get; set; }
        public DateTime? CreatedDate { get; set; }

        public virtual ICollection<Campaign> Campaigns { get; set; }
    }
}
