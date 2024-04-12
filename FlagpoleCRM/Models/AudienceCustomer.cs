using System;
using System.Collections.Generic;

namespace FlagpoleCRM.Models
{
    public partial class AudienceCustomer
    {
        public int Id { get; set; }
        public string? AudienceId { get; set; }
        public string CustomerId { get; set; } = null!;
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Audience? Audience { get; set; }
    }
}
