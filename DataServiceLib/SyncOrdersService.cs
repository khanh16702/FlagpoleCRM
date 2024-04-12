using Common.Helper;
using Common.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DataServiceLib
{
    public class SyncOrdersService
    {
        public static ResponseModel Insert(OrderRaw orderRaw, string websiteId, string source, MongoDbHelper<Order> mongoDb)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                var order = new Order()
                {
                    WebsiteId = websiteId.Replace("-",""),
                    OrgSrc = source
                };

                foreach (var prop in typeof(OrderRaw).GetProperties())
                {
                    var value = prop.GetValue(orderRaw, null);
                    prop.SetValue(order, value);
                }

                mongoDb.Insert(order);
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public static ResponseModel Update(Order order, OrderRaw model, MongoDbHelper<Order> mongoDb)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                foreach (var prop in typeof(OrderRaw).GetProperties())
                {
                    var value = prop.GetValue(model, null);
                    prop.SetValue(order, value);
                }
                mongoDb.Update(order.Id, order);
            }
            catch(Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
