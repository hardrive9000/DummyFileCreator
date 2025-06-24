using Cocona;
using System.Security.Cryptography;
using DummyFileCreator.Models;
using DummyFileCreator.Utils;

namespace DummyFileCreator
{
    class Program
    {
        #region Constants

        private const ulong DEFAULT_CHUNK_SIZE = 4194304L;
        private readonly string WELCOME_MESSAGE = $"+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+{Environment.NewLine}|D|u|m|m|y|F|i|l|e|C|r|e|a|t|o|r|{Environment.NewLine}+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+{Environment.NewLine}v0.1 by hardrive9000 (High Evolutionary | Alto Evolucionario)";

        #endregion

        static void Main(string[] args)
        {
            CoconaLiteApp.Run<Program>(args);
        }

        #region Public Methods

        public void Dummy([Option('f')] string filePath, [Option('s')] ulong size, [Option('u')] Units unit, [Option('r')] bool random)
        {
            ShowWelcomeMessage();
            if (IsValidFilePath(filePath))
            {
                if (!File.Exists(filePath))
                {
                    try
                    {
                        CreateDummyFile(filePath, GetFileSizeInBytes(size, unit), random);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($" Exception: {ex.Message}");
                    }
                }
                else
                    Console.WriteLine($"File already exists: {filePath}");
            }
            else
                Console.WriteLine($"Invalid path/filename: {filePath}");
        }

        public void DummyBatch([Option('f')] string filePath)
        {
            ShowWelcomeMessage();
            if (File.Exists(filePath))
            {
                string? line;
                string[] content;
                uint i = 0;

                using StreamReader sr = new(filePath);

                while ((line = sr.ReadLine()) != null)
                {
                    if (!line.TrimStart().StartsWith('#'))
                    {
                        content = line.Trim().Split('\t');

                        if ((content.Length > 2) && byte.TryParse(content[2], out byte random) && ulong.TryParse(content[1], out ulong sizeInBytes))
                        {
                            if (IsValidFilePath(content[0]))
                            {
                                if (!File.Exists(content[0]))
                                {
                                    try
                                    {
                                        CreateDummyFile(content[0], sizeInBytes, random == 1);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($" Exception: {ex.Message} Line {i}");
                                    }
                                }
                                else
                                    Console.WriteLine($"File already exists: {content[0]} Line {i}");
                            }
                            else
                                Console.WriteLine($"Invalid path/filename: {content[0]} Line {i}");
                        }
                        else
                            Console.WriteLine($"Invalid line {i + 1}: {line}");
                    }
                    i++;
                }
            }
            else
                Console.WriteLine("File not found");
        }

        #endregion

        #region Private Methods

        private void ShowWelcomeMessage()
        {
            Console.WriteLine(WELCOME_MESSAGE);
            Console.WriteLine();
        }
        private static bool IsValidFilePath(string filePath)
        {
            bool valid = false;
            string? path = Path.GetDirectoryName(filePath);
            string? name = Path.GetFileName(filePath);

            if (!string.IsNullOrEmpty(name))
            {
                valid = true;
                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        Console.Write($"Exception: {ex.Message} creating path {path} for file {name} ");
                        valid = false;
                    }
                }
            }

            return valid;
        }

        private static ulong GetFileSizeInBytes(ulong size, Units unit)
        {
            return unit switch
            {
                Units.KB => size * 1024L,
                Units.MB => size * 1024L * 1024L,
                Units.GB => size * 1024L * 1024L * 1024L,
                _ => size
            };
        }

        private static void CreateDummyFile(string filePath, ulong sizeInBytes, bool random)
        {
            uint totalChunks = (uint)Math.Ceiling((double)sizeInBytes / DEFAULT_CHUNK_SIZE);
            uint i = 1;
            byte[] data;

            Console.Write($"Creating file {filePath} ");
            ConsoleUtility.WriteProgressBar(0);

            using FileStream stream = File.OpenWrite(filePath);
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();

            if (totalChunks > 1)
            {
                data = new byte[DEFAULT_CHUNK_SIZE];

                if (random)
                {
                    while (sizeInBytes > DEFAULT_CHUNK_SIZE)
                    {
                        rng.GetBytes(data);
                        stream.Write(data, 0, data.Length);
                        stream.Flush(true);
                        sizeInBytes -= DEFAULT_CHUNK_SIZE;
                        ConsoleUtility.WriteProgressBar(i * 100 / totalChunks, true);
                        i++;
                    }
                }
                else
                {
                    Span<byte> span = new(data);
                    span.Clear();

                    while (sizeInBytes > DEFAULT_CHUNK_SIZE)
                    {
                        stream.Write(data, 0, data.Length);
                        stream.Flush(true);
                        sizeInBytes -= DEFAULT_CHUNK_SIZE;
                        ConsoleUtility.WriteProgressBar(i * 100 / totalChunks, true);
                        i++;
                    }
                }

                stream.Seek(0, SeekOrigin.End);
            }

            data = new byte[sizeInBytes];

            if (random)
            {
                rng.GetBytes(data);
                stream.Write(data, 0, data.Length);
                stream.Flush(true);
                ConsoleUtility.WriteProgressBar(i * 100 / totalChunks, true);
            }
            else
            {
                Span<byte> span = new(data);
                span.Clear();
                stream.Write(data, 0, data.Length);
                stream.Flush(true);
                ConsoleUtility.WriteProgressBar(i * 100 / totalChunks, true);
            }

            Console.WriteLine();
        }

        #endregion
    }
}
