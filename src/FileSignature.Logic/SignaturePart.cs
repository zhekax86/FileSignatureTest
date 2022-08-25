using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignature.Logic
{
    public class SignaturePart
    {
        public int PartNumber { get; set; }

        public int TotalParts { get; set; }

        public byte[] Hash { get; set; }
    }
}
