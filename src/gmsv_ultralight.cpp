#include "GarrysMod/Lua/Interface.h"
#include <stdio.h>
#include <Ultralight/Ultralight.h>
#include <iostream>
#include <string>
//#include <memory>
#ifdef _WIN64
#include <libloaderapi.h>
#elif __linux__
//#include <stdlib.h>
#include <dlfcn.h>
#endif
using namespace GarrysMod::Lua;
using namespace ultralight;

typedef void (*MsgFn)(const char*, ...);

MsgFn Msg;
class MyApp : public LoadListener {
	RefPtr<Renderer> renderer_;
	bool done_ = false;
public:
	RefPtr<View> view_;
	MyApp() {
		try {
			Msg("c++: MyApp: Creating...\n");

			Config config;
			Msg("c++: MyApp: Config created\n");

			config.device_scale_hint = 1.0;
			config.font_family_standard = "Arial";
			Platform::instance().set_config(config);
			Msg("c++: MyApp: Platform setted config\n");

			renderer_ = Renderer::Create();
			Msg("c++: MyApp: Renderer created\n");

			view_ = renderer_->CreateView(512, 512, false); // https://github.com/ultralight-ux/Ultralight/issues/257
			Msg("c++: MyApp: Renderer created view\n");

			view_->set_load_listener(this);
			Msg("c++: MyApp: view setted listener\n");
		}
		catch (const std::exception& e) {
			Msg(e.what());
		}
	}
	void SetURL(String url) {
		view_->LoadURL(url);
	}
	virtual ~MyApp() {
		Msg("c++: MyApp: Shutting Down");
		view_ = nullptr; // IT DONT WORK.
		renderer_ = nullptr; // IT FUCKING DOESNT!!!
	}
	virtual void Run() {
		try {
			std::cout << "Starting Run(), waiting for page to load..." << std::endl;
			while (!done_)
				renderer_->Update();
			std::cout << "Finished." << std::endl;
		}
		catch (const std::exception& e) {
			Msg(e.what());
		}
	}
	virtual void OnFinishLoading(ultralight::View* caller) {
		try {
			Msg("c++: Page loaded");
			renderer_->Render();
			//view_->bitmap()->WritePNG("result.png");
			std::cout << "Saved a render of our page to result.png." << std::endl;
		}
		catch (const std::exception& e) {
			Msg(e.what());
		}
		done_ = true;
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
		const char* url = LUA->GetString();
		if (url == NULL) {
			url = "https://google.com";
		}
		(*app.*pSetUrl)(url);
		Msg("c++: app.SetURL() is done\n");
		(*app.*pRun)();
		Msg("c++: app.Run() is done\n");

		uint8_t* adress = (uint8_t*)app->view_->bitmap()->LockPixels();
		Msg("c++: Rendering on surface (width: ");
		Msg(std::to_string(app->view_->width()).c_str());
		Msg(", height: ");
		Msg(std::to_string(app->view_->width()).c_str());
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
		for (uint16_t y = 0; y < app->view_->height(); y++)
		{
			for (uint16_t x = 0; x < app->view_->width(); x++)
			{
				if (adress[i + 3] != 0) {
					LUA->GetField(-1, "SetDrawColor");
					LUA->PushNumber(adress[i + 2]);//R
					LUA->PushNumber(adress[i + 1]);//G
					LUA->PushNumber(adress[i]);//B
					LUA->PushNumber(adress[i + 3]);//A
					LUA->Call(4, 0);
					i = i + 4;
					LUA->GetField(-1, "DrawRect");
					LUA->PushNumber(x);
					LUA->PushNumber(y);
					LUA->PushNumber(1);
					LUA->PushNumber(1);
					LUA->Call(4, 0);; // https://github.com/ultralight-ux/Ultralight/issues/257
				}
			}
		}
		app->view_->bitmap()->UnlockPixels(); // maybe locking permanently disallow reusing it, Maybe all shit writed before not worked because it doesn't unlock bitmap
		Msg("c++: Render end");
		LUA->Pop();
	}
	catch (const std::exception& e) {
		Msg(e.what());
	}
	return 0;
}
LUA_FUNCTION(ultralight_async_render) {
	// TODO: implement this
	return 0;
}
GMOD_MODULE_OPEN()
{
#ifdef _WIN32
	Msg = reinterpret_cast<MsgFn>(GetProcAddress(GetModuleHandleA("tier0.dll"), "Msg"));
#elif __linux__
	Msg = reinterpret_cast<MsgFn>(dlsym(dlopen("tier0.so", RTLD_LAZY), "Msg"));
#endif
	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);

	Msg("c++: Module opening...\n");

	app = new MyApp();
	Msg("c++: app created\n");

	pRun = &MyApp::Run;
	pSetUrl = &MyApp::SetURL;

	LUA->PushString("ultralight_render");
	LUA->PushCFunction(RenderImage);
	LUA->SetTable(-3);

	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);

	LUA->PushString("ultralight_async_render");
	LUA->PushCFunction(ultralight_async_render);
	LUA->SetTable(-3);

	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);

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
	// process with running MyApp is still active, after disabling this :'C
	delete app; //app->~MyApp();
	return 0;
}
