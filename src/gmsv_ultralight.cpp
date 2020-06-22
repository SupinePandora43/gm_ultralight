// PERFORMANCE
// ^^^^^^^^^^^ mean linking tier1 / premake
// !PERFORMANCE = not link / cmake
#include <string>
#include <thread>
#include <vector>
#include <cstring>
#include "GarrysMod/Lua/Interface.h"
#include "shoom/shm.h"

#ifdef PERFORMANCE
#include <interface.h>
#include <vgui/ISurface.h>
#endif

#define FSLOG_PREFIX "gm: "
#include "log/fslog.h"

using namespace GarrysMod::Lua;

Shm* ul_o_rpc;
Shm* ul_i_rpc;
Shm* ul_o_createview;
std::thread* renderer;

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

/* ul_o_rpc
 * [ 0 ] - shutdown
 * [ 1 ] - render
*/
/* ul_i_rpc
 * unused ?
*/

#define URLLEN 512

class IView {
	Shm* SHMwidth;
	Shm* SHMheight;
	Shm* SHMurl;
	Shm* SHMisloaded;
	Shm* SHMsync;
public:
	Shm* image;
	uint32_t width = 0;
	uint32_t height = 0;
	uint8_t sync = 0;
	IView(uint16_t id, uint32_t iwidth, uint32_t iheight) {
		width = iwidth;
		height = iheight;

		SHMwidth = new Shm{ std::string("ul_o_width_").append(std::to_string(id)), 64 };
		SHMheight = new Shm{ std::string("ul_o_height_").append(std::to_string(id)), 64 };

		SHMwidth->Create();
		SHMheight->Create();

		memcpy(SHMwidth->Data(), std::to_string(width).c_str(), std::to_string(width).length());
		memcpy(SHMheight->Data(), std::to_string(height).c_str(), std::to_string(height).length());

		SHMurl = new Shm{ std::string("ul_o_url_").append(std::to_string(id)), URLLEN };
		SHMurl->Create();

		image = new Shm{ std::string("ul_i_image_").append(std::to_string(id)), 1 + (width * height * 4) };

		SHMisloaded = new Shm{ std::string("ul_i_isloaded_").append(std::to_string(id)), 16 };
		SHMsync = new Shm{ std::string("ul_i_sync_").append(std::to_string(id)), 16 };
	}
	bool HasNewFrame() {
		SHMsync->Open();
		if (SHMsync->Data() != nullptr && SHMsync->Data()[0] != sync) {
			sync = SHMsync->Data()[0];
			return true;
		}
		return false;
	}
	void SetURL(std::string url) {
		for (int i = 0;i < URLLEN;i++) {
			SHMurl->Data()[i] = 0;
		}
		memcpy(SHMurl->Data(), url.c_str(), url.length());
	}
	bool IsLoaded() {
		SHMisloaded->Open();
		return SHMisloaded->Data() != nullptr ? SHMisloaded->Data()[0] : false;
	}
	uint8_t* GetImageResult() {
		image->Open();
		return image->Data();
	}
	~IView() {
		delete SHMwidth;
		delete SHMheight;
		delete SHMurl;
		delete SHMisloaded;
		delete SHMsync;
		delete image;
	}
};
std::vector<IView*> views;
uint16_t viewcount = 0;

#ifdef PERFORMANCE
vgui::ISurface* surface;
#else
typedef void (*MsgP)(const char*, ...);
MsgP Msg;
#endif

void rendererThread() {
	std::system("ultralight_renderer.exe");
}
LUA_FUNCTION(Start) {
	//bool force = LUA->GetBool(1);
	//if (renderer != nullptr && !force) {
	//	Msg("c++: already started\n");
	//}
	//else {
	renderer = new std::thread(rendererThread);
	//}
	return 0;
}
LUA_FUNCTION(CreateView) {
	LOG("getting numbers");
	uint32_t width = LUA->GetNumber(1);
	uint32_t height = LUA->GetNumber(2);
	if (ul_o_createview->Data() != nullptr) {
		LOG("creating view");
		IView* view = new IView(viewcount, width, height);
		LOG("returning id of view");
		LUA->PushNumber((double)viewcount);
		LOG("setting ul_o_createview->Data()");
		ul_o_createview->Data()[viewcount] = 1;
		LOG("viewcount++");
		viewcount++;
		LOG("push_back");
		views.push_back(view);
	}
	else {
		LOG("ul_o_createview->Data() == nullptr");
		Msg("c++: ul_o_createview->Data() == nullptr");
		LUA->PushNil();
	}
	LOG("return 1");
	return 1;
}
LUA_FUNCTION(SetURL) {
	uint8_t id = LUA->GetNumber(1);
	char* url = (char*)LUA->GetString(2);
	views.at(id)->SetURL(url);
	return 0;
}
// OnFinishLoading
LUA_FUNCTION(IsLoaded) {
	uint8_t id = LUA->GetNumber(1);
	LUA->PushBool(views.at(id)->IsLoaded());
	return 1;
}
LUA_FUNCTION(viewsSize) {
	Msg(std::to_string(views.size()).c_str());
	LUA->PushNumber(views.size());
	return 1;
}

LUA_FUNCTION(RenderView) {
	uint8_t id = LUA->GetNumber(1);
	// view->is_bitmap_dirty()
	if (views.at(id)->HasNewFrame()) {
#ifdef PERFORMANCE
		if (surface == nullptr) {
			Msg("c++: surface==nullptr");
			LOG("surface==nullptr");
			return 0;
		}
#else
		LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);
		LUA->GetField(-1, "surface");
#endif
		uint32_t i = 0;
		uint8_t* address = views.at(id)->GetImageResult();
		if (address == nullptr)return 0;
		for (uint32_t y = 0; y < views.at(id)->height; y++)
		{
			for (uint32_t x = 0; x < views.at(id)->width; x++)
			{
#ifdef PERFORMANCE
				surface->DrawSetColor(address[0], address[1], address[2], address[3]);
				surface->DrawFilledRect(x, y, 1, 1);
#else
				// https://www.youtube.com/watch?v=WDiB4rtp1qw
				LUA->GetField(-1, "SetDrawColor");
				LUA->PushNumber(address[i]); //     R
				LUA->PushNumber(address[i + 1]); // G
				LUA->PushNumber(address[i + 2]); // B
				LUA->PushNumber(address[i + 3]); // A
				LUA->Call(4, 0);
				LUA->GetField(-1, "DrawRect");
				LUA->PushNumber(x);
				LUA->PushNumber(y);
				LUA->PushNumber(1);
				LUA->PushNumber(1);
				LUA->Call(4, 0);
#endif
				i = i + 4;
			}
		}
#ifndef PERFORMANCE
		LUA->Pop();
#endif
		address = nullptr;
		Msg("c++: Render end\n");
	}
	return 0;
}

GMOD_MODULE_OPEN()
{
	LOG("opening");

#ifdef PERFORMANCE
	if (!Sys_LoadInterface("vguimatsurface", "VGUI_Surface030", NULL, (void**)&surface)) {
		Msg("c++: failed to load matsurface");
		LOG("failed to load matsurface");
	}
#else
	Msg = reinterpret_cast<MsgP>(getFunction("tier0", "Msg"));
#endif
	Msg("c++: Module opening...\n");
	LOG("opening isurface");
	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);

	ul_o_rpc = new Shm{ "ul_o_rpc", 128 };
	ul_o_rpc->Create();
	ul_i_rpc = new Shm{ "ul_i_rpc", 128 };

	ul_o_createview = new Shm{ "ul_o_createview", 200 };
	ul_o_createview->Create();

	LUA->PushCFunction(Start);
	LUA->SetField(-2, "ul_Start");

	LUA->PushCFunction(CreateView);
	LUA->SetField(-2, "ul_CreateView");

	LUA->PushCFunction(SetURL);
	LUA->SetField(-2, "ul_SetURL");

	LUA->PushCFunction(IsLoaded);
	LUA->SetField(-2, "ul_IsLoaded");

	LUA->PushCFunction(RenderView);
	LUA->SetField(-2, "ul_RenderView");

	LUA->PushCFunction(viewsSize);
	LUA->SetField(-2, "ul_size");

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
#ifdef PERFORMANCE
	surface = nullptr;
#else
	Msg = nullptr;
#endif
	if (renderer != nullptr) renderer->detach();
	if (ul_o_rpc) { ul_o_rpc->Data()[0] = 1; delete ul_o_rpc; }
	if (ul_i_rpc)delete ul_i_rpc;
	if (ul_o_createview)delete ul_o_createview;
	return 0;
}
