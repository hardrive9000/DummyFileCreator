namespace DummyFileCreator.Utils
{
    public static class ConsoleUtility
    {
        private const char block = '■';
        private const string back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";

        public static void WriteProgressBar(uint percent, bool update = false)
        {
            if (update)
                Console.Write(back);

            Console.Write("[");
            uint p = (uint)((percent / 10f) + .5f);

            for (uint i = 0; i < 10; i++)
            {
                if (i >= p)
                    Console.Write(' ');
                else
                    Console.Write(block);
            }

            Console.Write("] {0,3:##0}%", percent);
        }
    }
}
