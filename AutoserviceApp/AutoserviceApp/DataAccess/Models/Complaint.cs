using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceApp.Models
{
    public class Complaint
    {
        public int Код { get; set; }
        public int КодРаботы { get; set; }
        public string Описание { get; set; }
        public DateTime Дата { get; set; }
    }
}