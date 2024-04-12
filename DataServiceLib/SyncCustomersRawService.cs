using Common.Constant;
using Common.Helper;
using Common.Models;
using MongoDB.Bson;
using MongoDB.Driver.Linq;

namespace DataServiceLib
{
    public class SyncCustomersRawService
    {
        public static ResponseModel Insert(CustomerRaw customer, 
            string websiteId, 
            string source,
            MongoDbHelper<CustomerRaw> _mongoDbCustomerRaw,
            MongoDbHelper<Order> _mongoDbOrder)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                customer.WebsiteId = websiteId.Replace("-","");
                customer.OrgSrc = source;
                customer.IsSyncCustomer = false;

                CalculateWithOrders(ref customer, customer.Id, websiteId, source, _mongoDbOrder);
                _mongoDbCustomerRaw.Insert(customer);
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }

            return response;
        }

        public static ResponseModel Update(CustomerRaw customer, 
            ObjectId id, 
            MongoDbHelper<Order> _mongoDbOrder,
            MongoDbHelper<CustomerRaw> _mongoDbCustomerRaw,
            string websiteId, 
            string source)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try 
            {
                customer.WebsiteId = websiteId.Replace("-","");
                customer.OrgSrc = source;
                customer.IsSyncCustomer = false;

                CalculateWithOrders(ref customer, customer.Id, websiteId, source, _mongoDbOrder);
                _mongoDbCustomerRaw.Update(id, customer);
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public static void CalculateWithOrders(ref CustomerRaw customer, 
            long customerId, 
            string websiteId, 
            string source,
            MongoDbHelper<Order> _mongoDbOrder)
        {
            if (customer.TotalOrders == 0)
            {
                return;
            }

            var orders = _mongoDbOrder.Find(x => x.Customer != null && x.Customer.Id == customerId
                && x.WebsiteId == websiteId.Replace("-","")
                && x.OrgSrc == source)
                .Result.ToList();

            customer.TotalPaid += orders.Where(x => x.CancelledAt == null && x.FinancialStatus == EcommerceFields.FINANCIAL_PAID).Sum(x => x.TotalPrice);

            var lastOrderDate = DateTime.MinValue;
            var productsBought = new List<Product>();
            foreach (Order order in orders)
            {
                if (DateTime.Compare(lastOrderDate, order.CreatedAt) < 0)
                {
                    lastOrderDate = order.CreatedAt;
                }
                customer.TotalDiscountCodeUsed += order.DiscountCodes.Count;
                if (order.CancelledAt != null)
                {
                    customer.TotalCancelledOrders++;
                }
                if (order.FinancialStatus == EcommerceFields.FINANCIAL_PAID)
                {
                    customer.TotalPayTimes++;
                    if (order.CancelledAt == null && !string.IsNullOrEmpty(order.FulFillmentStatus) && order.FulFillmentStatus == EcommerceFields.FULFILLMENT_FULFILLED)
                    {
                        foreach(LineItem item in order.LineItems)
                        {
                            customer.TotalProducts += item.Quantity;
                            var obj = new Product
                            {
                                Id = item.ProductId.HasValue ? item.ProductId.Value : 0,
                                Name = item.Name
                            };

                            var existProduct = productsBought.FirstOrDefault(x => x.Id == obj.Id && x.Name == obj.Name);
                            if (existProduct == null)
                            {
                                obj.Quantity = item.Quantity;
                                productsBought.Add(obj);
                            }
                            else
                            {
                                existProduct.Quantity += item.Quantity;
                            }
                        }
                    }
                }
            }
            
            customer.AverageOrderValue = Math.Round((customer.TotalSpent / customer.TotalOrders), 2);
            customer.TotalUniqueProducts = productsBought.Count;
            productsBought = productsBought.OrderByDescending(x => x.Quantity).Take(3).ToList();
            customer.BestSelling.AddRange(productsBought);

            customer.RFM = new RFM();
            customer.RFM.MValue = customer.TotalSpent;
            customer.RFM.FValue = customer.TotalOrders;
            customer.RFM.RValue = DateTime.Now.Subtract(lastOrderDate).Days;
        }
    }
}
