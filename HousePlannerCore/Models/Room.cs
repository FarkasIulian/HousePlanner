using System.Collections.Generic;
using System.Drawing;

namespace HousePlannerCore.Models
{
    public class Room
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public long HouseId { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public int Floor { get; set; }
        public Point PositionInHouse { get; set; }
    }
}