using GmodNET.API;
using ImpromptuNinjas.UltralightSharp.Safe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GmodUltralight
{
    partial class GmodUltralight
    {
        CFuncManagedDelegate UltralightView_SH_GetPixel;
        CFuncManagedDelegate UltralightView_SH_EvaluateScript;
        public void LoadShared(ILua lua)
        {
            UltralightView_SH_GetPixel = (lua_state) =>
                   {
                       ILua lua = GmodInterop.GetLuaFromState(lua_state);
                       string viewID = lua.GetString(1);
                       uint x = (uint)lua.GetNumber(2);
                       uint y = (uint)lua.GetNumber(3);
                       View view = views[viewID];

                       // TODO: some basic checks

                       Bitmap bitmap = view.GetSurface().GetBitmap();
                       try
                       {
                           unsafe
                           {
                               byte* pixels = (byte*)bitmap.LockPixels();
                               long index = y * bitmap.GetRowBytes();
                               index += x * 4;
                               byte a = pixels[index + 3];
                               byte r = pixels[index + 2];
                               byte g = pixels[index + 1];
                               byte b = pixels[index];
                               lua.PushNumber(a);
                               lua.PushNumber(r);
                               lua.PushNumber(g);
                               lua.PushNumber(b);
                           }
                       }
                       finally
                       {
                           bitmap.UnlockPixels();
                       }
                       return 4;
                   };
            lua.PushCFunction(UltralightView_SH_GetPixel);
            lua.SetField(-1, "View_SH_GetPixel");

            UltralightView_SH_EvaluateScript = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                string viewID = lua.GetString(1);
                View view = views[viewID];
                string toevaluate = lua.GetString(2);

                string result = view.EvaluateScript(toevaluate);
                lua.PushString(result);
                return 1;
            };

            lua.PushCFunction(UltralightView_SH_EvaluateScript);
            lua.SetField(-1, "View_SH_EvaluateScript");
        }
    }
}
