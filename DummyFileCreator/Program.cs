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
                uint i = 1;

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
                            Console.WriteLine($"Invalid line {i}: {line}");
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
            uint currentChunk = 0;

            int bytesToWrite;

            byte[] buffer = new byte[DEFAULT_CHUNK_SIZE];

            Console.Write($"Creating file {filePath} ");
            ConsoleUtility.WriteProgressBar(0);

            using FileStream stream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None, (int)DEFAULT_CHUNK_SIZE, FileOptions.SequentialScan);
            stream.SetLength((long)sizeInBytes);
            stream.Position = 0;

            using RandomNumberGenerator? rng = random ? RandomNumberGenerator.Create() : null;

            if (!random)
                Array.Clear(buffer);

            while (sizeInBytes > 0)
            {
                bytesToWrite = (int)Math.Min(sizeInBytes, DEFAULT_CHUNK_SIZE);

                if (random)
                {
                    if (bytesToWrite == (int)DEFAULT_CHUNK_SIZE)
                    {
                        rng!.GetBytes(buffer);
                    }
                    else
                    {
                        Span<byte> finalBuffer = buffer.AsSpan(0, bytesToWrite);
                        rng!.GetBytes(finalBuffer);
                    }
                }

                stream.Write(buffer.AsSpan(0, bytesToWrite));

                sizeInBytes -= (ulong)bytesToWrite;
                currentChunk++;

                if (currentChunk % 10 == 0 || sizeInBytes == 0)
                    ConsoleUtility.WriteProgressBar(currentChunk * 100 / totalChunks, true);
            }

            stream.Flush();
            Console.WriteLine();
        }

        #endregion
    }
}
