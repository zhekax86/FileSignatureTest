using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSignatureTest
{
    internal class FileSignatureOptions
    {
        [Value(0, MetaName = "input file", HelpText = "File to calculate signature", Required = true)]
        public string InputFile { get; set; }

        [Option('b', "blockSize", HelpText = "Signature block size", Required = true )]
        public int BlockSize { get; set; }
    }
}
