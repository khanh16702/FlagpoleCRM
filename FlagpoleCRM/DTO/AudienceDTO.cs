using FlagpoleCRM.Models;

namespace FlagpoleCRM.DTO
{
    public class AudienceDTO : Audience
    {
        public int Type { get; set; }

        public AudienceDTO() { }  
        public AudienceDTO(Audience audience)
        {
            Id = audience.Id;
            Name = audience.Name;
            Description = audience.Description;
            IsHasModification = audience.IsHasModification;
            CreatedDate = audience.CreatedDate;
            ModifiedDate = audience.ModifiedDate;
            IsDynamic = audience.IsDynamic;
            Sqlquery = audience.Sqlquery;
            ElasticQuery = audience.ElasticQuery;
            RulesQueryBuilder = audience.RulesQueryBuilder;
            Limit = audience.Limit;
            IsDeleted = audience.IsDeleted;
            WebsiteId = audience.WebsiteId;
            Type = audience.IsDynamic ? 1 : 0;
        }
    }
}
