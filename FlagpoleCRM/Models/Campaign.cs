using System;
using System.Collections.Generic;

namespace FlagpoleCRM.Models
{
    public partial class Campaign
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int? WebsiteId { get; set; }
        public string? WebsiteGuid { get; set; }
        public int SendStatus { get; set; }
        public int SendType { get; set; }
        public DateTime? SendDate { get; set; }
        public int? EmailId { get; set; }
        public int? PhoneId { get; set; }
        public int? Success { get; set; }
        public int? Failure { get; set; }
        public int Channel { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual EmailAccount? Email { get; set; }
        public virtual PhoneAccount? Phone { get; set; }
        public virtual Website? Website { get; set; }
        public virtual Template? Template { get; set; }
    }
}
