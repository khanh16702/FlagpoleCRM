using Common.Enums;
using Common.Helper;
using Common.Models;
using PushCustomers.Utilize;
using RestSharp;
using RestSharp.Authenticators;

namespace PushCustomers.Helper
{
    public class RFMHelper
    {
        public static void CalculateRFM(MongoDbHelper<Customer> _mongoDbCustomer, ILogger<PushCustomersWorker> _logger, RestClient client)
        {
            try
            {
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Calculating RFM quintiles");

                var customers = _mongoDbCustomer.Find(x => x.RFM != null).Result.ToList();

                var rvalueList = customers.OrderByDescending(x => x.RFM.RValue).Select(x => x.RFM.RValue).ToList();
                var rvalQuintile = CalculateQuintile(rvalueList);

                var fvalueList = customers.OrderBy(x => x.RFM.FValue).Select(x => x.RFM.FValue).ToList();
                var fvalQuintile = CalculateQuintile(fvalueList);

                var mvalueList = customers.OrderBy(x => x.RFM.MValue).Select(x => x.RFM.MValue).ToList();
                var mvalQuintile = CalculateQuintile(mvalueList);

                foreach (Customer customer in customers)
                {
                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Calculating RFM of customer {customer.Id}");
                    customer.RFM.RScore = CalculateRFMScore(customer.RFM.RValue, rvalQuintile);
                    customer.RFM.FScore = CalculateRFMScore(customer.RFM.FValue, fvalQuintile);
                    customer.RFM.MScore = CalculateRFMScore(customer.RFM.MValue, mvalQuintile);
                    customer.RFM.RFMScore = customer.RFM.RScore * 100 + customer.RFM.FScore * 10 + customer.RFM.MScore;
                    customer.RFM.RFMGroup = GetRFMGroup(customer.RFM.RFMScore);
                    customer.ModifiedDate = DateTime.Now;
                    _mongoDbCustomer.Update(customer.Id, customer);

                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Pushing to Elasticsearch customer {customer.Id}");
                    PushToElastic.PushCustomer(customer, _logger, client);
                }

                var restCustomers = _mongoDbCustomer.Find(x => x.RFM == null).Result.ToList();
                foreach(Customer restCustomer in restCustomers)
                {
                    _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Pushing to Elasticsearch customer {restCustomer.Id}");
                    PushToElastic.PushCustomer(restCustomer, _logger, client);
                }
                _logger.LogInformation($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Done push customers to Elasticsearch");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss")}: Error while calculating RFM: {ex.Message}");
            }
        }

        private static double[] CalculateQuintile(List<double> rfmList)
        {
            double[] quintiles = new double[5];
            for (int i = 0; i < 5; i++)
            {
                int index = (int)Math.Ceiling((i + 1) * (double)rfmList.Count / 5) - 1;
                quintiles[i] = rfmList[index];
            }

            return quintiles;
        }

        private static int CalculateRFMScore(double value, double[] quintiles)
        {
            if (value <= quintiles[0]) return 1;
            if (value <= quintiles[1]) return 2;
            if (value <= quintiles[2]) return 3;
            if (value <= quintiles[3]) return 4;
            return 5;
        }

        private static int GetRFMGroup(int score)
        {
            var champions = new List<int> { 555, 554, 544, 545, 454, 455, 445 };
            var loyal = new List<int> { 543, 444, 435, 355, 354, 345, 344, 335 };
            var potential = new List<int> { 553, 551, 552, 541, 542, 535, 534, 533, 532, 531, 452, 451, 442, 441, 431, 453, 433, 432, 423, 353, 352, 351, 342, 341, 333, 323 };
            var recent = new List<int> { 512, 511, 422, 421, 412, 411, 311 };
            var promising = new List<int> { 525, 524, 523, 522, 521, 515, 514, 513, 425, 424, 413, 414, 415, 315, 314, 313 };
            var needingAttention = new List<int> { 443, 434, 343, 334, 325, 324 };
            var aboutToSleep = new List<int> { 331, 321, 312, 221, 213 };
            var atRisk = new List<int> { 255, 254, 245, 244, 253, 252, 243, 242, 235, 234, 225, 224, 153, 152, 145, 143, 142, 135, 134, 133, 125, 124 };
            var cantLoseThem = new List<int> { 155, 154, 144, 214, 215, 115, 114, 113 };
            var hibernating = new List<int> { 332, 322, 231, 241, 251, 233, 232, 223, 222, 132, 123, 122, 212, 211 };
            var lost = new List<int> { 111, 112, 121, 131, 141, 151 };

            if (champions.Contains(score)) return (int)ERFM.Champions;
            if (loyal.Contains(score)) return (int)ERFM.Loyal;
            if (potential.Contains(score)) return (int)ERFM.Potential;
            if (recent.Contains(score)) return (int)ERFM.Recent;
            if (promising.Contains(score)) return (int)ERFM.Promising;
            if (needingAttention.Contains(score)) return (int)ERFM.NeedingAttention;
            if (aboutToSleep.Contains(score)) return (int)ERFM.AboutToSleep;
            if (atRisk.Contains(score)) return (int)ERFM.AtRisk;
            if (cantLoseThem.Contains(score)) return (int)ERFM.CantLoseThem;
            if (hibernating.Contains(score)) return (int)ERFM.Hibernating;
            else return (int)ERFM.Lost;
        }
    }
}
