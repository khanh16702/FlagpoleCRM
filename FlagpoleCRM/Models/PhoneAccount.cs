using System;
using System.Collections.Generic;

namespace FlagpoleCRM.Models
{
    public partial class PhoneAccount
    {
        public PhoneAccount()
        {
            Campaigns = new HashSet<Campaign>();
        }

        public int Id { get; set; }
        public string Phone { get; set; } = null!;
        public string? AccountId { get; set; }
        public bool IsVerified { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Account? Account { get; set; }
        public virtual ICollection<Campaign> Campaigns { get; set; }
    }
}
