using System;
using ImpromptuNinjas.UltralightSharp.Safe;
using ImpromptuNinjas.UltralightSharp.Enums;
using System.Diagnostics;
using System.Threading;

namespace GmodUltralightSandbox
{
    class Program
    {
        private static void LoggerCallback(LogLevel logLevel, string? msg)
        {
            Debug.WriteLine($"{logLevel}: {msg}");
        }
        static void Main(string[] args)
        {
            Logger logger = new Logger
            {
                LogMessage = LoggerCallback
            };
            Config cfg = new Config();
            cfg.SetUseGpuRenderer(false);           // we're using cpu renderer, because idk how to write gpu driver with output to uint8_t*
            cfg.SetResourcePath("./resources/");    // where cacert.pem is
            //AppCore.EnableDefaultLogger("./");
            AppCore.EnablePlatformFileSystem("./"); // AppCore's GetPlatformFileSystem()
            AppCore.EnablePlatformFontLoader();     // AppCore's GetPlatformFontLoader()
            Ultralight.SetLogger(logger);           // log ul's messages to file
            Renderer renderer = new Renderer(cfg);


            View view = new View(renderer, 1024, 1024, false, renderer.GetDefaultSession());
            view.LoadUrl("https://github.com");
            bool loaded = false;
            view.SetFinishLoadingCallback((data, caller, frameId, isMainFrame, url) =>
            {
                loaded = true;
                renderer.Render();
                view.GetSurface().GetBitmap().WritePng("test_01.png");
            }, default);
            while (!loaded)
            {
                renderer.Update();
            }
            renderer.Render();
            Surface surface = view.GetSurface();
            int i = 2;
            unsafe
            {
                while (true)
                {
                    ImpromptuNinjas.UltralightSharp.IntRect dirtyBounds = surface.GetDirtyBounds();
                    if (!dirtyBounds.IsEmpty())
                    {
                        Bitmap bitmap = surface.GetBitmap();
                        byte* pixels = (byte*)bitmap.LockPixels();
                        System.Drawing.Bitmap sBitmap = new System.Drawing.Bitmap(dirtyBounds.Right - dirtyBounds.Left, dirtyBounds.Bottom - dirtyBounds.Top);
                        int index = 0;
                        for (int y = 0; y < dirtyBounds.Bottom; y++)
                        {
                            for (int x = 0; x < dirtyBounds.Right; x++)
                            {
                                if (y >= dirtyBounds.Top && y < dirtyBounds.Bottom)
                                {
                                    if (x >= dirtyBounds.Left && x < dirtyBounds.Right)
                                    {
                                        int a = ((byte)pixels[index + 3]);
                                        int r = ((byte)pixels[index]);
                                        int g = ((byte)pixels[index + 1]);
                                        int b = ((byte)pixels[index + 2]);
                                        sBitmap.SetPixel(
                                            x - dirtyBounds.Top,
                                            y - dirtyBounds.Left,
                                            System.Drawing.Color.FromArgb(
                                                a,
                                                r,
                                                g,
                                                b
                                                ));
                                    }
                                }
                                index += 4;
                            }
                        }
                        sBitmap.Save($"test_{i}.bmp");
                        bitmap.UnlockPixels();
                        surface.ClearDirtyBounds();
                        i++;
                    }
                    else
                        Thread.Sleep(100);
                }
            }
        }
    }
}
