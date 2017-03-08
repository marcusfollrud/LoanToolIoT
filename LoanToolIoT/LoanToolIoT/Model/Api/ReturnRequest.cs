using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Api
{
    public sealed class ReturnRequest
    {
        public string SerialNumber { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
