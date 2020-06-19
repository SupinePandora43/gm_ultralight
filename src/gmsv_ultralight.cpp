#include "GarrysMod/Lua/Interface.h"
#include <string>
#include <thread>
#include <vector>
#include <interface.h>
#include <vgui/ISurface.h>
#include "shoom/shm.h"

// backup
// https://github.com/SupinePandora43/gm_ultralight/blob/e9535c7598d3db17c7a063d2a9a80091d34bd7fb/src/gmsv_ultralight.cpp

using namespace GarrysMod::Lua;

Shm* ul_o_rpc;
Shm* ul_i_rpc;
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
*/
/* ul_i_rpc
 * [ 0 ] - pid TASKKILL
 * [ 1 ] - is rendered ?
*/
// ul_o_url_%id
/* ul_i_image_%id
 * [x * y * 4 ] exact as view->bitmap()->LockPixels()
*/

#define URLLEN 512

class IView {
	Shm* SHMwidth;
	Shm* SHMheight;
	Shm* SHMurl;
public:
	Shm* image;
	uint32_t width = 0;
	uint32_t height = 0;
	IView(uint8_t id, uint32_t iwidth, uint32_t iheight) {
		width = iwidth;
		SHMwidth = new Shm{ std::string("ul_o_width_").append(std::to_string(id)), 64 };
		SHMwidth->Create();
		memcpy(SHMwidth->Data(), std::to_string(iwidth).c_str(), std::to_string(iwidth).length());
		height = iheight;
		SHMheight = new Shm{ std::string("ul_o_height_").append(std::to_string(id)), 64 };
		SHMheight->Create();
		memcpy(SHMheight->Data(), std::to_string(iheight).c_str(), std::to_string(iheight).length());
		SHMurl = new Shm{ std::string("ul_o_url_").append(std::to_string(id)), URLLEN };
		SHMurl->Create();
		image = new Shm{ std::string("ul_i_image_").append(std::to_string(id)), 1 + (iwidth * iheight * 4) };
	}
	void SetURL(char* url) {
		memcpy(SHMurl->Data(), url, std::string(url).length());
	}
	uint8_t* Get() {
		image->Open();
		return image->Data();
	}
	~IView() {
		delete SHMwidth;
		delete SHMheight;
		delete SHMurl;
		delete image;
	}
};
std::vector<IView> views;
uint8_t viewcount = 0;
vgui::ISurface* surface;

#define FSLOG_PREFIX "gm: "
#include "log/fslog.h"

void rendererThread() {
	std::system("ultralight_renderer.exe");
}
LUA_FUNCTION(StartRendererThread) {
	renderer = new std::thread(rendererThread);
	return 0;
}
LUA_FUNCTION(CreateView) {
	uint32_t width = LUA->GetNumber(1);
	uint32_t height = LUA->GetNumber(2);
	IView view(viewcount, width, height);
	LUA->PushNumber(viewcount);
	viewcount++;
	return 1;
}
LUA_FUNCTION(SetURL) {
	uint8_t id = LUA->GetNumber(-1);
	char* url = (char*)LUA->GetString(-2);
	if (id < views.size()) {
		views.at(id).SetURL(url);
	}
	else {
		Msg("c++: not exists\n");
	}
	return 0;
}
LUA_FUNCTION(RenderView) {
	if (surface == nullptr) {
		Msg("c++: surface==nullptr");
		LOG("surface==nullptr");
	}
	uint8_t id = LUA->GetNumber(-1);
	if (id < views.size()) {
		uint32_t i = 0;
		uint8_t* address = views.at(id).Get();
		for (uint16_t y = 0; y < views.at(id).height; y++)
		{
			for (uint16_t x = 0; x < views.at(id).width; x++)
			{
				surface->DrawSetColor(address[0], address[1], address[2], address[3]);
				surface->DrawFilledRect(x, y, 1, 1);
				i = i + 4;
			}
		}
		address = nullptr;
		Msg("c++: Render end\n");
	}
	else {
		Msg("c++: id > views.size()");
		LOG("id > views.size()");
	}
	return 0;
}
GMOD_MODULE_OPEN()
{
	LOG("opening");
	Msg("c++: Module opening...\n");
	LOG("opening isurface");
	if (!Sys_LoadInterface("vguimatsurface", "VGUI_Surface030", NULL, (void**)&surface)) {
		Msg("c++: failed to load matsurface");
		LOG("failed to load matsurface");
	}
	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);

	ul_o_rpc = new Shm{ "ul_o_rpc", 128 };
	ul_o_rpc->Create();
	ul_i_rpc = new Shm{ "ul_i_rpc", 128 };

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
	surface = nullptr;
	if (renderer != nullptr) renderer->detach();
	if (ul_o_rpc) { ul_o_rpc->Data()[0] = 1; delete ul_o_rpc; }
	if (ul_i_rpc) { delete ul_i_rpc; }
	return 0;
}
