using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net.Attributes;
namespace LoanToolIoT.Model.Sql
{
    public sealed class DeviceHostList
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string IPRange { get; set; }
        public int StartPort { get; set; }
        public int EndPort { get; set; }
    }
}
