﻿using Common.Models;
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
    public class TemplateRepository : ITemplateRepository
    {
        private FlagpoleCRMContext _flagpoleCRM;
        private readonly ILog _log;
        public TemplateRepository(FlagpoleCRMContext flagpoleCRM, ILog log)
        {
            _flagpoleCRM = flagpoleCRM;
            _log = log;
        }

        public Template GetTemplateById(int id)
        {
            return _flagpoleCRM.Templates.FirstOrDefault(x => x.Id == id && !x.IsDeleted);
        }

        public List<Template> GetTemplates(string websiteId)
        {
            return _flagpoleCRM.Templates.Where(x => x.WebsiteGuid == websiteId && !x.IsDeleted).ToList();
        }

        public ResponseModel InsertOrUpdate(Template model)
        {
            var response = new ResponseModel { IsSuccessful = true };
            try
            {
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    throw new Exception("Name must not be null or empty");
                }

                if (model.Id == 0)
                {
                    var template = new Template()
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Type = model.Type,
                        Subject = model.Subject,
                        Content = model.Content,
                        WebsiteId = model.WebsiteId,
                        WebsiteGuid = model.WebsiteGuid,
                        CreatedDate = DateTime.UtcNow,
                        IsDeleted = false
                    };
                    _flagpoleCRM.Templates.Add(template);
                    _flagpoleCRM.SaveChanges();
                }
                else
                {
                    var template = _flagpoleCRM.Templates.FirstOrDefault(x => x.Id == model.Id);
                    if (template == null)
                    {
                        throw new Exception("Template could not be found");
                    }
                    template.Name = model.Name;
                    template.Description = model.Description;
                    template.Type = model.Type;
                    template.Subject = model.Subject;
                    template.Content = model.Content;
                    template.IsDeleted = model.IsDeleted;
                    _flagpoleCRM.Templates.Update(template);
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
