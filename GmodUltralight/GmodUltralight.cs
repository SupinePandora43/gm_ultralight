using GmodNET.API;
using System;
using System.IO;
using ImpromptuNinjas.UltralightSharp.Safe;
using System.Collections.Generic;
using ImpromptuNinjas.UltralightSharp.Enums;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GmodUltralight
{
    public partial class GmodUltralight : GmodNET.API.IModule
    {
        public string ModuleName => "GmodUltralight";
        public string ModuleVersion => "0.1.1";

        LoggerLogMessageCallback cb;
        Logger logger;
        Renderer renderer;
        Dictionary<string, View> views;

        static int View_TypeId;
        private static void LOG(string msg)
        {
            StreamWriter writer = new StreamWriter("./fslogcs.txt", true);
            writer.WriteLine(msg);
            writer.Close();
            Console.WriteLine(msg);
        }

        private static void LoggerCallback(LogLevel logLevel, string msg)
        {
            Debug.WriteLine($"{logLevel}: {msg}");
            LOG(msg);
        }
        /// <summary>
        /// Sends single pixel to clients
        /// </summary>
        private static void SendPixel(ILua lua, byte a, byte r, byte g, byte b, uint x, uint y)
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

        int UltralightView_CL_DrawDirty(ILua lua)
        {
            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
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
        }
        int UltralightView_SV_DrawSingle(ILua lua)
        {
            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            View view = views[viewID];
            uint x = (uint)lua.GetNumber(2);
            uint y = (uint)lua.GetNumber(3);
            Bitmap bitmap = view.GetSurface().GetBitmap();
            try
            {
                unsafe
                {
                    long index = (y * bitmap.GetRowBytes()) + (x * 4);
                    byte* pixels = (byte*)bitmap.LockPixels();
                    byte a = pixels[index + 3];
                    byte r = pixels[index + 2];
                    byte g = pixels[index + 1];
                    byte b = pixels[index];
                    SendPixel(lua, a, r, g, b, x, y);
                }
            }
            finally
            {
                bitmap.UnlockPixels();
            }
            /*ImpromptuNinjas.UltralightSharp.IntRect bounds = surface.GetDirtyBounds();
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
                }*/
            return 0;
        }
        int csprint(ILua lua)
        {
            Console.WriteLine(lua.GetString(1));
            return 0;
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



            Load_View_Shared(lua);



            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.GetField(-1, "util");
            lua.GetField(-1, "AddNetworkString");
            lua.PushString("Ultralight_DrawSingle");
            lua.MCall(1, 0);
            lua.Pop();



            lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
            lua.CreateTable();

            // shared.cs
            LoadShared(lua);

            lua.SetField(-2, "Ultralight");

            lua.PushManagedFunction(csprint);
            lua.SetField(-2, "csprint");

            lua.Pop();
        }
        public void Unload(ILua lua)
        {
            Console.WriteLine("UL: Goodbye world!");

            // release all lua refereces
            Unload_View_Shared(lua);

            // release all native references
            views = null;
            renderer.Dispose();
            renderer = null;
            cb = null;
        }
    }
}
