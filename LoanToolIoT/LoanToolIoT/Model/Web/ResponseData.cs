using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Web
{
    public sealed class ResponseData
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public int Code { get; set; }
    }
}
