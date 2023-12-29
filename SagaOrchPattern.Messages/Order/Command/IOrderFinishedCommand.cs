using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.Order.Command
{
    public interface IFinalizeOrderCommand
    {
        public Guid OrderId { get; set; }
    }
}
