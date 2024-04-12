using System;
using System.Collections.Generic;

namespace FlagpoleCRM.Models
{
    public partial class Audience
    {
        public Audience()
        {
            AudienceCustomers = new HashSet<AudienceCustomer>();
        }

        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public bool IsHasModification { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsDynamic { get; set; }
        public string? Sqlquery { get; set; }
        public string? ElasticQuery { get; set; }
        public string? RulesQueryBuilder { get; set; }
        public int? Limit { get; set; }
        public bool IsDeleted { get; set; }
        public string WebsiteId { get; set; }

        public virtual ICollection<AudienceCustomer> AudienceCustomers { get; set; }
    }
}
