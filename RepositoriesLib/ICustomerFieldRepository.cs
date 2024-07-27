using FlagpoleCRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoriesLib
{
    public interface ICustomerFieldRepository
    {
        List<CustomerField> GetCustomerFields();
        string GetDataType(string fieldName);
    }
}
