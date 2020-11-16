using GmodNET.API;
using ImpromptuNinjas.UltralightSharp.Safe;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
                Console.WriteLine("UL: Created View");
                views.Add(viewID, view);
                View_Type result = new View_Type(viewID);
                //IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(result));
                //Marshal.StructureToPtr(result, p, false);
                lua.PushUserType(GCHandle.ToIntPtr(GCHandle.Alloc(result, GCHandleType.Weak)), View_TypeId);
            }
            else
            {
                lua.PushNil();
            }

            return 1;
        }
        int ulView_meta__tostring(ILua lua)
        {
            View_Type viewType = (View_Type)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            string viewID = viewType.id;
            lua.PushString(viewID);
            return 1;
        }
        /// <summary>
        /// Called after lua's "collectgarbage"
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        int ulView_meta__gc(ILua lua)
        {
            GCHandle gchandle = GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId));
            View_Type viewType = (View_Type)gchandle.Target;

            views.Remove(viewType.id);
            Console.WriteLine($"UL: view {viewType.id} is garbage collected");
            gchandle.Free();

            return 0;
        }
        int UltralightView_GetPixel(ILua lua)
        {
            View_Type viewType = (View_Type)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            string viewID = viewType.id;
            View view = views[viewID];

            uint x = (uint)lua.GetNumber(2);
            uint y = (uint)lua.GetNumber(3);
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
            View_Type viewType = (View_Type)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            string viewID = viewType.id;
            View view = views[viewID];
            string toevaluate = lua.GetString(2);
            string result = view.EvaluateScript(toevaluate);
            lua.PushString(result);
            return 1;
        }
        int UltralightView_LoadURL(ILua lua)
        {
            try
            {
                View_Type viewType = (View_Type)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
                string viewID = viewType.id;
                View view = views[viewID];
                string url = lua.GetString(2);
                view.LoadUrl(url);
                Console.WriteLine($"UL: ({viewType.id}).LoadURL({url})");
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }
        int UltralightView_LoadHTML(ILua lua)
        {
            View_Type viewType = (View_Type)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            string viewID = viewType.id;
            View view = views[viewID];
            string html = lua.GetString(2);
            view.LoadHtml(html);
            return 0;
        }
        int UltralightView_UpdateUntilLoads(ILua lua)
        {
            View_Type viewType = (View_Type)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            string viewID = viewType.id;
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
        public void Load_View_Shared(ILua lua)
        {
            lua.PushManagedFunction(ulView_meta__tostring);
            lua.SetField(-2, "__tostring");
            
            lua.PushManagedFunction(ulView_meta__gc);
            lua.SetField(-2, "__gc");

            lua.PushManagedFunction(UltralightView_LoadURL);
            lua.SetField(-2, "LoadURL");

            lua.PushManagedFunction(UltralightView_LoadHTML);
            lua.SetField(-2, "LoadHTML");

            lua.PushManagedFunction(UltralightView_UpdateUntilLoads);
            lua.SetField(-2, "UpdateUntilLoads");

            lua.PushManagedFunction(UltralightView_EvaluateScript);
            lua.SetField(-2, "EvaluateScript");

            lua.PushManagedFunction(UltralightView_GetPixel);
            lua.SetField(-2, "GetPixel");
        }
        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        static int unloaded_ulView_meta__gc(IntPtr lua_state)
        {
            ILua lua = GmodInterop.GetLuaFromState(lua_state);
            try
            {
                IntPtr handle = lua.GetUserType(1, View_TypeId);
                GCHandle.FromIntPtr(handle).Free();
                lua.Pop();
            }
            catch
            {

            }
            return 0;
        }
        public void Unload_View_Shared(ILua lua)
        {
            lua.PushNil();
            lua.SetField(-2, "__tostring");

            unsafe
            {
                lua.PushCFunction(&unloaded_ulView_meta__gc);
            }
            lua.SetField(-2, "__gc");

            lua.PushNil();
            lua.SetField(-2, "LoadURL");

            lua.PushNil();
            lua.SetField(-2, "LoadHTML");

            lua.PushNil();
            lua.SetField(-2, "UpdateUntilLoads");

            lua.PushNil();
            lua.SetField(-2, "EvaluateScript");

            lua.PushNil();
            lua.SetField(-2, "GetPixel");
        }
        public void LoadShared(ILua lua)
        {
            lua.PushManagedFunction(Ultralight_Update);
            lua.SetField(-2, "Update");

            lua.PushManagedFunction(Ultralight_Render);
            lua.SetField(-2, "Render");

            lua.PushManagedFunction(Ultralight_createView);
            lua.SetField(-2, "CreateView");
        }
    }
}
