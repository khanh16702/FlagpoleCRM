using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class SmtpDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string SMTPServer { get; set; }
        public int Port { get; set; }
    }
}
