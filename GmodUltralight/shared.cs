using GmodNET.API;

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

        public void Load_View_Shared(ILua lua)
        {
            handles = new();

            View_TypeId = lua.CreateMetaTable("ulView");

            lua.PushManagedFunction(View__meta__index);
            lua.SetField(-2, "__index");

            lua.PushManagedFunction(View__meta__tostring);
            lua.SetField(-2, "__tostring");

            lua.PushManagedFunction(View__meta__gc);
            lua.SetField(-2, "__gc");

            lua.Pop();
        }
        public void Unload_View_Shared(ILua lua)
        {
            lua.PushMetaTable(View_TypeId);

            lua.PushNil();
            lua.SetField(-2, "__tostring");

            lua.PushNil();
            lua.SetField(-2, "__index");

            unsafe
            {
                lua.PushCFunction(&View__static__meta__gc);
            }
            lua.SetField(-2, "__gc");

            handles.ForEach((gCHandle) =>
            {
                gCHandle.Free();
            });
            handles = null;

            lua.Pop();
        }
        public void LoadShared(ILua lua)
        {
            lua.PushManagedFunction(Ultralight_Update);
            lua.SetField(-2, "Update");

            lua.PushManagedFunction(Ultralight_Render);
            lua.SetField(-2, "Render");

            lua.PushManagedFunction(View_new);
            lua.SetField(-2, "CreateView");
        }
    }
}
