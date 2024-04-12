using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Common.Models
{
    [BsonIgnoreExtraElements]
    public class CustomerRaw
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonIgnoreIfDefault]
        public ObjectId ObjId { get; set; }
        [JsonProperty("id")]
        [BsonElement("id")]
        public long Id { get; set; }
        [JsonProperty("email")]
        [BsonElement("email")]
        public string? Email { get; set; }
        [JsonProperty("email_marketing_consent")]
        [BsonElement("email_marketing_consent")]
        public EmailConsent? EmailConsent { get; set; }
        [JsonProperty("accepts_marketing")]
        [BsonElement("accepts_marketing")]
        public bool? AcceptsMarketing { get; set; }
        [JsonProperty("created_at")]
        [BsonElement("created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [JsonProperty("first_name")]
        [BsonElement("first_name")]
        public string? FirstName { get; set; }
        [JsonProperty("last_name")]
        [BsonElement("last_name")]
        public string? LastName { get; set; }
        [JsonProperty("total_spent")]
        [BsonElement("total_spent")]
        public double TotalSpent { get; set; }
        [JsonProperty("last_order_id")]
        [BsonElement("last_order_id")]
        public long? LastOrderId { get; set; }
        [JsonProperty("tags")]
        [BsonElement("tags")]
        public string? Tags { get; set; }
        [JsonProperty("phone")]
        [BsonElement("phone")]
        public string? Phone { get; set; }
        [JsonProperty("birthday")]
        [BsonElement("birthday")]
        public DateTime? Birthday { get; set; }
        [JsonProperty("addresses")]
        [BsonElement("addresses")]
        public List<Address>? Addresses { get; set; }
        [JsonProperty("RFM")]
        [BsonElement("RFM")]
        public RFM RFM { get; set; }
        [JsonProperty("orders_count")]
        [BsonElement("orders_count")]
        public int TotalOrders { get; set; } = 0;
        [JsonProperty("TotalPaid")]
        [BsonElement("TotalPaid")]
        public double TotalPaid { get; set; } = 0;
        [JsonProperty("TotalPayTimes")]
        [BsonElement("TotalPayTimes")]
        public int TotalPayTimes { get; set; } = 0;
        [JsonProperty("AverageOrderValue")]
        [BsonElement("AverageOrderValue")]
        public double AverageOrderValue { get; set; }
        [JsonProperty("TotalProducts")]
        [BsonElement("TotalProducts")]
        public int TotalProducts { get; set; } = 0;
        [JsonProperty("TotalUniqueProducts")]
        [BsonElement("TotalUniqueProducts")]
        public int TotalUniqueProducts { get; set; } = 0;
        [JsonProperty("BestSelling")]
        [BsonElement("BestSelling")]
        public List<Product> BestSelling { get; set; } = new List<Product>();
        [JsonProperty("TotalCancelledOrders")]
        [BsonElement("TotalCancelledOrders")]
        public int TotalCancelledOrders { get; set; } = 0;
        [JsonProperty("CompletePaymentTimes")]
        [BsonElement("CompletePaymentTimes")]
        public int CompletePaymentTimes { get; set; } = 0;
        [JsonProperty("TotalDiscountCodeUsed")]
        [BsonElement("TotalDiscountCodeUsed")]
        public int TotalDiscountCodeUsed { get; set; } = 0;
        [JsonProperty("OrgSrc")]
        [BsonElement("OrgSrc")]
        public string OrgSrc { get; set; }
        [JsonProperty("WebsiteId")]
        [BsonElement("WebsiteId")]
        public string WebsiteId { get; set; }
        [JsonProperty("IsSyncCustomer")]
        [BsonElement("IsSyncCustomer")]
        public bool IsSyncCustomer { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Address
    {
        [JsonProperty("id")]
        [BsonElement("id")]
        public long Id { get; set; }
        [JsonProperty("customer_id")]
        [BsonElement("customer_id")]
        public long CustomerId { get; set; }
        [JsonProperty("name")]
        [BsonElement("name")]
        public string? Name { get; set; }
        [JsonProperty("first_name")]
        [BsonElement("first_name")]
        public string? FirstName { get; set; }
        [JsonProperty("last_name")]
        [BsonElement("last_name")]
        public string? LastName { get; set; }
        [JsonProperty("company")]
        [BsonElement("company")]
        public string? Company { get; set; }
        [JsonProperty("address1")]
        [BsonElement("address1")]
        public string? Address1 { get; set; }
        [JsonProperty("address2")]
        [BsonElement("address2")]
        public string? Address2 { get; set; }
        [JsonProperty("phone")]
        [BsonElement("phone")]
        public string? Phone { get; set; }
    }

    public class EmailConsent
    {
        [JsonProperty("state")]
        [BsonElement("state")]
        public string? State { get; set; }
    }

    public class CustomerInOrder
    {
        [BsonId]
        [BsonIgnore]
        public ObjectId? ObjId { get; set; }
        [JsonProperty("id")]
        [BsonElement("id")]
        public long Id { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class OrderRaw
    {
        [JsonProperty("id")]
        [BsonElement("orgId")]
        public long OrgId { get; set; }
        [JsonProperty("name")]
        [BsonElement("name")]
        public string? Name { get; set; }
        [JsonProperty("created_at")]
        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonProperty("cancelled_at")]
        [BsonElement("cancelled_at")]
        public DateTime? CancelledAt { get; set; }
        [JsonProperty("discount_codes")]
        [BsonElement("discount_codes")]
        public List<DiscountCode> DiscountCodes { get; set; }
        [JsonProperty("financial_status")]
        [BsonElement("financial_status")]
        public string FinancialStatus { get; set; }
        [JsonProperty("fulfillment_status")]
        [BsonElement("fulfillment_status")]
        public string? FulFillmentStatus { get; set; }
        [JsonProperty("total_price")]
        [BsonElement("total_price")]
        public double TotalPrice { get; set; }
        [JsonProperty("total_discounts")]
        [BsonElement("total_discounts")]
        public double TotalDiscounts { get; set; }
        [JsonProperty("line_items")]
        [BsonElement("line_items")]
        public List<LineItem> LineItems { get; set; }
        [JsonProperty("customer")]
        [BsonElement("customer")]
        public CustomerInOrder? Customer { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class DiscountCode
    {
        [JsonProperty("amount")]
        [BsonElement("amount")]
        public double? Amount { get; set; }

        [JsonProperty("code")]
        [BsonElement("code")]
        public string Code { get; set; }
        [JsonProperty("type")]
        [BsonElement("type")]
        public string Type { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class LineItem
    {
        [JsonProperty("id")]
        [BsonElement("id")]
        public long Id { get; set; }
        [JsonProperty("name")]
        [BsonElement("name")]
        public string Name { get; set; }
        [JsonProperty("price")]
        [BsonElement("price")]
        public string Price { get; set; }
        [JsonProperty("product_id")]
        [BsonElement("product_id")]
        public long? ProductId { get; set; }
        [JsonProperty("quantity")]
        [BsonElement("quantity")]
        public int Quantity { get; set; }

    }

    public class CustomerResult
    {
        [JsonProperty("customers")]
        public List<CustomerRaw> Customers { get; set; }
    }

    public class OrderResult
    {
        [JsonProperty("orders")]
        public List<OrderRaw> Orders { get; set; }
    }
}
