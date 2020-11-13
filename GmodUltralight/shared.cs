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
        int Ultralight_Update(ILua lua)
        {
            renderer.Update();
            return 0;
        }
        int Ultralight_Render(ILua lua)
        {
            renderer.Render();
            return 0;
        }

        int Ultralight_createView(ILua lua)
        {
            uint width = (uint)lua.GetNumber(1);
            uint height = (uint)lua.GetNumber(2);
            bool transparent = lua.GetBool(3);
            string viewID = Guid.NewGuid().ToString();

            if (!views.ContainsKey(viewID))
            {
                View view = new View(renderer, width, height, transparent, renderer.GetDefaultSession());
                views.Add(viewID, view);
                PrintToConsole(lua, "UL: Created View");
                lua.PushString(viewID);
            }
            else
            {
                lua.PushNil();
            }

            return 1;
        }
        int UltralightView_GetPixel(ILua lua)
        {
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
        }
        int UltralightView_EvaluateScript(ILua lua)
        {
            string viewID = lua.GetString(1);
            View view = views[viewID];
            string toevaluate = lua.GetString(2);

            string result = view.EvaluateScript(toevaluate);
            lua.PushString(result);
            return 1;
        }
        int UltralightView_LoadURL(ILua lua)
        {
            string viewID = lua.GetString(1);
            View view = views[viewID];
            string url = lua.GetString(2);
            view.LoadUrl(url);

            return 0;
        }
        int UltralightView_LoadHTML(ILua lua)
        {
            string viewID = lua.GetString(1);
            View view = views[viewID];
            string html = lua.GetString(2);
            view.LoadHtml(html);
            return 0;
        }
        int UltralightView_UpdateUntilLoads(ILua lua)
        {
            string viewID = lua.GetString(1);
            View view = views[viewID];
            bool loaded = false;
            void finishcallback(IntPtr data, View caller, ulong frameId, bool isMainFrame, string url)
            {
                loaded = true;
            }
            view.SetFinishLoadingCallback(finishcallback, default);
            uint timeout = 0;
            while (!loaded && timeout < 1000000)
            {
                renderer.Update();
                timeout++;
            }
            renderer.Render();
            Bitmap bitmap = view.GetSurface().GetBitmap();

            bitmap.WritePng("csresult.png");

            return 0;
        }
        int UltralightView_IsValid(ILua lua)
        {
            string viewID = lua.GetString(1);
            lua.PushBool(views.ContainsKey(viewID));
            return 1;
        }

        public void LoadShared(ILua lua)
        {
            lua.PushManagedFunction(Ultralight_Update);
            lua.SetField(-2, "Update");

            lua.PushManagedFunction(Ultralight_Render);
            lua.SetField(-2, "Render");

            lua.PushManagedFunction(Ultralight_createView);
            lua.SetField(-2, "CreateView");

            lua.PushManagedFunction(UltralightView_GetPixel);
            lua.SetField(-2, "View_GetPixel");

            lua.PushManagedFunction(UltralightView_EvaluateScript);
            lua.SetField(-2, "View_EvaluateScript");

            lua.PushManagedFunction(UltralightView_LoadURL);
            lua.SetField(-2, "View_LoadURL");

            lua.PushManagedFunction(UltralightView_LoadHTML);
            lua.SetField(-2, "View_LoadHTML");

            lua.PushManagedFunction(UltralightView_UpdateUntilLoads);
            lua.SetField(-2, "View_UpdateUntilLoads");

            lua.PushManagedFunction(UltralightView_IsValid);
            lua.SetField(-2, "View_IsValid");
        }
    }
}
