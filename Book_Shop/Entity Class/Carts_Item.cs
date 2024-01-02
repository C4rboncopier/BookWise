using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Book_Shop.Entity_Class
{
    public class Carts_Item
    {
        public int userId { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public string category {  get; set; }
        public decimal price { get; set; }
        public int amount { get; set; }
        public decimal total { get; set; }
    }
}
