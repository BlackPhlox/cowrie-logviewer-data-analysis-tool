using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRMDataReader.CrmUtilities
{
    static class PrintUtil
    {
            const char _block = '■';
            const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
            const string _twirl = "-\\|/";
            public static void WriteProgressBar(int percent, bool update = false)
            {
                if (update)
                    Console.Write(_back);
                Console.Write("[");
                var p = (int)((percent / 10f) + .5f);
                for (var i = 0; i < 10; ++i)
                {
                    if (i >= p)
                        Console.Write(' ');
                    else
                        Console.Write(_block);
                }
                Console.Write("] {0,3:##0}%", percent);
            }
            public static void WriteProgress(int progress, bool update = false)
            {
                if (update)
                    Console.Write("\b");
                Console.Write(_twirl[progress % _twirl.Length]);
            }

            public static void PrintFillLine(char c)
            {
                Console.WriteLine(new string(c, Console.WindowWidth).Substring(1));
            }

            public static void PrintFillMidLine(string midStr,char c)
            {
                var seb = new string(c, ((Console.WindowWidth / 2) - (midStr.Length - 1) / 2));
                Console.Write(seb);
                Console.Write(midStr);
                Console.WriteLine(seb.Substring(1));
            }

    }
}
