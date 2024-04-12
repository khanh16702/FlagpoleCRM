using FlagpoleCRM.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataServiceLib
{
    public interface ICustomerFieldService
    {
        List<CustomerField> GetCustomerFields();
        string GetDataType(string fieldName);
    }
    public class CustomerFieldService : ICustomerFieldService
    {
        private FlagpoleCRMContext _flagpoleCRM;
        private readonly ILog _log;
        public CustomerFieldService(FlagpoleCRMContext flagpoleCRM, ILog log)
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
