using System;
using System.Runtime.InteropServices;

namespace GmodUltralight
{
	class SourceSDK
	{
		[DllImport("kernel32.dll")]
		public static extern int GetModuleHandle(string lpModuleName);

		[DllImport("kernel32.dll", EntryPoint = "LoadLibrary")]
		static extern int LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

		[DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
		static extern IntPtr GetProcAddress(int hModule, [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate IntPtr CreateInterfaceFn(string pName, int* pReturnCode);

		//CSysModule

		public unsafe CreateInterfaceFn Sys_GetFactory(int pModule)
		{
			if (pModule == 0)
				return null;

			return (CreateInterfaceFn)Marshal.GetDelegateForFunctionPointer(GetProcAddress(pModule, "CreateInterface"), typeof(CreateInterfaceFn));
		}

		public unsafe bool Sys_LoadInterface(
			string pModuleName,
			string pInterfaceVersionName,
			out int pOutModule,
			out IntPtr pOutInterface)
		{
			int pMod = GetModuleHandle(pModuleName);
			//int pMod = LoadLibrary(pModuleName);

			CreateInterfaceFn fn = Sys_GetFactory(pMod);

			pOutInterface = fn(pInterfaceVersionName, null);
			/*CSysModule* pMod = Sys_LoadModule(pModuleName);
			if ( !pMod )
				return false;

			CreateInterfaceFn fn = Sys_GetFactory(pMod);
			if ( !fn )
			{
				Sys_UnloadModule(pMod );
				return false;
			}

			* pOutInterface = fn(pInterfaceVersionName, NULL);
			if ( !( * pOutInterface ) )
			{
				Sys_UnloadModule(pMod );
				return false;
			}
			*/
			pOutModule = pMod;

			return true;
		}
	}
}
