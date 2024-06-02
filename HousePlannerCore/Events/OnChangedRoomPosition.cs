using HousePlannerCore.Models;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousePlannerCore.Events
{
    public class OnChangedRoomPosition : PubSubEvent<(long,double,double)>
    {
    }
}
