using ResxFinder.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ResxFinder.Messages
{
    public class UpdateParserCheckBoxesMessage
    {
        public FileParser Parser { get; private set; }

        public UpdateParserCheckBoxesMessage(FileParser parser)
        {
            Parser = parser;
        }
    }
}
