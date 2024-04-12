using System;
using System.Collections.Generic;

namespace FlagpoleCRM.Models
{
    public partial class Website
    {
        public Website()
        {
            Campaigns = new HashSet<Campaign>();
            CustomerFields = new HashSet<CustomerField>();
        }

        public int Id { get; set; }
        public string Guid { get; set; } = null!;
        public string Url { get; set; } = null!;
        public int? WebsiteType { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsDeleted { get; set; }
        public string AccountId { get; set; }

        public string? ShopifyToken { get; set; }
        public string? ShopifyStore { get; set; }
        public string? HaravanToken { get; set; }

        public virtual Account? Account { get; set; }
        public virtual ICollection<Campaign> Campaigns { get; set; }
        public virtual ICollection<CustomerField> CustomerFields { get; set; }
    }
}
