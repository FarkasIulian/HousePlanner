using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousePlannerCore.Events
{
    public class OnRightClickSendPoint : PubSubEvent<Point>
    {
    }
}
