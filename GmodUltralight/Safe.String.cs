using ImpromptuNinjas.UltralightSharp;
using System;

namespace GmodUltralight.Safe
{
    public sealed unsafe class String : IDisposable
    {
        internal readonly ImpromptuNinjas.UltralightSharp.String* _;
        public ImpromptuNinjas.UltralightSharp.String* Unsafe => _;
        public String(String* _)
        {
            this._ = _;
        }
        public String(string str)
        {
            _ = ImpromptuNinjas.UltralightSharp.String.Create(str);
        }
        public void Dispose()
        {
            _->Destroy();
        }
    }
}
