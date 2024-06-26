﻿using System.Drawing;

namespace HousePlannerCore.Models
{
    public class Furniture
    {
        public long Id { get; set; }
        public long RoomId { get; set; }
        public string Name { get; set; }
        public float Width { get; set; }
        public float Length { get; set; }
        public Point PositionInRoom { get; set; }
        public string Picture { get; set; }
    }
}