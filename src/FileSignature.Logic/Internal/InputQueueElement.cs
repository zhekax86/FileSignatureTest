using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic.Internal
{
    public class InputQueueElement
    {
        public long BlockNumber { get; set; }
        public byte[] buffer { get; set; }
        public int bufferLength { get; set; }
    }
}
