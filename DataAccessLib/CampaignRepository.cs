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

namespace DataAccessLib
{
    public class CampaignRepository : ICampaignRepository
    {
        private FlagpoleCRMContext _flagpoleCRM;
        private readonly ILog _log;
        public CampaignRepository(FlagpoleCRMContext flagpoleCRM, ILog log)
        {
            _flagpoleCRM = flagpoleCRM;
            _log = log;
        }

        public Campaign GetCampaignById(int id)
        {
            return _flagpoleCRM.Campaigns.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }

        public EmailAccount GetEmailSenderById(int id)
        {
            return _flagpoleCRM.EmailAccounts.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }

        public List<Campaign> GetListCampaign(string websiteId)
        {
            return _flagpoleCRM.Campaigns.Where(x => x.WebsiteGuid == websiteId && !x.IsDeleted).ToList();
        }

        public List<EmailAccount> GetListEmailSender(string websiteId)
        {
            return _flagpoleCRM.EmailAccounts.Where(x => x.WebsiteId == websiteId && !x.IsDeleted).ToList();
        }

        public List<Campaign> GetProcessCampaigns()
        {
            return _flagpoleCRM.Campaigns
                .Where(x => x.SendDate.HasValue
                    && DateTime.Compare(x.SendDate.Value, DateTime.UtcNow) <= 0
                    && x.SendStatus == (int)ESendStatus.Waiting
                    && !x.IsDeleted)
                .OrderBy(x => x.Id)
                .ToList();
        }

        public ResponseModel UpdateCampaign(Campaign model)
        {
            var response = new ResponseModel() { IsSuccessful = true };
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    throw new Exception("Name cannot be left blank");
                }
                if (model.Id == 0)
                {
                    var campaign = new Campaign
                    {
                        Name = model.Name,
                        Description = model.Description,
                        WebsiteGuid = model.WebsiteGuid,
                        WebsiteId = model.WebsiteId,
                        SendStatus = (int)ESendStatus.Waiting,
                        SendDate = model.SendDate,
                        SendType = model.SendType,
                        SenderName = model.SenderName,
                        EmailId = model.EmailId,
                        PhoneId = model.PhoneId,
                        Channel = model.Channel,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false,
                        TemplateId = model.TemplateId,
                        AudienceId = model.AudienceId
                    };
                    _flagpoleCRM.Campaigns.Add(campaign);
                    _flagpoleCRM.SaveChanges();
                }
                else
                {
                    var campaign = GetCampaignById(model.Id);
                    if (campaign == null)
                    {
                        throw new Exception("Campaign not found");
                    }
                    campaign.Name = model.Name;
                    campaign.Description = model.Description;
                    campaign.WebsiteGuid = model.WebsiteGuid;
                    campaign.WebsiteId = model.WebsiteId;
                    campaign.SendStatus = model.SendStatus;
                    campaign.SendDate = model.SendDate;
                    campaign.SendType = model.SendType;
                    campaign.SenderName = model.SenderName;
                    campaign.EmailId = model.EmailId;
                    campaign.PhoneId = model.PhoneId;
                    campaign.Channel = model.Channel;
                    campaign.ModifiedDate = DateTime.UtcNow;
                    campaign.IsDeleted = model.IsDeleted;
                    campaign.TemplateId = model.TemplateId;
                    campaign.AudienceId = model.AudienceId;
                    _flagpoleCRM.Campaigns.Update(campaign);
                    _flagpoleCRM.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public ResponseModel UpdateEmailSender(EmailAccount model)
        {
            var response = new ResponseModel { IsSuccessful = true };
            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                response.IsSuccessful = false;
                response.Message = "Email or password cannot be left blank";
            }
            try
            {
                if (model.Id == 0)
                {
                    var emailAccount = new EmailAccount
                    {
                        Email = model.Email,
                        Password = model.Password,
                        WebsiteId = model.WebsiteId,
                        IsDeleted = false,
                        CreatedDate = DateTime.UtcNow
                    };
                    _flagpoleCRM.EmailAccounts.Add(emailAccount);
                    _flagpoleCRM.SaveChanges();
                }
                else
                {
                    var emailAccount = GetEmailSenderById(model.Id);
                    if (emailAccount == null)
                    {
                        throw new Exception("Email not found");
                    }
                    emailAccount.Email = model.Email;
                    emailAccount.Password = model.Password;
                    emailAccount.WebsiteId = model.WebsiteId;
                    emailAccount.IsDeleted = model.IsDeleted;
                    _flagpoleCRM.EmailAccounts.Update(emailAccount);
                    _flagpoleCRM.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                response.IsSuccessful = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
