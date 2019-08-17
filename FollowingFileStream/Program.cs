using System;
using System.IO;
using System.Threading.Tasks;

namespace FollowingFileStream
{
    class Program
    {
        private const string InputPath = "source.txt";
        private const string OutputPath = "destination.txt";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var input = WriteInput();
            await CopyToOutput();
            await input;
        }

        private static async Task WriteInput()
        {
            using (var source = new FileStream(InputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var sw = new StreamWriter(source))
            using (var sr = new StreamReader(Console.OpenStandardInput()))
            {
                await sr.CopyToAsync(sw, stopOn:"quit");
            }
        }
        private static async Task CopyToOutput()
        {
            using (var source = new FollowingFileStream(InputPath))
            using (var destination = new FileStream(OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await source.CopyToAsync(destination);
            }
        }
    }

    static public class TextReaderExtensions
    {
        public static async Task CopyToAsync(this TextReader reader, TextWriter writer, string stopOn)
        {
            string line = string.Empty;
            while ((line = await reader.ReadLineAsync().ConfigureAwait(false)) != stopOn)
            {
                await writer.WriteLineAsync(line).ConfigureAwait(false);
                await writer.FlushAsync().ConfigureAwait(false);
            }
        }
    }
}
