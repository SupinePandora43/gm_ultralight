using GmodNET.API;
using ImpromptuNinjas.UltralightSharp.Safe;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace GmodUltralight
{
    partial class GmodUltralight
    {
        int Ultralight_Update(ILua lua)
        {
            renderer.Update();
            return 0;
        }
        int Ultralight_Render(ILua lua)
        {
            renderer.Render();
            return 0;
        }

        int Ultralight_createView(ILua lua)
        {
            uint width = (uint)lua.GetNumber(1);
            uint height = (uint)lua.GetNumber(2);
            bool transparent = lua.GetBool(3);
            string viewID = Guid.NewGuid().ToString();

            if (!views.ContainsKey(viewID))
            {
                View view = new View(renderer, width, height, transparent, renderer.GetDefaultSession());
                Console.WriteLine("UL: Created View");
                views.Add(viewID, view);
                //IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf(result));
                //Marshal.StructureToPtr(result, p, false);
                lua.PushUserType(GCHandle.ToIntPtr(GCHandle.Alloc(viewID, GCHandleType.Weak)), View_TypeId);
            }
            else
            {
                lua.PushNil();
            }

            return 1;
        }

        int ulView_meta__tostring(ILua lua)
        {
            string viewID = (string)GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId)).Target;
            lua.PushString(viewID);
            return 1;
        }
        /// <summary>
        /// Called after lua's "collectgarbage"
        /// </summary>
        /// <param name="lua"></param>
        /// <returns></returns>
        int ulView_meta__gc(ILua lua)
        {
            GCHandle gchandle = GCHandle.FromIntPtr(lua.GetUserType(1, View_TypeId));
            string viewID = (string)gchandle.Target;

            views.Remove(viewID);
            Console.WriteLine($"UL: view {viewID} is garbage collected");
            gchandle.Free();

            return 0;
        }
        /// <summary>
        /// after module unloaded it will fix crash after using __gc on view
        /// </summary>
        /// <param name="lua_state"></param>
        /// <returns></returns>
        [UnmanagedCallersOnly(CallConvs = new Type[] { typeof(CallConvCdecl) })]
        static int unloaded_ulView_meta__gc(IntPtr lua_state)
        {
            ILua lua = GmodInterop.GetLuaFromState(lua_state);
            try
            {
                IntPtr handle = lua.GetUserType(1, View_TypeId);
                GCHandle.FromIntPtr(handle).Free();
                lua.Pop();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return 0;
        }



        int ulView_meta__index(ILua lua)
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
                default:
                    lua.PushNil();
                    break;
            }
            return 1;
        }



        public void Load_View_Shared(ILua lua)
        {
            View_TypeId = lua.CreateMetaTable("ulView");

            lua.PushManagedFunction(ulView_meta__index);
            lua.SetField(-2, "__index");

            lua.PushManagedFunction(ulView_meta__tostring);
            lua.SetField(-2, "__tostring");

            lua.Pop();
        }
        public void Unload_View_Shared(ILua lua)
        {
            lua.PushMetaTable(View_TypeId);

            lua.PushNil();
            lua.SetField(-2, "__tostring");

            unsafe
            {
                lua.PushCFunction(&unloaded_ulView_meta__gc);
            }
            lua.SetField(-2, "__gc");

            // that's a genius thing!
            lua.PushNil();
            lua.SetField(-2, "__index");

            lua.Pop();
        }
        public void LoadShared(ILua lua)
        {
            lua.PushManagedFunction(Ultralight_Update);
            lua.SetField(-2, "Update");

            lua.PushManagedFunction(Ultralight_Render);
            lua.SetField(-2, "Render");

            lua.PushManagedFunction(Ultralight_createView);
            lua.SetField(-2, "CreateView");
        }
    }
}
