using GmodNET.API;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using ImpromptuNinjas.UltralightSharp.Safe;
using String = ImpromptuNinjas.UltralightSharp.String;
using System.Collections.Generic;
using ImpromptuNinjas.UltralightSharp.Enums;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;
using GmodNET.Extras;

namespace GmodUltralight
{
    public partial class GmodUltralight : GmodNET.API.IModule
    {
        // GmodDotNet
        public string ModuleName => "GmodUltralight";
        public string ModuleVersion => "0.1.1";
        // Ultralight                      # =null them all!!! hahahhahahha
        LoggerLogMessageCallback cb;
        Logger logger;
        Renderer renderer;
        Dictionary<string, View> views;
        // LUA
        CFuncManagedDelegate Ultralight_createView;
        CFuncManagedDelegate Ultralight_Update;
        CFuncManagedDelegate Ultralight_Render;
        CFuncManagedDelegate UltralightView_CL_DrawDirty;
        CFuncManagedDelegate UltralightView_SV_DrawSingle;
        CFuncManagedDelegate UltralightView_LoadURL;
        CFuncManagedDelegate UltralightView_LoadHTML;
        CFuncManagedDelegate UltralightView_UpdateUntilLoads;
        CFuncManagedDelegate UltralightView_IsValid;

        private static void LOG(string msg)
        {
            StreamWriter writer = new StreamWriter("./fslogcs.txt", true);
            writer.WriteLine(msg);
            writer.Close();
        }

        private static void PrintToConsole(ILua lua, string msg)
        {
            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.GetField(-1, "print");
            lua.PushString(msg);
            lua.MCall(1, 0);
            lua.Pop(1);
        }
        private static void LoggerCallback(LogLevel logLevel, string? msg)
        {
            Debug.WriteLine($"{logLevel}: {msg}");
            LOG(msg);
        }
        /// <summary>
        /// Sends single pixel to clients
        /// </summary>
        private static void SendPixel(ILua lua, byte a, byte r, byte g, byte b, int x, int y)
        {
            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.GetField(-1, "net");
            lua.GetField(-1, "Start");
            lua.PushString("Ultralight_DrawSingle");
            lua.MCall(1, 0);

            lua.GetField(-1, "WriteUInt");
            lua.PushNumber(r);
            lua.PushNumber(8);
            lua.MCall(2, 0);

            lua.GetField(-1, "WriteUInt");
            lua.PushNumber(g);
            lua.PushNumber(8);
            lua.MCall(2, 0);

            lua.GetField(-1, "WriteUInt");
            lua.PushNumber(b);
            lua.PushNumber(8);
            lua.MCall(2, 0);

            lua.GetField(-1, "WriteUInt");
            lua.PushNumber(a);
            lua.PushNumber(8);
            lua.MCall(2, 0);

            lua.GetField(-1, "WriteUInt");
            lua.PushNumber(x);
            lua.PushNumber(32);
            lua.MCall(2, 0);

            lua.GetField(-1, "WriteUInt");
            lua.PushNumber(y);
            lua.PushNumber(32);
            lua.MCall(2, 0);

            lua.GetField(-1, "Broadcast");
            lua.MCall(0, 0);
            lua.Pop();
        }
        public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
        {
            /*assembly_context.SetCustomNativeLibraryResolver((context, name) =>
            {
                return NativeLibrary.Load(Path.GetFullPath("./garrysmod/lua/bin/Modules/GmodUltralight/runtimes/win-x64/native/" + name));
            });*/
            //Ultralight.SetLogger(new Logger { LogMessage = (logLevel, msg) => Console.WriteLine($"{logLevel.ToString()}: {msg}") });
            // TODO: really? LogMessage = LoggerCallback
            cb = LoggerCallback;
            logger = new Logger
            {
                LogMessage = cb
            };
            Config cfg = new Config();
            cfg.SetUseGpuRenderer(false);           // we're using cpu renderer, because idk how to write gpu driver with output to uint8_t*
            cfg.SetResourcePath("./resources/");    // where cacert.pem is
            //AppCore.EnableDefaultLogger("./");
            AppCore.EnablePlatformFileSystem("./"); // AppCore's GetPlatformFileSystem()
            AppCore.EnablePlatformFontLoader();     // AppCore's GetPlatformFontLoader()
            Ultralight.SetLogger(logger);           // log ul's messages to file
            renderer = new Renderer(cfg);
            views = new Dictionary<string, View>();

            Ultralight_createView = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);

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
            };
            UltralightView_LoadURL = (lua_state) =>
                    {
                        ILua lua = GmodInterop.GetLuaFromState(lua_state);

                        string viewID = lua.GetString(1);
                        View view = views[viewID];
                        string url = lua.GetString(2);
                        view.LoadUrl(url);

                        return 0;
                    };
            UltralightView_LoadHTML = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                string viewID = lua.GetString(1);
                View view = views[viewID];
                string html = lua.GetString(2);
                view.LoadHtml(html);
                return 0;
            };
            UltralightView_UpdateUntilLoads = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);

                string viewID = lua.GetString(1);
                View view = views[viewID];
                bool loaded = false;
                FinishLoadingCallback finishcallback = (data, caller, frameId, isMainFrame, url) =>
                {
                    loaded = true;
                };
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
            };
            UltralightView_IsValid = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                string viewID = lua.GetString(1);
                lua.PushBool(views.ContainsKey(viewID));
                return 1;
            };
            UltralightView_CL_DrawDirty = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                string viewID = lua.GetString(1);
                View view = views[viewID];
                Surface surface = view.GetSurface();
                Bitmap bitmap = surface.GetBitmap();
                ImpromptuNinjas.UltralightSharp.IntRect bounds = surface.GetDirtyBounds();
                if (!bounds.IsEmpty())
                    unsafe
                    {
                        lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
                        lua.GetField(-1, "surface");
                        byte* pixels = (byte*)bitmap.LockPixels();
                        long index = 0;
                        // TODO: start from bounds.Top
                        for (int y = 0; y < bounds.Bottom; y++)
                        {
                            for (int x = 0; x < bounds.Right; x++)
                            {
                                if (y >= bounds.Top && y < bounds.Bottom)
                                {
                                    if (x >= bounds.Left && x < bounds.Right)
                                    {
                                        int a = ((byte)pixels[index + 3]);
                                        int r = ((byte)pixels[index + 2]);
                                        int g = ((byte)pixels[index + 1]);
                                        int b = ((byte)pixels[index]);
                                        lua.GetField(-1, "SetDrawColor");
                                        lua.PushNumber(r);
                                        lua.PushNumber(g);
                                        lua.PushNumber(b);
                                        lua.PushNumber(a);
                                        lua.MCall(4, 0);
                                        lua.GetField(-1, "DrawRect");
                                        lua.PushNumber(x);
                                        lua.PushNumber(y);
                                        lua.PushNumber(1);
                                        lua.PushNumber(1);
                                        lua.MCall(4, 0);
                                    }
                                }
                                index += 4;
                            }
                            index = y * bitmap.GetRowBytes();
                        }
                        pixels = null; // TODO: free memory?
                        bitmap.UnlockPixels();
                        surface.ClearDirtyBounds();
                    }
                return 0;
            };
            UltralightView_SV_DrawSingle = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                string viewID = lua.GetString(1);
                View view = views[viewID];
                Surface surface = view.GetSurface();
                Bitmap bitmap = surface.GetBitmap();
                ImpromptuNinjas.UltralightSharp.IntRect bounds = surface.GetDirtyBounds();
                if (!bounds.IsEmpty())
                    try
                    {
                        unsafe
                        {
                            byte* pixels = (byte*)bitmap.LockPixels();
                            long index = 0;
                            for (int y = 0; y < bounds.Bottom; y++)
                            {
                                for (int x = 0; x < bounds.Right; x++)
                                {
                                    if (y >= bounds.Top && y < bounds.Bottom)
                                    {
                                        if (x >= bounds.Left && x < bounds.Right)
                                        {
                                            byte a = pixels[index + 3];
                                            byte r = pixels[index + 2];
                                            byte g = pixels[index + 1];
                                            byte b = pixels[index];

                                            SendPixel(lua, a, r, g, b, x, y);
                                        }
                                    }
                                    index += 4;
                                }
                                index = y * bitmap.GetRowBytes();
                            }
                            pixels = null; // TODO: free memory?
                            bitmap.UnlockPixels();
                            surface.ClearDirtyBounds();
                        }
                    }
                    finally
                    {
                        bitmap.UnlockPixels();
                    }
                return 0;
            };
            Ultralight_Update = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                renderer.Update();
                return 0;
            };
            Ultralight_Render = (lua_state) =>
            {
                ILua lua = GmodInterop.GetLuaFromState(lua_state);
                renderer.Render();
                return 0;
            };
            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.CreateTable();

            lua.PushCFunction(Ultralight_createView);
            lua.SetField(-2, "CreateView");

            lua.PushCFunction(Ultralight_Update);
            lua.SetField(-2, "Update");

            lua.PushCFunction(Ultralight_Render);
            lua.SetField(-2, "Render");

            lua.PushCFunction(UltralightView_LoadURL);
            lua.SetField(-2, "View_LoadURL");

            lua.PushCFunction(UltralightView_LoadHTML);
            lua.SetField(-2, "View_LoadHTML");

            lua.PushCFunction(UltralightView_UpdateUntilLoads);
            lua.SetField(-2, "View_UpdateUntilLoads");

            lua.PushCFunction(UltralightView_IsValid);
            lua.SetField(-2, "View_IsValid");

            lua.PushCFunction(UltralightView_CL_DrawDirty);
            lua.SetField(-2, "View_CL_DrawDirty");

            lua.PushCFunction(UltralightView_SV_DrawSingle);
            lua.SetField(-2, "View_SV_DrawSingle");

            // shared.cs
            LoadShared(lua);

            lua.SetField(-2, "Ultralight");

            lua.Pop();
        }
        public void Unload(ILua lua)
        {
            views = null;
            renderer.Dispose();
            renderer = null;
            cb = null;
        }
    }
}
