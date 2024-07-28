using Common.Enums;
using Common.Models;
using FlagpoleCRM.Models;
using log4net;
using RepositoriesLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceLib
{
    public interface ICampaignService
    {
        EmailAccount GetEmailSenderById (int id);
        List<EmailAccount> GetListEmailSender(string websiteId);
        ResponseModel UpdateEmailSender(EmailAccount model);
        Campaign GetCampaignById(int id);
        List<Campaign> GetListCampaign(string websiteId);
        List<Campaign> GetProcessCampaigns();
        ResponseModel UpdateCampaign(Campaign model);
    }
    public class CampaignService : ICampaignService
    {
        private readonly ICampaignRepository _campaignRepository;
        public CampaignService(ICampaignRepository campaignRepository)
        {
            _campaignRepository = campaignRepository;
        }

        public EmailAccount GetEmailSenderById(int id)
        {
            return _campaignRepository.GetEmailSenderById(id);
        }

        public List<EmailAccount> GetListEmailSender(string websiteId)
        {
            return _campaignRepository.GetListEmailSender(websiteId);
        }

        public ResponseModel UpdateEmailSender(EmailAccount model)
        {
            return _campaignRepository.UpdateEmailSender(model);
        }

        public Campaign GetCampaignById(int id)
        {
            return _campaignRepository.GetCampaignById(id);
        }

        public List<Campaign> GetListCampaign(string websiteId)
        {
            return _campaignRepository.GetListCampaign(websiteId);
        }

        public List<Campaign> GetProcessCampaigns()
        {
            return _campaignRepository.GetProcessCampaigns();
        }

        public ResponseModel UpdateCampaign(Campaign model)
        {
            return _campaignRepository.UpdateCampaign(model);
        }
    }
}
