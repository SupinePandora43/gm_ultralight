#include "GarrysMod/Lua/Interface.h"
#include <Ultralight/Ultralight.h>
#include <string>
#ifdef _WIN64
#include <libloaderapi.h>
#elif __linux__ or __APPLE__
//#include <stdlib.h>
#include <dlfcn.h>
#endif

using namespace GarrysMod::Lua;
using namespace ultralight;

typedef void (*MsgP)(const char*, ...);

MsgP Msg;
class MyApp : public LoadListener {
	RefPtr<Renderer> renderer;
	bool done_ = false;
public:
	RefPtr<View> view;
	MyApp() {
		try {
			Msg("c++: MyApp: Creating...\n");

			Config config;
			Msg("c++: MyApp: Config created\n");

			config.device_scale_hint = 1.0;
			config.font_family_standard = "Arial";
			Platform::instance().set_config(config);
			Msg("c++: MyApp: Platform setted config\n");

			renderer = Renderer::Create();
			Msg("c++: MyApp: Renderer created\n");

			view = renderer->CreateView(2048, 2048, false); // https://github.com/ultralight-ux/Ultralight/issues/257#issuecomment-636330995
			Msg("c++: MyApp: Renderer created view\n");

			view->Resize(512, 512);
			Msg("c++ MyApp: view->Resize(512, 512)\n");

			view->set_load_listener(this);
			Msg("c++: MyApp: view setted listener\n");
		}
		catch (const std::exception& e) {
			Msg(e.what());
		}
	}
	void SetURL(String url) {
		view->LoadURL(url);
	}
	RefPtr<View> getView() {
		return view;
	}
	~MyApp() {
		Msg("c++: ~MyApp\n");
		view = nullptr; // IT DONT WORK.
		renderer = nullptr; // IT FUCKING DOESNT!!!
	}
	void Run() {
		Msg("c++: MyApp: Run(): STARTED, Loading page\n");
		int timeout = 0;
		done_ = false;
		while (!done_) {
			timeout++;
			renderer->Update();
		}
		Msg("c++: MyApp: Run(): END, only ");
		Msg(std::to_string(timeout).c_str());
		Msg(" calls\n");
	}

	void OnFinishLoading(ultralight::View* caller) {
		done_ = true;
		Msg("c++: MyApp: OnFinishLoading: START renderer->Render()\n");
		renderer->Render();
		view->bitmap()->WritePNG("result.png");
		Msg("c++: MyApp: OnFinishLoading(): END\n");
	}
};

void (MyApp::* pRun)() = NULL; //https://stackoverflow.com/a/1486279/9765252
void (MyApp::* pSetUrl)(String url) = NULL; //https://stackoverflow.com/a/1486279/9765252
MyApp* app;

LUA_FUNCTION(RenderImage) {
	try {
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
		Msg(std::to_string(app->view->width()).c_str());
		Msg(" )\n");

		Msg("c++: MyApp: Color( ");
		Msg(std::to_string(adress[2]).c_str());
		Msg(",");
		Msg(std::to_string(adress[1]).c_str());
		Msg(",");
		Msg(std::to_string(adress[0]).c_str());
		Msg(",");
		Msg(std::to_string(adress[3]).c_str());
		Msg(" )");

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
				LUA->PushNumber(adress[i + 2]); // R
				LUA->PushNumber(adress[i + 1]); // G
				LUA->PushNumber(adress[i]); //     B
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

		app->view->bitmap()->UnlockPixels(); // maybe locking permanently disallow reusing it, Maybe all shit writed before not worked because it doesn't unlock bitmap
		Msg("c++: Render end\n");
		adress = nullptr;
	}
	catch (const std::exception& e) {
		Msg(e.what());
		Msg("\n");
	}
	return 3;
}

LUA_FUNCTION(createApp) {
	app = new MyApp();
	pRun = &MyApp::Run;
	pSetUrl = &MyApp::SetURL;
	Msg("c++: app created\n");
	return 0;
}
LUA_FUNCTION(destroyApp) {
	delete app;
	pRun = nullptr;
	pSetUrl = nullptr;
	return 0;
}

GMOD_MODULE_OPEN()
{
#ifdef _WIN32
	Msg = reinterpret_cast<MsgP>(GetProcAddress(GetModuleHandle("tier0.dll"), "Msg"));
#elif __linux__
	Msg = reinterpret_cast<MsgP>(dlsym(dlopen("libtier0.so", RTLD_LAZY), "Msg"));
#elif __APPLE__
	Msg = reinterpret_cast<MsgP>(dlsym(dlopen("libtier0.dylib", RTLD_LAZY), "Msg"));
#else
#error unknown platform
#endif

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
	// process with running MyApp is still active, after this :'C
	delete app;
	Msg = nullptr;
	return 0;
}
