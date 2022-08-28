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
        try
        {
            ReadSignature(options.FileName, options.BlockSize);
        }
        catch (Exception exception)
        {
            Console.WriteLine("An error has occured while calculation file signature");
            Console.WriteLine(exception.Message);
            Console.WriteLine(exception.StackTrace);
        }
    }

    private static void ReadSignature(string fileName, int blockSize)
    {
        var signatureReader = FileReader.GetSignature(fileName, blockSize);

        foreach (var signaturePart in signatureReader)
        {
            Console.WriteLine($"Part {signaturePart.PartNumber} of {signaturePart.TotalParts} hash code is {FormatHash(signaturePart.Hash)}");
        }
    }

    private static string FormatHash(byte[] hash)
    {
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }
}