#include "GarrysMod/Lua/Interface.h"
//#include <Ultralight/Ultralight.h>
#include <Ultralight/CAPI.h>
#include <string>
#ifdef _WIN64
#include <libloaderapi.h>
#elif __linux__ or __APPLE__
//#include <stdlib.h>
#include <dlfcn.h>
#endif

using namespace GarrysMod::Lua;
//using namespace ultralight;

typedef void (*MsgP)(const char*, ...);

MsgP Msg;

bool done = false;
void jojo() {
	done = true;
}

class App {
	ULRenderer renderer;
public:
	ULView view;
	App() {
		try {
			Msg("c++: App: Creating...\n");

			ULConfig config = ulCreateConfig();
			Msg("c++: App: Config created\n");

			ulConfigSetDeviceScaleHint(config, 2.0);
			ulConfigSetFontFamilyStandard(config, ulCreateString("Arial"));

			renderer = ulCreateRenderer(config);
			Msg("c++: App: Renderer created\n");

			view = ulCreateView(renderer, 2048, 2048, false); // https://github.com/ultralight-ux/Ultralight/issues/257#issuecomment-636330995
			Msg("c++: App: Renderer created view\n");

			ulViewResize(view, 512, 512);
			Msg("c++ App: view->Resize(512, 512)\n");
			ulViewSetFinishLoadingCallback(view, [](void* user_data, ULView caller) {jojo();}, nullptr);
			Msg("c++: App: view setted listener\n");
		}
		catch (const std::exception& e) {
			Msg(e.what());
		}
	}
	void SetURL(ULString url) {
		ulViewLoadURL(view, url);
	}
	~App() {
		Msg("c++: ~App\n");
		ulDestroyView(view);
		ulDestroyRenderer(renderer);
	}
	void Run() {
		Msg("c++: App: Run(): STARTED, Loading page\n");
		uint32_t timeout = 0;
		done = false;
		while (!done && timeout < 100000) {
			timeout++;
			ulUpdate(renderer);
		}
		Msg("c++: App: Run(): END, only ");
		Msg(std::to_string(timeout).c_str());
		Msg(" calls\n");
	}
	void OnFinishLoading() {
		done = true;
		Msg("c++: App: OnFinishLoading: START renderer->Render()\n");
		ulRender(renderer);
		ulBitmapWritePNG(ulViewGetBitmap(view), "result");
		Msg("c++: App: OnFinishLoading(): END\n");
	}
};

App* app;
void (App::* pRun)() = NULL; //https://stackoverflow.com/a/1486279/9765252
void (App::* pSetUrl)(ULString url) = NULL; //https://stackoverflow.com/a/1486279/9765252

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

	(*app.*pSetUrl)(ulCreateString(url));
	Msg("c++: app.SetURL() is done\n");
	(*app.*pRun)();
	Msg("c++: app.Run() is done\n");

	ULBitmap bitmap = ulViewGetBitmap(app->view);
	uint8_t* adress = (uint8_t*)ulBitmapLockPixels(bitmap);

	Msg("c++: Rendering on surface (width: ");
	Msg(std::to_string(ulBitmapGetWidth(bitmap)).c_str());
	Msg(", height: ");
	Msg(std::to_string(ulBitmapGetHeight(bitmap)).c_str());
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
	for (uint16_t y = 0; y < ulBitmapGetHeight(bitmap); y++)
	{
		for (uint16_t x = 0; x < ulBitmapGetWidth(bitmap); x++)
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

	ulBitmapUnlockPixels(bitmap); // maybe locking permanently disallow reusing it, Maybe all shit writed before not worked because it doesn't unlock bitmap
	Msg("c++: Render end\n");
	return 3;
}

LUA_FUNCTION(createApp) {
	app = new App();
	pRun = &App::Run;
	pSetUrl = &App::SetURL;
	Msg("c++: app created\n");
	return 0;
}
LUA_FUNCTION(destroyApp) {
	delete app;
	pRun = nullptr;
	pSetUrl = nullptr;
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
#elif __linux__ or _APPLE_
#ifdef _APPLE_
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

GMOD_MODULE_OPEN()
{
	Msg = reinterpret_cast<MsgP>(getFunction("tier0", "Msg"));

	Msg("c++: Module opening...\n");

	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);

	LUA->CreateTable(); // {

	LUA->PushCFunction(RenderImage);
	LUA->SetField(-2, "render");

	LUA->PushCFunction(createApp);
	LUA->SetField(-2, "createApp");

	LUA->PushCFunction(destroyApp);
	LUA->SetField(-2, "destroyApp");

	LUA->SetField(-2, "ultralight"); // }

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
	delete app;
	Msg = nullptr;
	return 0;
}
