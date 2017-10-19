using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Messages
{
    public class UpdateProgressMessage
    {
        public int Current { get; set; }
        public int Total { get; set; }

        public UpdateProgressMessage(int total, int current)
        {
            Current = current;
            Total = total;
        }

    }
}
