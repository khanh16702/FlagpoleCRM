using Common.Models;
using FlagpoleCRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoriesLib
{
    public interface ICampaignRepository
    {
        EmailAccount GetEmailSenderById(int id);
        List<EmailAccount> GetListEmailSender(string websiteId);
        ResponseModel UpdateEmailSender(EmailAccount model);
        Campaign GetCampaignById(int id);
        List<Campaign> GetListCampaign(string websiteId);
        List<Campaign> GetProcessCampaigns();
        ResponseModel UpdateCampaign(Campaign model);
    }
}
