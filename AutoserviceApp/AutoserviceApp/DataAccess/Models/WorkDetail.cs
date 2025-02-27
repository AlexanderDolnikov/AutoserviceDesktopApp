using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceApp.Models
{
    public class WorkDetail
    {
        public int Код { get; set; }
        public int КодРаботы { get; set; }
        public int КодДетали { get; set; }
        public int Количество { get; set; }
    }
}