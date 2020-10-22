using GmodNET.API;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ImpromptuNinjas.UltralightSharp.Safe;
using String = ImpromptuNinjas.UltralightSharp.String;
using System.Collections.Generic;

namespace gm_ultralight
{
    public class GmodUltralight : GmodNET.API.IModule
    {
        public string ModuleName => "GmodUltralight";

        public string ModuleVersion => "0.1.0";
        Renderer renderer;
        Dictionary<string, View> views;
        CFuncManagedDelegate UltralightView_LoadURL;
        CFuncManagedDelegate UltralightView_UpdateUntilLoads;
        CFuncManagedDelegate Ultralight_createView;
        public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
        {
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
                View view = new View(renderer, width, height, transparent, (Session)null);
                string viewID = System.Guid.NewGuid().ToString();
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
                return 0;
            };
        }
        public void Unload(ILua lua)
        {
            //views = null;
            renderer.Dispose();
        }
    }
}
