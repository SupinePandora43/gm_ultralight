#include "GarrysMod/Lua/Interface.h"
#include <string>
#include <thread>
#include <vector>
#ifdef PERFORMANCE
#include <interface.h>
#include <vgui/ISurface.h>
#endif
#include "shoom/shm.h"

using namespace GarrysMod::Lua;

Shm* ul_o_rpc;
Shm* ul_i_rpc;
Shm* ul_o_render;
Shm* ul_o_createview;
std::thread* renderer;

void* getFunction(std::string library, const char* funcName) {
#ifdef _WIN64
	library = library + ".dll";
	HMODULE dll = GetModuleHandle(library.c_str());
	if (dll == nullptr) {
		dll = LoadLibrary(library.c_str());
	}
	return (void*)GetProcAddress(dll, funcName);
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
 * [ 2 .. 127 ] - create view id
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
		SHMisloaded = new Shm{ std::string("ul_i_isloaded_").append(std::to_string(id)), 16 };
	}
	void SetURL(char* url) {
		memcpy(SHMurl->Data(), url, std::string(url).length());
	}
	uint8_t* Get() {
		image->Open();
		return image->Data();
	}
	bool IsLoaded() {
		SHMisloaded->Open();
		return SHMisloaded->Data()[0];
	}
	~IView() {
		delete SHMwidth;
		delete SHMheight;
		delete SHMurl;
		delete image;
		delete SHMisloaded;
	}
};
std::vector<IView> views;
uint8_t viewcount = 0;

#ifdef PERFORMANCE
vgui::ISurface* surface;
#else
typedef void (*MsgP)(const char*, ...);
MsgP Msg;
#endif

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
	ul_o_createview->Data()[viewcount] = viewcount;
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
LUA_FUNCTION(UpdateUntilLoads) {
	uint8_t id = LUA->GetNumber(-1);

	return 0;
}
LUA_FUNCTION(IsLoaded) {
	uint8_t id = LUA->GetNumber(-1);
	if (id < views.size()) {
		LUA->PushBool(views.at(id).IsLoaded());
		return 1;
	}
	else {
		Msg("c++: not exists\n");
		return 0;
	}
}
LUA_FUNCTION(Render) {
	ul_o_render->Data()[0] = 1;
	return 0;
}
LUA_FUNCTION(RenderView) {
	uint8_t id = LUA->GetNumber(-1);
#ifdef PERFORMANCE
	if (surface == nullptr) {
		Msg("c++: surface==nullptr");
		LOG("surface==nullptr");
	}
#else
	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);
	LUA->GetField(-1, "surface");
#endif
	if (id < views.size()) {
		uint32_t i = 0;
		uint8_t* address = views.at(id).Get();
		for (uint16_t y = 0; y < views.at(id).height; y++)
		{
			for (uint16_t x = 0; x < views.at(id).width; x++)
			{
#ifdef PERFORMANCE
				surface->DrawSetColor(address[0], address[1], address[2], address[3]);
				surface->DrawFilledRect(x, y, 1, 1);
#else
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
	ul_o_createview = new Shm{ "ul_o_render", 255 };
	ul_o_createview->Create();
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
