#include "GarrysMod/Lua/Interface.h"
#include <string>
#include <thread>
#if defined(__linux__) || defined(__APPLE__)
#include <dlfcn.h> // dlsym
#include <cstring> // memcpy
#include <fcntl.h>     // for O_* constants
#include <sys/mman.h>  // mmap, munmap
#include <sys/stat.h>  // for mode constants
#include <unistd.h>    // unlink
#if defined(__APPLE__)
#include <errno.h>
#endif
#include <stdexcept>
#elif _WIN64
#include <libloaderapi.h> // GetProcAddres
#include <windows.h> // CreateFileMapping
#include <io.h>
#endif
#include "shoom/shm.h" // Shm

using namespace GarrysMod::Lua;

typedef void (*MsgP)(const char*, ...);
MsgP Msg;
Shm* ul_io_rpc;
Shm* ul_i_image;
Shm* ul_o_url;
uint16_t x = 1024;
uint16_t y = 1024;
std::thread* renderer;
/*
LUA_FUNCTION(RenderImage) {
	Msg("c++: RenderImage() called\n");
	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);
	//LUA->GetField(-2, "print");
	//LUA->PushString("c++: RenderImage() called");
	//LUA->Call(1, 0);
	LUA->CheckString(-2);
	const char* url = LUA->GetString(-2);
	if (url == NULL) {
		url = "https://google.com";
	}

	(*app.*pSetUrl)(url);
	Msg("c++: app.SetURL() is done\n");
	(*app.*pRun)();
	Msg("c++: app.Run() is done\n");

	uint8_t* adress = (uint8_t*)app->view->bitmap()->LockPixels();

	Msg("c++: Rendering on surface (width: ");
	Msg(std::to_string(app->view->width()).c_str());
	Msg(", height: ");
	Msg(std::to_string(app->view->height()).c_str());
	Msg(" )\n");

	// TODO:
	// gm_ultralight -> vguimatsurface
	// get `surface` from `vguimatsurface.dll`
	// Sys_GetFactory to get the factory and then get the interface from it
	// use https://github.com/ValveSoftware/source-sdk-2013/blob/master/mp/src/public/vgui/ISurface.h
	// CornerPin#6579 Garry's Mod #cpp 25.05.2020

	// gm_ultralight -> LUA -> vguimatsurface
	LUA->GetField(-1, "surface");
	uint32_t i = 0;
	for (uint16_t y = 0; y < app->view->height(); y++)
	{
		for (uint16_t x = 0; x < app->view->width(); x++)
		{
			LUA->GetField(-1, "SetDrawColor");
			LUA->PushNumber(adress[i]); //     R
			LUA->PushNumber(adress[i + 1]); // G
			LUA->PushNumber(adress[i + 2]); // B
			LUA->PushNumber(adress[i + 3]); // A
			LUA->Call(4, 0);
			i = i + 4;
			LUA->GetField(-1, "DrawRect");
			LUA->PushNumber(x);
			LUA->PushNumber(y);
			LUA->PushNumber(1);
			LUA->PushNumber(1);
			LUA->Call(4, 0);
		}
	}
	LUA->Pop();

	LUA->PushNumber(adress[2]);
	LUA->PushNumber(adress[1]);
	LUA->PushNumber(adress[0]);
	LUA->PushNumber(adress[3]);
	adress = nullptr;
	app->view->bitmap()->UnlockPixels(); // maybe locking permanently disallow reusing it, Maybe all shit writed before not worked because it doesn't unlock bitmap
	Msg("c++: Render end\n");
	return 3;
}
*/
LUA_FUNCTION(test) {
	Msg("1", "2", "3", std::string("ZA WARUDO").c_str());
	return 0;
}
void* getFunction(std::string library, const char* funcName) {
#ifdef _WIN64
	library = library + ".dll";
	HMODULE dll = GetModuleHandle(library.c_str());
	if (dll == nullptr) {
		dll = LoadLibrary(library.c_str());
	}
	return GetProcAddress(dll, funcName);
#elif __linux__ or __APPLE__
#ifdef __APPLE__
	const char* postfix = ".dylib";
#else
	const char* postfix = ".so";
#endif
	library = "lib" + library + postfix;
	void* lib = dlopen(library.c_str(), RTLD_LAZY);
	return dlsym(lib, funcName);
#else
#error unknown platform
#endif
}
/* ul_io_rpc
 * [ 0 ] - shutdown
 * [ 1 ] - pid TASKKILL
 * [ 2 ] - is rendered ?
*/
// ul_o_url - url
/* ul_i_image
 * [x * y * 4 ] exact as view->bitmap()->LockPixels()
*/
void rendererThread() {
	std::system("ultralight_renderer.exe");
}

#define FSLOG_PREFIX "gm: "
#include "log/fslog.h"

LUA_FUNCTION(StartRendererThread) {
	LOG("checking renderer");
	if (renderer != nullptr) {
		LOG("renderer != nullptr");
		Msg("c++: renderer already created >:");
		return 0;
	}
	LOG("checking rpc");
	if (ul_io_rpc == nullptr) {
		LOG("ul_io_rpc == nullptr");
		Msg("c++: shoom first!");
		return 0;
	}
	LOG("checking ul_o_url");
	if (ul_o_url == nullptr) {
		LOG("ul_o_url == nullptr");
		Msg("c++: shoom first!");
		return 0;
	}
	LOG("Starting renderer");
	Msg("c++: Starting renderer\n");
	renderer = new std::thread(rendererThread);
	LOG("Renderer thread started");
	Msg("c++: Renderer thread started");
	return 0;
}
LUA_FUNCTION(SetUrl) {
	try {
		if (ul_o_url == nullptr) {
			LOG("SetUrl @ ul_o_url == nullptr");
			Msg("c++: sry, initialize first!");
			return 0;
		}
		//LUA->CheckString();
		const char* url = LUA->GetString();
		LOG(url);
		//if (url == nullptr || url == NULL) {
		//	url = "https://github.com";
		//}
		//LOG("copying");
		//Msg("copying");
		memcpy(ul_o_url->Data(), url, std::strlen(url));
		//LOG("done");
		//Msg("done");
	}
	catch (std::exception e) {
		LOG(e.what());
	}
	return 0;
}
LUA_FUNCTION(shoom) {
	LOG("Initializing shoom");
	Msg("c++: Starting IPC\n");
	LOG("Initializing ul_io_rpc");
	ul_io_rpc = new Shm{ "ul_io_rpc", 128 };
	ul_io_rpc->Create();
	LOG("Initializing ul_o_url");
	ul_o_url = new Shm{ "ul_o_url", 512 };
	ul_o_url->Create();
	LOG("Initializing ul_i_image");
	ul_i_image = new Shm{ "ul_i_image", (size_t)(512 * 512 * 4 )+1 };
	LOG("Initialized shoom");
	return 0;
}
LUA_FUNCTION(UpdateRenderResult) {
	ul_i_image->Open();
	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);
	LUA->GetField(-1, "surface");
	uint32_t i = 0;
	uint8_t* address = ul_i_image->Data();
	for (uint16_t y = 0; y < 512; y++) //https://github.com/danielga/sourcesdk-minimal/blob/403f18104139472451a2b3518973fadeaf9691cf/tier1/interface.cpp#L472
	{ // https://github.com/danielga/sourcesdk-minimal/blob/403f18104139472451a2b3518973fadeaf9691cf/tier1/interface.cpp#L423
		for (uint16_t x = 0; x < 512; x++)
		{
			LUA->GetField(-1, "SetDrawColor");
			LUA->PushNumber(address[i]); //     R
			LUA->PushNumber(address[i + 1]); // G
			LUA->PushNumber(address[i + 2]); // B
			LUA->PushNumber(address[i + 3]); // A
			LUA->Call(4, 0);
			i = i + 4;
			LUA->GetField(-1, "DrawRect");
			LUA->PushNumber(x);
			LUA->PushNumber(y);
			LUA->PushNumber(1);
			LUA->PushNumber(1);
			LUA->Call(4, 0);
		}
	}
	address = nullptr;
	LUA->Pop();
	Msg("c++: Render end\n");
	return 0;
}

GMOD_MODULE_OPEN()
{
	Msg = reinterpret_cast<MsgP>(getFunction("tier0", "Msg"));

	Msg("c++: Module opening...\n");

	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);

	LUA->PushCFunction(SetUrl);
	LUA->SetField(-2, "SetUrl");

	//LUA->CreateTable(); // {

	LUA->PushNumber(1);
	LUA->SetField(-2, "delay");

	LUA->PushCFunction(shoom);
	LUA->SetField(-2, "ul_shoom");

	LUA->PushCFunction(UpdateRenderResult);
	LUA->SetField(-2, "ul_UpdateRenderResult");

	LUA->PushCFunction(StartRendererThread);
	LUA->SetField(-2, "ul_StartRendererThread");

	LUA->PushCFunction(test);
	LUA->SetField(-2, "test");

	//LUA->SetField(-2, "ultralight"); // }

	LUA->GetField(-1, "SERVER");
	if (LUA->GetBool(-1)) {
		Msg("c++: SERVER\n");
	}
	LUA->Pop();

	LUA->GetField(-1, "CLIENT");
	if (LUA->GetBool(-1)) {
		Msg("c++: CLIENT\n");
	}
	LUA->Pop();
	return 0;
}

GMOD_MODULE_CLOSE()
{
	Msg = nullptr;
	if (renderer != nullptr) renderer->detach();
	if (ul_i_image) delete ul_i_image;
	if (ul_io_rpc) { ul_io_rpc->Data()[0] = 1; delete ul_io_rpc; }
	if (ul_o_url) delete ul_o_url;
	return 0;
}
