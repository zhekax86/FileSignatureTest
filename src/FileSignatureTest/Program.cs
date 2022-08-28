using CommandLine;
using FileSignature.Logic;
using FileSignatureTest;

internal class Program
{
    private static void Main(string[] args)
    {
        var parserResults = Parser.Default.ParseArguments<FileSignatureOptions>(args)
            .WithParsed(Run);
    }

    private static void Run(FileSignatureOptions options)
    {
        var signatureReader = FileReader.GetSignature(options.InputFile, options.BlockSize);

        foreach(var signaturePart in signatureReader)
        {
            Console.WriteLine($"Part {signaturePart.PartNumber} of {signaturePart.TotalParts} hash code is {FormatHash(signaturePart.Hash)}");
        }
    }

    private static string FormatHash(byte[] hash)
    {
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}