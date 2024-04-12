using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Common.Models
{
    public class Customer : ElasticCustomer
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
    }

    public class Contact
    {
        [JsonProperty("FullName")]
        [BsonElement("FullName")]
        public string? FullName { get; set; }
        [JsonProperty("Phone")]
        [BsonElement("Phone")]
        public string? Phone { get; set; }
        [JsonProperty("Email")]
        [BsonElement("Email")]
        public string? Email { get; set; }
        [JsonProperty("Address")]
        [BsonElement("Address")]
        public string? Address { get; set; }
        [JsonProperty("OrgId")]
        [BsonElement("OrgId")]
        public long OrgId { get; set; }
        [JsonProperty("OrgSrc")]
        [BsonElement("OrgSrc")]
        public string OrgSrc { get; set; }

    }

    public class Product
    {
        [JsonProperty("ProductId")]
        [BsonElement("ProductId")]
        public long Id { get; set; }
        [JsonProperty("Name")]
        [BsonElement("Name")]
        public string Name { get; set; }
        [JsonProperty("Quantity")]
        [BsonElement("Quantity")]
        public int Quantity { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Order : OrderRaw
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
        [JsonProperty("websiteId")]
        [BsonElement("websiteId")]
        public string WebsiteId { get; set; }
        [JsonProperty("orgSrc")]
        [BsonElement("orgSrc")]
        public string OrgSrc { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class RFM
    {
        [JsonProperty("RValue")]
        [BsonElement("RValue")]
        public double RValue { get; set; }
        [JsonProperty("FValue")]
        [BsonElement("FValue")]
        public double FValue { get; set; }
        [JsonProperty("MValue")]
        [BsonElement("MValue")]
        public double MValue { get; set; }

        [JsonProperty("RScore")]
        [BsonElement("RScore")]
        public int RScore { get; set; }
        [JsonProperty("FScore")]
        [BsonElement("FScore")]
        public int FScore { get; set; }
        [JsonProperty("MScore")]
        [BsonElement("MScore")]
        public int MScore { get; set; }

        [JsonProperty("RFMGroup")]
        [BsonElement("RFMGroup")]
        public int RFMGroup { get; set; }
        [JsonProperty("RFMScore")]
        [BsonElement("RFMScore")]
        public int RFMScore { get; set; }
    }
}
