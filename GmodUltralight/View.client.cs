using GmodNET.API;
using ImpromptuNinjas.UltralightSharp.Safe;
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
	}
}
