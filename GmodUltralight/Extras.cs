using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GmodNET.Extras
{
    public static class Tier0
    {
        [DllImport("tier0", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Msg(char[] message);
    }
    /// <summary>
    /// <warning>
    /// PROPER GC needed here
    /// </warning>
    /// </summary>
    class Extras
    {
        private class ConsoleWriter : TextWriter
        {
            public override Encoding Encoding => throw new NotImplementedException();
            public override string NewLine { get => "\n"; }
            public override void Write(char[] buffer)
            {
                Tier0.Msg(buffer);
            }
        }
        static ConsoleWriter msgWriter;
        public static void Init()
        {
            msgWriter = new ConsoleWriter();
            Console.SetOut(msgWriter);
        }
    }
}
