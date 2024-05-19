using System.Drawing;

namespace HousePlanner.Models
{
    public class Furniture
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; }
        public string Description { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public Point PositionInRoom { get; set; }
        public string Picture { get; set; }
    }
}