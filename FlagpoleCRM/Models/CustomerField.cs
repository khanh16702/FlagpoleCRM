using System;
using System.Collections.Generic;

namespace FlagpoleCRM.Models
{
    public partial class CustomerField
    {
        public int Id { get; set; }
        public string KeyName { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string? DataType { get; set; }
        public int? WebsiteId { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public virtual Website? Website { get; set; }
    }
}
