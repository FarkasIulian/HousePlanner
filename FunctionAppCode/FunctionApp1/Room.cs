using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class Room
    {
        public long Id { get; set; } = 0;
        public string Name { get; set; }
        public long HouseId { get; set; }
        public int Width { get; set; }
        public int Length { get; set; }
        public int Floor { get; set; }
        public Point PositionInHouse { get; set; }

    }
}
