using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousePlannerCore.Events
{
    public class OnChangedFurniturePosition : PubSubEvent<(long, double, double)>
    {
    }
}
