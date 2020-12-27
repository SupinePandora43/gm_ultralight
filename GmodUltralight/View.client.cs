using GmodNET.API;
using ImpromptuNinjas.UltralightSharp.Safe;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GmodUltralight
{
	partial class GmodUltralight
	{
		int View_DrawDirty(ILua lua)
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
		class ColoredRect
		{
			public byte a, r, g, b;
			public uint x, w;
		}
		int View_DrawToSurface(ILua lua)
		{
			string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
			View view = views[viewID];
			Surface surface = view.GetSurface();
			Bitmap bitmap = surface.GetBitmap();
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "surface");
			try
			{
				unsafe
				{
					byte* pixels = (byte*)bitmap.LockPixels();
					long index = 0;
					for (uint y = 0; y < view.GetHeight(); y++)
					{
						List<ColoredRect> rects = new();
						for (uint x = 0; x < view.GetWidth(); x++)
						{
							byte a = pixels[index + 3];
							byte r = pixels[index + 2];
							byte g = pixels[index + 1];
							byte b = pixels[index];

							if (rects.Count < 1)
							{
								rects.Add(new ColoredRect { a = a, b = b, g = g, r = r, w = 1, x = x });
							}
							else
							{
								ColoredRect rect = rects.Last();
								if (rect.a == a && rect.r == r && rect.g == g && rect.b == b)
								{
									rect.w++;
								}
								else
								{
									rects.Add(new ColoredRect { a = a, b = b, g = g, r = r, w = 1, x = x });
								}
							}

							index += 4;
						}
						rects.ForEach((rect) =>
						{
							lua.GetField(-1, "SetDrawColor");
							lua.PushNumber(rect.r);
							lua.PushNumber(rect.g);
							lua.PushNumber(rect.b);
							lua.PushNumber(rect.a);
							lua.MCall(4, 0);
							lua.GetField(-1, "DrawRect");
							lua.PushNumber(rect.x);
							lua.PushNumber(y);
							lua.PushNumber(rect.w);
							lua.PushNumber(1);
							lua.MCall(4, 0);
						});
						index = y * bitmap.GetRowBytes();
					}
					pixels = null; // TODO: free memory?
				}
			}
			finally
			{
				bitmap.UnlockPixels();
			}
			lua.Pop();
			return 0;
		}
	}
}
