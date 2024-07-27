using FlagpoleCRM.Models;
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
        public List<CustomerField> GetCustomerFields()
        {
            throw new NotImplementedException();
        }

        public string GetDataType(string fieldName)
        {
            throw new NotImplementedException();
        }
    }
}
