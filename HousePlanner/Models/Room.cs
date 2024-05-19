using System.Collections.Generic;
using System.Drawing;

namespace HousePlanner.Models
{
    public class Room
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; }
        public int Floor { get; set; }
        public Point PositionInHouse { get; set; }
        public List<Furniture> StuffInRoom { get; set; }
    }
}