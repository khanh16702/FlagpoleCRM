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
    public class TemplateRepository : ITemplateRepository
    {
        public Template GetTemplateById(int id)
        {
            throw new NotImplementedException();
        }

        public List<Template> GetTemplates(string websiteId)
        {
            throw new NotImplementedException();
        }

        public ResponseModel InsertOrUpdate(Template model)
        {
            throw new NotImplementedException();
        }
    }
}
