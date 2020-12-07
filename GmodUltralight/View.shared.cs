using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using GmodNET.API;
using ImpromptuNinjas.UltralightSharp.Enums;
using ImpromptuNinjas.UltralightSharp.Safe;

namespace GmodUltralight
{
    partial class GmodUltralight
    {
        /// <summary>
        /// contains GCHandles
        /// used to free them on module unload
        /// </summary>
        List<GCHandle> handles;

        int View_new(ILua lua)
        {
            uint width = (uint)lua.GetNumber(1);
            uint height = (uint)lua.GetNumber(2);
            bool transparent = lua.GetBool(3);
            string viewID = Guid.NewGuid().ToString();

            if (!views.ContainsKey(viewID))
            {
                View view = new(renderer, width, height, transparent, renderer.GetDefaultSession());
                Console.WriteLine("UL: Created View");
                views.Add(viewID, view);
                //IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(result));
                //Marshal.StructureToPtr(result, p, false);
                GCHandle gCHandle = GCHandle.Alloc(viewID, GCHandleType.Weak);
                handles.Add(gCHandle);
                lua.PushUserType(GCHandle.ToIntPtr(gCHandle), View_TypeId);
            }
            else
            {
                lua.PushNil();
            }
            return 1;
        }

        static int View__meta__tostring(ILua lua)
        {
            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            lua.PushString(viewID);
            return 1;
        }

        /// <summary>
        /// Called after lua's "collectgarbage"
        /// </summary>
        int View__meta__gc(ILua lua)
        {
            try
            {
                GCHandle gchandle = GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId));
                string viewID = (string)gchandle.Target;

                views.Remove(viewID);
                Console.WriteLine($"[UL] ({viewID}): view is garbage collected");
                int indexOfGCHandle = handles.IndexOf(gchandle);
                if (indexOfGCHandle != -1)
                {
                    handles.RemoveAt(indexOfGCHandle);
                }
                else
                {
                    Console.WriteLine("Can't find gchandle in handles");
                    Console.Error.WriteLine("Can't find gchandle in handles");
                }
                gchandle.Free();
                Console.WriteLine($"[UL] ({viewID}): handle released");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }

        int View__meta__index(ILua lua)
        {
            // lets see what throws an exception :D
            GCHandle gchandle = GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId));
            string id = (string)gchandle.Target;
            string name = lua.GetString(2);
            Console.WriteLine($"UL ({id}): trying to index {name}");
            switch (name)
            {
                case "Dispose":
                    {
                        lua.PushManagedFunction((lua) =>
                        {
                            GCHandle gchandle = GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId));
                            string id = (string)gchandle.Target;
                            // it will call view's Dispose()
                            views.Remove(id);
                            gchandle.Free();
                            return 0;
                        });
                        break;
                    }
                case "IsValid":
                    {
                        lua.PushManagedFunction((lua) =>
                        {
                            string id = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
                            lua.PushBool(views.ContainsKey(id));
                            return 1;
                        });
                        break;
                    }
                case "LoadURL":
                    {
                        lua.PushManagedFunction((lua) =>
                        {
                            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
                            View view = views[viewID];
                            string url = lua.GetString(2);
                            view.LoadUrl(url);
                            Console.WriteLine($"UL: ({viewID}).LoadURL({url})");
                            return 0;
                        });
                        break;
                    }
                case "LoadHTML":
                    {
                        lua.PushManagedFunction((lua) =>
                        {
                            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
                            View view = views[viewID];
                            string html = lua.GetString(2);
                            view.LoadHtml(html);
                            return 0;
                        });
                        break;
                    }
                case "UpdateUntilLoads":
                    {
                        lua.PushManagedFunction((lua) =>
                        {
                            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
                            View view = views[viewID];

                            bool loaded = false;
                            void finishcallback(IntPtr data, View caller, ulong frameId, bool isMainFrame, string url)
                            {
                                loaded = true;
                            }
                            view.SetFinishLoadingCallback(finishcallback, default);
                            uint timeout = 0;
                            while (!loaded && timeout < 10000)
                            {
                                renderer.Update();
                                timeout++;
                                Thread.Sleep(10);
                            }
                            lua.PushBool(loaded);
                            return 1;
                        });
                        break;
                    }
                case "EvaluateScript":
                    {
                        lua.PushManagedFunction((lua) =>
                        {
                            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
                            View view = views[viewID];
                            lua.PushString(view.EvaluateScript(lua.GetString(2)));
                            return 1;
                        });
                        break;
                    }
                case "GetPixel":
                    {
                        lua.PushManagedFunction((lua) =>
                        {
                            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
                            View view = views[viewID];
                            uint x = (uint)lua.GetNumber(2);
                            uint y = (uint)lua.GetNumber(3);
                            bool a_first = lua.GetBool(4);
                            Bitmap bitmap = view.GetSurface().GetBitmap();
                            try
                            {
                                unsafe
                                {
                                    byte* pixels = (byte*)bitmap.LockPixels();
                                    long index = y * bitmap.GetRowBytes();
                                    index += x * 4;
                                    byte a = pixels[index + 3];
                                    byte r = pixels[index + 2];
                                    byte g = pixels[index + 1];
                                    byte b = pixels[index];

                                    if (a_first) lua.PushNumber(a);

                                    lua.PushNumber(r);
                                    lua.PushNumber(g);
                                    lua.PushNumber(b);

                                    if (!a_first) lua.PushNumber(a);
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
                            return 4;
                        });
                        break;
                    }
                case "Bake":
                    lua.PushManagedFunction((lua) =>
                    {
                        string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
                        View view = views[viewID];
                        string nameForSave = lua.GetString(2);

                        Directory.CreateDirectory("./garrysmod/materials/ultralight");
                        Bitmap bitmap = view.GetSurface().GetBitmap();
                        bitmap.SwapRedBlueChannels();
                        try
                        {
                            bitmap.WritePng($"./garrysmod/materials/ultralight/{nameForSave}.png");
                        }
                        finally
                        {
                            bitmap.SwapRedBlueChannels();
                        }
                        lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
                        lua.GetField(-1, "resource");
                        lua.GetField(-1, "AddSingleFile");
                        lua.PushString($"materials/ultralight/{nameForSave}.png");
                        lua.MCall(1, 0);
                        lua.Pop();
                        return 0;
                    });
                    break;
                case "FireMouseEvent":
                    lua.PushManagedFunction(View_FireMouseEvent);
                    break;
                case "DrawDirty":
                    lua.PushManagedFunction(View_DrawDirty);
                    break;
                case "DrawSingle":
                    lua.PushManagedFunction(View_DrawSingle);
                    break;
                default:
                    /*lua.PushManagedFunction((lua) =>
                    {
                        MethodInfo method = GetType().GetMethod(name);
                        if (method != null)
                            return (int)method.Invoke(this, new object[] { lua });
                        return 0;
                    });*/
                    lua.PushNil();
                    break;
            }
            return 1;
        }
        int View_FireMouseEvent(ILua lua)
        {
            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            View view = views[viewID];
            MouseEventType mouseEventType = (MouseEventType)lua.GetNumber(2);
            int x = (int)lua.GetNumber(3);
            int y = (int)lua.GetNumber(4);
            MouseButton mouseButton = (MouseButton)lua.GetNumber(5);
            MouseEvent mouseEvent = new(mouseEventType, x, y, mouseButton);
            view.FireMouseEvent(mouseEvent);
            return 0;
        }
    }
}
