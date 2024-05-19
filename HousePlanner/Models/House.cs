

using System.Collections.Generic;

namespace HousePlanner.Models
{
    public  class House
    {
        public long Id { get; set; } = 0;

        public List<Room> Rooms { get; set; }

        public int NumberOfFloors { get; set; }

        public string OwnerEmail { get; set; }
    }
}
