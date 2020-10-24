using GmodNET.API;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ImpromptuNinjas.UltralightSharp.Safe;
using String = ImpromptuNinjas.UltralightSharp.String;
using System.Collections.Generic;

namespace GmodUltralight
{
    public class GmodUltralight : GmodNET.API.IModule
    {
        public string ModuleName => "GmodUltralight";

        public string ModuleVersion => "0.1.1";
        Renderer renderer;
        Dictionary<string, View> views;
        CFuncManagedDelegate Ultralight_createView;
        CFuncManagedDelegate UltralightView_LoadURL;
        CFuncManagedDelegate UltralightView_UpdateUntilLoads;
        CFuncManagedDelegate UltralightView_IsValid;
        public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
        {
            /*assembly_context.SetCustomNativeLibraryResolver((context, name) =>
            {
                return NativeLibrary.Load(Path.GetFullPath("./garrysmod/lua/bin/Modules/GmodUltralight/runtimes/win-x64/native/" + name));
            });*/
            //Ultralight.SetLogger(new Logger { LogMessage = (logLevel, msg) => Console.WriteLine($"{logLevel.ToString()}: {msg}") });
            Config cfg = new Config();
            AppCore.EnableDefaultLogger("./");
            AppCore.EnablePlatformFileSystem("./");
            AppCore.EnablePlatformFontLoader();
            renderer = new Renderer(cfg);
            views = new Dictionary<string, View>();

            Ultralight_createView = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                uint width = (uint)lua.GetNumber(1);
                uint height = (uint)lua.GetNumber(2);
                bool transparent = lua.GetBool(3);
                string viewID = Guid.NewGuid().ToString();
                View view = new View(renderer, width, height, transparent, renderer.GetDefaultSession());
                views.Add(viewID, view);
                lua.PushString(viewID);
                return 1;
            };
            UltralightView_LoadURL = (lua_state) =>
                    {
                        ILua lua = GmodInterop.GetLuaFromState(lua_state);
                        string viewID = lua.GetString(1);
                        View view = views[viewID];
                        string url = lua.GetString(1);
                        view.LoadUrl(url);
                        return 0;
                    };
            UltralightView_UpdateUntilLoads = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                string viewID = lua.GetString(1);
                View view = views[viewID];
                bool loaded = false;
                view.SetFinishLoadingCallback((data, caller, frameId, isMainFrame, url) =>
                {
                    loaded = true;
                }, default);
                uint timeout = 0;
                while (!loaded && timeout < 100000)
                {
                    renderer.Update();
                    timeout++;
                }
                renderer.Render();
                Bitmap bitmap = view.GetSurface().GetBitmap();
                bitmap.WritePng("csresult.png");
                return 0;
            };
            UltralightView_IsValid = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                string viewID = lua.GetString(1);
                lua.PushBool(views.ContainsKey(viewID));
                return 1;
            };

            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.PushCFunction(Ultralight_createView);
            lua.SetField(-2, "Ultralight_createView");
            lua.Pop(1);

            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.PushCFunction(UltralightView_LoadURL);
            lua.SetField(-2, "UltralightView_LoadURL");
            lua.Pop(1);

            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.PushCFunction(UltralightView_UpdateUntilLoads);
            lua.SetField(-2, "UltralightView_UpdateUntilLoads");
            lua.Pop(1);

            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.PushCFunction(UltralightView_IsValid);
            lua.SetField(-2, "UltralightView_IsValid");
            lua.Pop(1);
        }
        public void Unload(ILua lua)
        {
            views = null;
            renderer.Dispose();
            renderer = null;
        }
    }
}
