using System;
using System.IO;
using System.Threading.Tasks;

namespace Manandre.IO
{
    static class Program
    {
        private const string InputPath = "source.txt";
        private const string OutputPath = "destination.txt";

        static async Task Main()
        {
            var input = WriteInput().ConfigureAwait(false);
            await CopyToOutput().ConfigureAwait(false);
            await input;
        }

        private static async Task WriteInput()
        {
            using (var sw = new StreamWriter(new FileStream(InputPath, FileMode.Create, FileAccess.Write, FileShare.Read)))
            using (var sr = new StreamReader(Console.OpenStandardInput()))
            {
                await sr.CopyToAsync(sw, stopOn:"quit").ConfigureAwait(false);
            }
        }
        private static async Task CopyToOutput()
        {
            using (var source = new FollowingFileStream(InputPath))
            using (var destination = new FileStream(OutputPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                await source.CopyToAsync(destination).ConfigureAwait(false);
            }
        }
    }

    static class TextReaderExtensions
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
