using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.Order.Event
{
    public interface IOrderFinishedEvent
    {
        public Guid OrderId { get; set; }
    }
}
