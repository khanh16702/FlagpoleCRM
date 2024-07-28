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
    public class CustomerFieldRepository : ICustomerFieldRepository
    {
        private FlagpoleCRMContext _flagpoleCRM;
        private readonly ILog _log;
        public CustomerFieldRepository(FlagpoleCRMContext flagpoleCRM, ILog log)
        {
            _flagpoleCRM = flagpoleCRM;
            _log = log;
        }

        public List<CustomerField> GetCustomerFields()
        {
            return _flagpoleCRM.CustomerFields.ToList();
        }

        public string GetDataType(string fieldName)
        {
            return _flagpoleCRM.CustomerFields.FirstOrDefault(x => x.KeyName == fieldName).DataType;
        }
    }
}
