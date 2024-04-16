using Common.Helper;
using Common.Models;
using RestSharp;

namespace PushCustomers.Utilize
{
    public class PushToElastic
    {
        public static void PushCustomer(Customer customer, ILogger<PushCustomersWorker> _logger, RestClient client)
        {
            var pushElasticResponse = new ResponseModel();
            var elasticCustomer = new ElasticCustomer();
            foreach (var prop in typeof(ElasticCustomer).GetProperties())
            {
                var value = prop.GetValue(customer, null);
                prop.SetValue(elasticCustomer, value);
            }

            var existCustomer = ElasticHelper.SearchById(customer.Id.ToString(), client, "flagpolecrm.customer");
            if (!string.IsNullOrEmpty(existCustomer.Error))
            {
                pushElasticResponse = ElasticHelper.InsertDocument(elasticCustomer, customer.Id.ToString(), "flagpolecrm.customer", client);
            }
            else
            {
                pushElasticResponse = ElasticHelper.UpdateDocument(elasticCustomer, customer.Id.ToString(), "flagpolecrm.customer", client);
            }
            if (!pushElasticResponse.IsSuccessful)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Error while pushing to Elastic: {pushElasticResponse.Message}");
            }
        }
    }
}
