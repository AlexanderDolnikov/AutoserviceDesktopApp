using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceApp.Models
{
    public class Work
    {
        public int Код { get; set; }
        public int КодЗаказа { get; set; }
        public int КодМастера { get; set; }
        public string Описание { get; set; }
        public decimal Стоимость { get; set; }
        public int КодВидаРаботы { get; set; }
    }
}

