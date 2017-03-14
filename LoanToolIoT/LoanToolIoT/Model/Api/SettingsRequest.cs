using LoanToolIoT.Model.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Api
{
    public sealed class SettingsRequest
    {
        public ApiRequest API { get; set; }
        public Settings Setting {get;set; }
    }
}
