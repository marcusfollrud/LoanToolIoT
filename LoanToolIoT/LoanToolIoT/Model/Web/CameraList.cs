using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanToolIoT.Model.Web
{
    public sealed class CameraList
    {
        public string mac { get; set; }
        public string model { get; set; }
        public bool available { get; set; }
    }
}
