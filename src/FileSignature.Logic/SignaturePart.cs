using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic
{
    public class SignaturePart
    {
        public long PartNumber { get; set; }

        public long TotalParts { get; set; }

        public byte[] Hash { get; set; }
    }
}
