using GmodNET.API;
using ImpromptuNinjas.UltralightSharp.Safe;
using System;
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
						for (uint x = 0; x < view.GetWidth(); x++)
						{
							byte a = pixels[index + 3];
							byte r = pixels[index + 2];
							byte g = pixels[index + 1];
							byte b = pixels[index];

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
							index += 4;
						}
						index = y * bitmap.GetRowBytes();
					}
					pixels = null; // TODO: free memory?
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
			lua.Pop();
			return 0;
		}
		int View_DrawToSurfaceByLines(ILua lua)
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
					uint height = view.GetHeight();
					uint width = view.GetWidth();
					for (uint y = 0; y < height; y++)
					{
						bool firstRect = true;
						byte _a = 0;
						byte _r = 0;
						byte _g = 0;
						byte _b = 0;
						uint rect_x = 0;
						uint rect_width = 0;
						for (uint x = 0; x < width; x++)
						{
							byte a = pixels[index + 3];
							byte r = pixels[index + 2];
							byte g = pixels[index + 1];
							byte b = pixels[index];

							if (firstRect)
							{
								if (x + 1 >= width)
								{
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
								else
								{
									rect_x = x;
									rect_width = 1;
									_a = a;
									_r = r;
									_g = g;
									_b = b;
								}
								firstRect = true;
							}
							else
							{
								if (_a == a && _r == r && _g == g && _b == b)
								{
									rect_width++;
									if (x + 1 >= width)
									{
										lua.GetField(-1, "SetDrawColor");
										lua.PushNumber(_r);
										lua.PushNumber(_g);
										lua.PushNumber(_b);
										lua.PushNumber(_a);
										lua.MCall(4, 0);
										lua.GetField(-1, "DrawRect");
										lua.PushNumber(rect_x);
										lua.PushNumber(y);
										lua.PushNumber(rect_width);
										lua.PushNumber(1);
										lua.MCall(4, 0);
									}
								}
								else
								{
									lua.GetField(-1, "SetDrawColor");
									lua.PushNumber(_r);
									lua.PushNumber(_g);
									lua.PushNumber(_b);
									lua.PushNumber(_a);
									lua.MCall(4, 0);
									lua.GetField(-1, "DrawRect");
									lua.PushNumber(rect_x);
									lua.PushNumber(y);
									lua.PushNumber(rect_width);
									lua.PushNumber(1);
									lua.MCall(4, 0);
									if (x + 1 >= width)
									{
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
									else
									{
										_a = a;
										_r = r;
										_g = g;
										_b = b;
										rect_x = x;
										rect_width = 1;
									}
								}
							}

							index += 4;
						}
						index = y * bitmap.GetRowBytes();
					}
					pixels = null; // TODO: free memory?
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
			lua.Pop();
			return 0;
		}
	}
}
