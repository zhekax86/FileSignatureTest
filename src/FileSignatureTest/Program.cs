using CommandLine;
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

    }
}