#include "GarrysMod/Lua/Interface.h"
#include <stdio.h>
#include <Ultralight/Ultralight.h>
#include <iostream>
#include <string>
//#include <memory>
#ifdef _DEBUG
#ifdef _WIN64
#include <libloaderapi.h> // hook "tier0.dll"'s Msg function
#elif __linux__
//#include <stdlib.h>
#include <dlfcn.h>
#endif
#endif
using namespace GarrysMod::Lua;
using namespace ultralight;

typedef void* (__cdecl* MsgFn)(const char*, ...);
MsgFn Msg;
/*#ifdef _WIN64
#else
void Msg(const char*, ...) {}
#endif*/

class MyApp : public LoadListener {
	RefPtr<Renderer> renderer_;
	bool done_ = false;
public:
	RefPtr<View> view_;
	MyApp() {
		Msg("c++: MyApp: Creating...\n");

		Config config;
		Msg("c++: MyApp: Config created\n");

		config.device_scale_hint = 1.0;
		config.font_family_standard = "Arial";
		Platform::instance().set_config(config);
		Msg("c++: MyApp: Platform setted config\n");

		renderer_ = Renderer::Create();
		Msg("c++: MyApp: Renderer created\n");

		view_ = renderer_->CreateView(256, 256, false);
		Msg("c++: MyApp: Renderer created view\n");

		view_->set_load_listener(this);
		Msg("c++: MyApp: view setted listener\n");

		//Msg("c++: MyApp: view loaded url\n");
	}
	void SetURL() {
		this->SetURL("https://google.com");
	}
	void SetURL(String url) {
		view_->LoadURL(url);
	}
	virtual ~MyApp() {
		view_ = nullptr;
		renderer_ = nullptr;
	}

	void Run() {
		std::cout << "Starting Run(), waiting for page to load..." << std::endl;
		while (!done_)
			renderer_->Update();
		std::cout << "Finished." << std::endl;
	}

	virtual void OnFinishLoading(ultralight::View* caller) {
		Msg("c++: Page loaded");
		renderer_->Render();
		view_->bitmap()->WritePNG("result.png");
		std::cout << "Saved a render of our page to result.png." << std::endl;
		done_ = true;
	}
};

LUA_FUNCTION(RenderImage) {
	LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);
	Msg("c++: RenderImage() called\n");
	//LUA->GetField(-2, "print");
	//LUA->PushString("c++: RenderImage() called");
	//LUA->Call(1, 0);
	MyApp app;
	Msg("c++: app created\n");
	app.SetURL();
	Msg("c++: app.SetURL() is done\n");
	app.Run();
	Msg("c++: app.Run() is done\n");

	uint8_t* adress = (uint8_t*)app.view_->bitmap()->LockPixels();
	//adress = view_->bitmap()->raw_pixels();
	//size_t sizeofadress = sizeof((uint8_t)adress);
	Msg("c++: Rendering on surface (width: "); //#include <string> 
	Msg(std::to_string(app.view_->width()).c_str());
	Msg(", height: ");
	Msg(std::to_string(app.view_->width()).c_str());
	Msg(" )\n");
	LUA->GetField(-1, "surface");
	uint32_t i = 0;
	for (uint16_t y = 0; y < app.view_->height(); y++)
	{
		for (uint16_t x = 0; x < app.view_->width(); x++)
		{
			LUA->GetField(-1, "SetDrawColor");
			LUA->PushNumber(adress[i + 2]);//R
			LUA->PushNumber(adress[i + 1]);//G
			LUA->PushNumber(adress[i]);//B
			LUA->PushNumber(adress[i + 3]);//A
			i = i + 4;
			LUA->Call(4, 0);
			LUA->GetField(-1, "DrawRect");
			LUA->PushNumber(x);
			LUA->PushNumber(y);
			LUA->PushNumber(1);
			LUA->PushNumber(1);
			LUA->Call(4, 0);
		}
	}
	Msg("c++: Render end");
	LUA->Pop();

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
	LUA->PushString("ultralight_render");
	LUA->PushCFunction(RenderImage);
	//LUA->SetField(-3, "ultralight_render");
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

	Msg("c++: CLIENT\n");
	return 0;
}

GMOD_MODULE_CLOSE()
{
	return 0;
}