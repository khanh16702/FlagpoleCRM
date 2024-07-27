using Common.Models;
using FlagpoleCRM.Models;
using RepositoriesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLib
{
    public class CampaignRepository : ICampaignRepository
    {
        public Campaign GetCampaignById(int id)
        {
            throw new NotImplementedException();
        }

        public EmailAccount GetEmailSenderById(int id)
        {
            throw new NotImplementedException();
        }

        public List<Campaign> GetListCampaign(string websiteId)
        {
            throw new NotImplementedException();
        }

        public List<EmailAccount> GetListEmailSender(string websiteId)
        {
            throw new NotImplementedException();
        }

        public List<Campaign> GetProcessCampaigns()
        {
            throw new NotImplementedException();
        }

        public ResponseModel UpdateCampaign(Campaign model)
        {
            throw new NotImplementedException();
        }

        public ResponseModel UpdateEmailSender(EmailAccount model)
        {
            throw new NotImplementedException();
        }
    }
}
