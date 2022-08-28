using CommandLine;

namespace FileSignatureTest
{
    internal class FileSignatureOptions
    {
        [Value(0, MetaName = "input file", HelpText = "File to generate signature", Required = true)]
        public string FileName { get; set; }

        [Option('b', "blockSize", HelpText = "Signature block size", Required = true )]
        public int BlockSize { get; set; }
    }
}
