using GmodNET.API;
using System;
using System.Linq;

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

			lua.PushNil();
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

			lua.CreateTable();
			lua.PushNumber(0);
			lua.SetField(-2, "None");
			lua.PushNumber(1);
			lua.SetField(-2, "Left");
			lua.PushNumber(2);
			lua.SetField(-2, "Middle");
			lua.PushNumber(3);
			lua.SetField(-2, "Right");
			lua.SetField(-2, "MouseButton");

			lua.CreateTable();
			lua.PushNumber(0);
			lua.SetField(-2, "MouseMoved");
			lua.PushNumber(1);
			lua.SetField(-2, "MouseDown");
			lua.PushNumber(2);
			lua.SetField(-2, "MouseUp");
			lua.SetField(-2, "MouseEventType");

			lua.CreateTable();
			lua.PushNumber(0);
			lua.SetField(-2, "KeyDown");
			lua.PushNumber(1);
			lua.SetField(-2, "KeyUp");
			lua.PushNumber(2);
			lua.SetField(-2, "RawKeyDown");
			lua.PushNumber(3);
			lua.SetField(-2, "Char");
			lua.SetField(-2, "KeyEventType");

			lua.CreateTable();
			Type keyCodeEnums = typeof(KeyCode);
			string[] enumNames = keyCodeEnums.GetEnumNames();
			foreach (string s in enumNames)
			{
				lua.PushNumber((int)Enum.Parse(keyCodeEnums, s));
				lua.SetField(-2, s);
			}
			lua.SetField(-2, "KeyCode");

			lua.CreateTable();
			lua.PushNumber((byte)Modifiers.kMod_AltKey);
			lua.SetField(-2, "AltKey");
			lua.PushNumber((byte)Modifiers.kMod_CtrlKey);
			lua.SetField(-2, "CtrlKey");
			lua.PushNumber((byte)Modifiers.kMod_MetaKey);
			lua.SetField(-2, "MetaKey");
			lua.PushNumber((byte)Modifiers.kMod_ShiftKey);
			lua.SetField(-2, "ShiftKey");
			lua.PushNumber(0);
			lua.SetField(-2, "None");
			lua.SetField(-2, "Modifiers");

			lua.CreateTable();
			lua.PushNumber(0);
			lua.SetField(-2, "ScrollByPixel");
			lua.PushNumber(1);
			lua.SetField(-2, "ScrollByPage");
			lua.SetField(-2, "ScrollEventType");
		}
		public static void Unload_Shared(ILua lua)
		{
			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.PushNil();
			lua.SetField(-2, "Ultralight");
			lua.Pop();
		}
	}
}
