using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;
using MimeKit;
using System.Globalization;
using MongoDB.Driver.Linq;
using EnumsNET;

namespace Common.Models
{
    public class ElasticQuery
    {
        [JsonProperty("from")]
        public int From { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("query")]
        public object Query { get; set; }
        [JsonProperty("_source")]
        public bool Source { get; set; }
        [JsonProperty("sort")]
        public List<Dictionary<string, object>> Sort { get; set; }
    }

    public class ElasticSearchAfter : ElasticQuery
    {
        [JsonProperty("search_after")]
        public List<object> SearchAfter { get; set; }

        [JsonProperty("pit")]
        public Pit Pit { get; set; }

        public ElasticSearchAfter() { }

        public ElasticSearchAfter(ElasticQuery elasticQuery)
        {
            From = 0;
            Size = elasticQuery.Size;
            Query = elasticQuery.Query;
            Source = elasticQuery.Source;
            Sort = new List<Dictionary<string, object>>()
            {
                new Dictionary<string, object>()
                {
                    {
                        "OrgCreatedDate", new {
                            order = "asc"
                        }
                    }
                }
            };
            Pit = null;
        }
    }

    public class ElasticCountQuery
    {
        [JsonProperty("query")]
        public object Query { get; set; }
    }

    public class ElasticCountResult
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class Hit2
    {
        public long Index { get; set; }
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_source")]
        public Source Source { get; set; }

        [JsonProperty("sort")]
        public List<object> Sort { get; set; }

        public string Error { get; set; }
    }

    public class Hit
    {
        [JsonProperty("total")]
        public Total Total { get; set; }

        [JsonProperty("hits")]
        public List<Hit2> Hits { get; set; }
    }

    public class ElasticSearchResult
    {
        [JsonProperty("took")]
        public int Took { get; set; }

        [JsonProperty("timed_out")]
        public bool TimedOut { get; set; }

        [JsonProperty("_shards")]
        public Shards Shards { get; set; }

        [JsonProperty("hits")]
        public Hit Hits { get; set; }

        public string Error { get; set; }
    }

    public class Shards
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("successful")]
        public int Successful { get; set; }

        [JsonProperty("skipped")]
        public int Skipped { get; set; }

        [JsonProperty("failed")]
        public int Failed { get; set; }
    }

    public class Total
    {
        [JsonProperty("value")]
        public int Value { get; set; }

        [JsonProperty("relation")]
        public string Relation { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Source
    {
        public long OrgId { get; set; }
        public DateTime? Birthday { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime OrgCreatedDate { get; set; }
        public string WebsiteId { get; set; }
        public RFM RFM { get; set; }
        public List<Contact> Contacts { get; set; } = new List<Contact>();
        public List<int>? ChannelSubscribes { get; set; }
        public int TotalOrders { get; set; } = 0;
        public double TotalSpent { get; set; } = 0;
        public double TotalPaid { get; set; } = 0;
        public int TotalPayTimes { get; set; } = 0;
        public double AverageOrderValue { get; set; }
        public int TotalProducts { get; set; } = 0;
        public int TotalUniqueProducts { get; set; } = 0;
        public List<Product> BestSelling { get; set; } = new List<Product>();
        public int TotalCancelledOrders { get; set; } = 0;
        public int CompletePaymentTimes { get; set; } = 0;
        public int TotalDiscountCodeUsed { get; set; } = 0;
        public string OrgCreatedAtView
        {
            get
            {
                return OrgCreatedDate.ToString("HH:mm:ss dd/MM/yyyy");
            }
        }

        public string UpdatedAtView
        {
            get
            {
                return ModifiedDate.HasValue ? ModifiedDate.Value.ToString("HH:mm:ss dd/MM/yyyy") : "-";
            }
        }

        public string BirthdayText
        {
            get
            {
                return Birthday.HasValue ? Birthday.Value.ToString("dd/MM/yyyy") : "-";
            }
        }

        public int PhoneCount
        {
            get
            {
                var cnt = 0;
                foreach (Contact contact in Contacts)
                {
                    if (!string.IsNullOrEmpty(contact.Phone))
                    {
                        cnt++;
                    }
                }
                return cnt;
            }
        }

        public int EmailCount
        {
            get
            {
                var cnt = 0;
                foreach (Contact contact in Contacts)
                {
                    if (!string.IsNullOrEmpty(contact.Email))
                    {
                        cnt++;
                    }
                }
                return cnt;
            }
        }

        public int NameCount
        {
            get
            {
                var cnt = 0;
                foreach (Contact contact in Contacts)
                {
                    if (!string.IsNullOrEmpty(contact.FullName))
                    {
                        cnt++;
                    }
                }
                return cnt;
            }
        }

        public int AddressCount
        {
            get
            {
                var cnt = 0;
                foreach (Contact contact in Contacts)
                {
                    if (!string.IsNullOrEmpty(contact.Address))
                    {
                        cnt++;
                    }
                }
                return cnt;
            }
        }

        public string ChannelSubcribeView
        {
            get
            {
                if (ChannelSubscribes == null || (ChannelSubscribes != null && ChannelSubscribes.Count == 0))
                {
                    return "-";
                }
                var lcList = "";

                foreach (int channelValue in ChannelSubscribes)
                {
                    if (Enum.IsDefined(typeof(EChannelSubscribe), channelValue))
                    {
                        if (!string.IsNullOrEmpty(lcList))
                        {
                            lcList += "</br>";
                        }
                        var channelName = Enum.GetName(typeof(EChannelSubscribe), channelValue);
                        lcList += $"<span class='badge badge-success badge-pill text-color-success bg-success-custom fs-md font-weight-normal'>{channelName}</span>&nbsp;";
                    }
                }
                if (string.IsNullOrEmpty(lcList)) return "—";
                return lcList;
            }
        }

        public string TagsView
        {
            get
            {
                if (Tags == null || (Tags != null && Tags.Count == 0))
                {
                    return "-";
                }
                var lcList = "";

                foreach (string tag in Tags)
                {
                    if (!string.IsNullOrEmpty(lcList))
                    {
                        lcList += "</br>";
                    }
                    lcList += $"<span class='badge badge-info'>{tag}</span>&nbsp;";
                }
                if (string.IsNullOrEmpty(lcList)) return "—";
                return lcList;
            }
        }

        public string RFMView
        {
            get
            {
                if (RFM != null)
                {
                    var group = RFM.RFMGroup;
                    return ((ERFM)group).AsString(EnumFormat.Description);
                }
                else
                {
                    return "-";
                }
            }
        }

        public string BestSellingView
        {
            get
            {
                if (BestSelling == null || (BestSelling != null && BestSelling.Count == 0))
                {
                    return "-";
                }

                var lcList = "";
                foreach (Product product in BestSelling)
                {
                    if (!string.IsNullOrEmpty(lcList))
                    {
                        lcList += "</br>";
                    }
                    lcList += product.Name;
                }
                if (string.IsNullOrEmpty(lcList)) return "—";
                return lcList;
            }
        }
    }

    public class Pit
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("keep_alive")]
        public string KeepAlive { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class MongoCustomerModel : Source
    {
        [JsonProperty("_id")]
        public ObjectId Id { get; set; }
    }

    public class ElasticCustomer
    {
        [JsonProperty("OrgId")]
        [BsonElement("OrgId")]
        public long OrgId { get; set; }
        [JsonProperty("Birthday")]
        [BsonElement("Birthday")]
        public DateTime? Birthday { get; set; }
        [JsonProperty("Tags")]
        [BsonElement("Tags")]
        public List<string> Tags { get; set; } = new List<string>();
        [JsonProperty("CreatedDate")]
        [BsonElement("CreatedDate")]
        public DateTime CreatedDate { get; set; }
        [JsonProperty("ModifiedDate")]
        [BsonElement("ModifiedDate")]
        public DateTime? ModifiedDate { get; set; }
        [JsonProperty("OrgCreatedDate")]
        [BsonElement("OrgCreatedDate")]
        public DateTime OrgCreatedDate { get; set; }
        [JsonProperty("WebsiteId")]
        [BsonElement("WebsiteId")]
        public string WebsiteId { get; set; }
        [JsonProperty("RFM")]
        [BsonElement("RFM")]
        public RFM RFM { get; set; }
        [JsonProperty("Contacts")]
        [BsonElement("Contacts")]
        public List<Contact> Contacts { get; set; } = new List<Contact>();
        [JsonProperty("ChannelSubscribes")]
        [BsonElement("ChannelSubscribes")]
        public List<int>? ChannelSubscribes { get; set; }
        [JsonProperty("TotalOrders")]
        [BsonElement("TotalOrders")]
        public int TotalOrders { get; set; } = 0;
        [JsonProperty("TotalSpent")]
        [BsonElement("TotalSpent")]
        public double TotalSpent { get; set; } = 0;
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
    }
}
