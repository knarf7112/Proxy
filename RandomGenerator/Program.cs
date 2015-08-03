using System;

namespace RandomGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] expected = new byte[] { 0x8E, 0xEB, 0x6A, 0x5E, 0x1E, 0xE9, 0xB3, 0x14, 
                                           0x71, 0xC2, 0xE9, 0x96, 0xC3, 0x92, 0xF9, 0x43 };
            SessionKeyGenerator rng = new SessionKeyGenerator();
            byte[] specifiedBytes = rng.GetRanA(11);

            Console.WriteLine("start compare byte array ...");
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != specifiedBytes[i])
                {
                    Console.WriteLine("錯誤索引(" + i + ")預期陣列值:" + expected[i] + " != 指定陣列值:" + specifiedBytes[i]);
                }
            }
            Console.WriteLine("Finished compare bytes!");

            //rng.WriteFile(4096);//產生隨機檔案,用來複製的
            Console.ReadKey();
        }
    }
}
