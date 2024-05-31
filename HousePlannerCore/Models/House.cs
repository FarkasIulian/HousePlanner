

using System.Collections.Generic;

namespace HousePlannerCore.Models
{
    public  class House
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; }

        public int NumberOfFloors { get; set; }

        public string OwnerEmail { get; set; }
    }
}
