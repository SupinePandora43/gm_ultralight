﻿#include "GarrysMod/Lua/Interface.h"
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

#ifdef _WIN64
typedef void* (__cdecl* MsgFn)(const char*, ...);
MsgFn Msg;
#else
void Msg(const char*, ...) {}
#endif

class MyApp : public LoadListener {
	RefPtr<Renderer> renderer_;
	RefPtr<View> view_;
	lua_State *lua_ = nullptr;
	bool done_ = false;
	uint8_t* adr_ = nullptr;
public:
	MyApp() {
		Msg("c++: MyApp: Creating...\n");

		Config config;
		Msg("c++: MyApp: Config created\n");

		config.device_scale_hint = 2.0;
		config.font_family_standard = "Arial";
		Platform::instance().set_config(config);
		Msg("c++: MyApp: Platform setted config\n");

		renderer_ = Renderer::Create();
		Msg("c++: MyApp: Renderer created\n");

		view_ = renderer_->CreateView(64, 64, false);
		Msg("c++: MyApp: Renderer created view\n");

		view_->set_load_listener(this);
		Msg("c++: MyApp: view setted listener\n");

		//Msg("c++: MyApp: view loaded url\n");
	}
	
	void SetState(lua_State *L)
	{
		lua_ = (lua_State *)L;
	}
	
	void SetBitmap()
	{
		if(view_ != nullptr)
		{
			adr_ = (uint8_t*)view_->bitmap()->LockPixels();
		}
	}
	
	void SetURL()
	{
		this->SetURL("https://google.com");
	}
	
	void SetURL(String url) {
		view_->LoadURL(url);
	}
	
	void GetField(const char *name)
	{
		if(lua_ != nullptr && name != nullptr)
		{
			lua_->GetField(-1, name);
		}
	}
	
	void SetDrawColor(uint32_t i)
	{
		if(lua_ != nullptr && adr_ != nullptr)
		{
			GetField("SetDrawColor")
			lua_->PushNumber(adr_[i + 2]);//R
			lua_->PushNumber(adr_[i + 1]);//G
			lua_->PushNumber(adr_[i + 0]);//B
			lua_->PushNumber(adr_[i + 3]);//A
			lua_->Call(4, 0);
		}
	}
	
	void DrawPixel(uint16_t x, uint16_t y)
	{
		if(lua_ != nullptr)
		{
			GetField("DrawRect")
			lua_->PushNumber(x);
			lua_->PushNumber(y);
			lua_->PushNumber(1);
			lua_->PushNumber(1);
			lua_->Call(4, 0);
		}
	}
	
	virtual ~MyApp()
	{
		view_ = nullptr;
		renderer_ = nullptr;
		adr_ = nullptr;
	}

	void Run()
	{
		std::cout << "Starting Run(), waiting for page to load..." << std::endl;
		while (!done_)
			renderer_->Update();
		std::cout << "Finished." << std::endl;
	}
	
	void Draw()
	{
		GetField("surface")
		uint16_t w = view_->width();
		uint16_t h = view_->height();
		for (uint16_t y = 0; y < h; y++)
		{
			for (uint16_t x = 0; x < w; x++)
			{
				SetDrawColor((uint32_t)x * 4 + (uint32_t)y * w);
				DrawPixel(x, y);
			}
		}
	}
	
	virtual void OnFinishLoading(ultralight::View* caller)
	{
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
	app.SetBitmap()
	Msg("c++: app.SetBitmap() is done\n");
	app.SetState(LUA);
	Msg("c++: app.SetState() is done\n");
	//adress = view_->bitmap()->raw_pixels();
	//size_t sizeofadress = sizeof((uint8_t)adress);
	app.Draw();
	LUA->Pop();
	return 0;
}
/*
int MyExampleFunction(lua_State* state)
{
	ILuaBase * LUA = state->luabase;
	if (LUA->IsType(1, Type::Number))
	{
		MyApp app;
		app.Run();
		char strOut[512];
		float fNumber = LUA->GetNumber(1);
		sprintf(strOut, "Thanks for the number - I love %f!!", fNumber);
		LUA->PushSpecial(GarrysMod::Lua::SPECIAL_GLOB);
		LUA->GetField(-1, "print");
		LUA->PushString(strOut);
		LUA->Call(1, 0);
		LUA->Pop(1);
		return 0;
	}
	LUA->PushString("This string is returned");
	return 1;
}*/

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
