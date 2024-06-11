using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class House
    {

        public long Id { get; set; } = 0;
        public string Name { get; set; }

        public int NumberOfFloors { get; set; }

        public string OwnerEmail { get; set; }

    }
}
