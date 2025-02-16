using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoserviceApp.Models.TaskQueries
{
    public class GroupedDetail
    {
        public string DetailName { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalCost { get; set; } 
    }

}
