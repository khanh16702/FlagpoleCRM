using FlagpoleCRM.Models;

namespace FlagpoleCRM.DTO
{
    public class WebsiteDTO : Website
    {
        public int Index { get; set; }
        public string HasShopifyData
        {
            get
            {
                return (!string.IsNullOrEmpty(ShopifyStore) && !string.IsNullOrEmpty(ShopifyToken)) ? "Yes" : "No";
            }
        }
        public string HasHaravanData
        {
            get
            {
                return !string.IsNullOrEmpty(HaravanToken) ? "Yes" : "No";
            }
        }
    }
}
