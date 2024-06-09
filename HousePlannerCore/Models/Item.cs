using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousePlannerCore.Models
{
    public class Item
    {
        public long Id { get; set; } 
        public string Name { get; set; }

        public long FurnitureId { get; set; }
    }
}
