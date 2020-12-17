using System;
using ImpromptuNinjas.UltralightSharp.Safe;
using ImpromptuNinjas.UltralightSharp.Enums;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.IO;

namespace GmodUltralightSandbox
{
	class Program
	{
		private static void LoggerCallback(LogLevel logLevel, string? msg)
		{
			Debug.WriteLine($"{logLevel}: {msg}");
		}

		private readonly static char[] chars = { '#', '#', '@', '%', '=', '+', '*', ':', '-', '.', '\u00A0' }; //&nbsp;
		private static int Map(int input, int inputMin, int inputMax, int min, int max)
		{
			return min + (input - inputMin) * (max - min) / (inputMax - inputMin);
		}
		private static char colorToChar(byte r, byte g, byte b, byte a)
		{
			int brightness = Map((r + g + b) * a, 0, 195075, 0, 10);
			return chars[brightness];
		}

		static string ToAscii(View view)
		{
			StringBuilder stringBuilder = new();
			Bitmap bitmap = view.GetSurface().GetBitmap();

			System.Drawing.Bitmap bitmap1 = new((int)view.GetWidth(), (int)view.GetHeight());

			try
			{
				unsafe
				{
					byte* pixels = (byte*)bitmap.LockPixels();
					long index = 0;

					for (int y = 0; y < bitmap.GetHeight(); y++)
					{
						for (int x = 0; x < bitmap.GetWidth(); x++)
						{
							byte a = pixels[index + 3];
							byte r = pixels[index + 2];
							byte g = pixels[index + 1];
							byte b = pixels[index];

							bitmap1.SetPixel(x, y, System.Drawing.Color.FromArgb(a, r, g, b));

							index += 4;
						}
					}

					System.Drawing.Bitmap bitmap2 = new((System.Drawing.Image)bitmap1, new System.Drawing.Size(128, 128));

					for (int y = 0; y < bitmap2.Height; y++)
					{
						for (int x = 0; x < bitmap2.Width; x++)
						{
							System.Drawing.Color color = bitmap2.GetPixel(x, y);
							stringBuilder.Append(colorToChar(color.R, color.G, color.B, color.A));
						}
						stringBuilder.Append('\n');
					}
				}
			}
			finally
			{
				bitmap.UnlockPixels();
			}
			return stringBuilder.ToString();
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

			View view = new View(renderer, 512, 512, false, renderer.GetDefaultSession());
			view.LoadUrl("https://discord.com"); //https://codepen.io/crasty01/pen/EdXoVN
			bool loaded = false;
			view.SetFinishLoadingCallback((data, caller, frameId, isMainFrame, url) =>
			{
				loaded = true;
				renderer.Render();
				view.GetSurface().GetBitmap().SwapRedBlueChannels();
				view.GetSurface().GetBitmap().WritePng("test_1.png");
				view.GetSurface().GetBitmap().SwapRedBlueChannels();
				view.GetSurface().ClearDirtyBounds();
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
					renderer.Update();
					renderer.Render();
					ImpromptuNinjas.UltralightSharp.IntRect dirtyBounds = surface.GetDirtyBounds();
					if (!dirtyBounds.IsEmpty())
					{
						Bitmap bitmap = surface.GetBitmap();
						byte* pixels = (byte*)bitmap.LockPixels();
						System.Drawing.Bitmap sBitmap = new System.Drawing.Bitmap((int)view.GetWidth(), (int)view.GetHeight()); //dirtyBounds.Right - dirtyBounds.Left, dirtyBounds.Bottom - dirtyBounds.Top
						long index = dirtyBounds.Top * bitmap.GetRowBytes();
						for (int y = dirtyBounds.Top; y < dirtyBounds.Bottom; y++)
						{
							index += 4 * dirtyBounds.Left;
							for (int x = dirtyBounds.Left; x < dirtyBounds.Right; x++)
							{
								if (y >= dirtyBounds.Top && y < dirtyBounds.Bottom)
								{
									if (x >= dirtyBounds.Left && x < dirtyBounds.Right)
									{
										int a = ((byte)pixels[index + 3]);
										int r = ((byte)pixels[index + 2]);
										int g = ((byte)pixels[index + 1]);
										int b = ((byte)pixels[index]);
										sBitmap.SetPixel(
											x,//- dirtyBounds.Left,
											y,//- dirtyBounds.Top,
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
							index = y * bitmap.GetRowBytes();
						}
						sBitmap.Save($"test_{i}.bmp");
						sBitmap.Dispose();
						//pixels = null;
						bitmap.UnlockPixels();

						surface.ClearDirtyBounds();

						File.WriteAllText($"test_{i}.txt", ToAscii(view));

						i++;
					}
					//else
					//	Thread.Sleep(100);
				}
			}
		}
	}
}
