using GmodNET.API;
using ImpromptuNinjas.UltralightSharp.Safe;
using System;
using System.Runtime.InteropServices;

namespace GmodUltralight
{
	partial class GmodUltralight
	{
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
		int View_DrawSingle(ILua lua)
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
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			finally
			{
				bitmap.UnlockPixels();
			}
			return 0;
		}
	}
}
