using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messages.Order.Command
{
    public interface ICancelOrderCommand
    {
        public Guid OrderId { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
