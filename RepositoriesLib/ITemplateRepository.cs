using Common.Models;
using FlagpoleCRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoriesLib
{
    public interface ITemplateRepository
    {
        List<Template> GetTemplates(string websiteId);
        Template GetTemplateById(int id);
        ResponseModel InsertOrUpdate(Template model);
    }
}
