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
    public class UnsubscribeRepository : IUnsubscribeRepository
    {
        private FlagpoleCRMContext _flagpoleCRM;
        private readonly ILog _log;
        public UnsubscribeRepository(FlagpoleCRMContext flagpoleCRM, ILog log)
        {
            _flagpoleCRM = flagpoleCRM;
            _log = log;
        }

        public List<UnsubcribedEmail> GetListUnsubs(string websiteId)
        {
            return _flagpoleCRM.UnsubcribedEmails.Where(x => x.WebsiteId == websiteId && !x.IsDeleted).ToList();
        }

        public UnsubcribedEmail GetUnsubByEmail(string email)
        {
            return _flagpoleCRM.UnsubcribedEmails.FirstOrDefault(x => x.Email == email && !x.IsDeleted);
        }

        public UnsubcribedEmail GetUnsubById(int id)
        {
            return _flagpoleCRM.UnsubcribedEmails.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }

        public ResponseModel UpdateUnsubscribe(UnsubcribedEmail model)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email))
                {
                    throw new Exception("Email not found");
                }
                if (model.Id == 0)
                {
                    if (GetUnsubByEmail(model.Email) != null)
                    {
                        throw new Exception("Email has already unsubscribed");
                    }
                    var unsub = new UnsubcribedEmail
                    {
                        Email = model.Email,
                        WebsiteId = model.WebsiteId,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    _flagpoleCRM.UnsubcribedEmails.Add(unsub);
                    _flagpoleCRM.SaveChanges();
                }
                else
                {
                    var unsub = GetUnsubById(model.Id);
                    if (unsub == null)
                    {
                        throw new Exception("This email has not unsubscribed");
                    }
                    unsub.IsDeleted = model.IsDeleted;
                    unsub.ModifiedDate = DateTime.UtcNow;
                    _flagpoleCRM.UnsubcribedEmails.Update(unsub);
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
