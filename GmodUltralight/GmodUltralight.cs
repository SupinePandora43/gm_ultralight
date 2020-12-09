using GmodNET.API;
using System;
using System.IO;
using ImpromptuNinjas.UltralightSharp.Safe;
using System.Collections.Generic;
using ImpromptuNinjas.UltralightSharp.Enums;
using System.Diagnostics;

namespace GmodUltralight
{
	public partial class GmodUltralight : GmodNET.API.IModule
	{
		public string ModuleName => "Ultralight";
		public string ModuleVersion => "0.1.1";

		LoggerLogMessageCallback cb;
		Logger logger;
		Renderer renderer;
		Dictionary<string, View> views;

		static int View_TypeId;
		private static void LOG(string msg)
		{
			StreamWriter writer = new("./fslogcs.txt", true);
			writer.WriteLine(msg);
			writer.Close();
			Console.WriteLine(msg);
		}

		private static void LoggerCallback(LogLevel logLevel, string msg)
		{
			Debug.WriteLine($"{logLevel}: {msg}");
			LOG(msg);
		}
		public void Load(ILua lua, bool is_serverside, ModuleAssemblyLoadContext assembly_context)
		{
			// TODO: really? LogMessage = LoggerCallback
			cb = LoggerCallback;
			logger = new Logger
			{
				LogMessage = cb
			};
			Config cfg = new();
			cfg.SetUseGpuRenderer(false);           // we're using cpu renderer, because idk how to write gpu driver with output to uint8_t*
			cfg.SetResourcePath("./garrysmod/lua/bin/Modules/GmodUltralight/resources/"); // where cacert.pem is
																						  //AppCore.EnableDefaultLogger("./");
			AppCore.EnablePlatformFileSystem("./"); // AppCore's GetPlatformFileSystem()
			AppCore.EnablePlatformFontLoader();     // AppCore's GetPlatformFontLoader()
			Ultralight.SetLogger(logger);           // log ul's messages to file
			renderer = new Renderer(cfg);
			views = new Dictionary<string, View>();



			Load_View_Shared(lua);



			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.GetField(-1, "util");
			lua.GetField(-1, "AddNetworkString");
			lua.PushString("Ultralight_DrawSingle");
			lua.MCall(1, 0);
			lua.Pop();



			lua.PushSpecial(SPECIAL_TABLES.SPECIAL_GLOB);
			lua.CreateTable();

			// shared.cs
			LoadShared(lua);

			lua.SetField(-2, "Ultralight");

			lua.Pop();
		}
		public void Unload(ILua lua)
		{
			Console.WriteLine("UL: Goodbye world!");

			// release all lua refereces
			Unload_View_Shared(lua);

			// release all native references
			views = null;
			renderer.Dispose();
			renderer = null;
			cb = null;
		}
	}
}
