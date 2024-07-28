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
    public interface ICustomerFieldService
    {
        List<CustomerField> GetCustomerFields();
        string GetDataType(string fieldName);
    }
    public class CustomerFieldService : ICustomerFieldService
    {
        private readonly ICustomerFieldRepository _customerFieldRepository;
        public CustomerFieldService(ICustomerFieldRepository customerFieldRepository, ILog log)
        {
            _customerFieldRepository = customerFieldRepository;
        }

        public List<CustomerField> GetCustomerFields()
        {
            return _customerFieldRepository.GetCustomerFields();
        }

        public string GetDataType(string fieldName)
        {
            return _customerFieldRepository.GetDataType(fieldName);
        }
    }
}
