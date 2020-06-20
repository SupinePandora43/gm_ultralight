#include <Ultralight/Ultralight.h>
#include <chrono>   // std::chrono::milliseconds
#include <thread>   // std::this_thread::sleep_for
#include <string>   // std::string
#include <iostream> // std::cout
#include <cstring>  // memcpy
#include <vector>   // std::vector
#include "shoom/shm.h"

using namespace ultralight;

#define URLLEN 512

class IView :public LoadListener {
	Shm* SHMwidth;
	Shm* SHMheight;
	Shm* SHMurl;
	Shm* SHMisloaded;
public:
	Shm* image;
	uint32_t width = 0;
	uint32_t height = 0;
	char* url;
	RefPtr<View> view;
	bool rendered = false;

	IView(uint8_t id, RefPtr<Renderer> renderer) {
		SHMwidth = new Shm{ std::string("ul_o_width_").append(std::to_string(id)), 64 };
		SHMwidth->Open();
		width = std::stoi((char*)SHMwidth->Data());

		SHMheight = new Shm{ std::string("ul_o_height_").append(std::to_string(id)), 64 };
		SHMheight->Open();
		height = std::stoi((char*)SHMheight->Data());

		SHMurl = new Shm{ std::string("ul_o_url_").append(std::to_string(id)), URLLEN };
		SHMurl->Open();
		url = (char*)SHMurl->Data();

		image = new Shm{ std::string("ul_i_image_").append(std::to_string(id)), 1 + (width * height * 4) };
		image->Create();

		SHMisloaded = new Shm{ std::string("ul_i_isloaded_").append(std::to_string(id)), 16 };
		SHMisloaded->Create();
		view = renderer->CreateView(width * 4, height * 4, false); // https://github.com/ultralight-ux/Ultralight/issues/257#issuecomment-636330995
		view->set_load_listener(this);
		view->Resize(width, height);
	}
	uint8_t* Get() {
		return image->Data();
	}
	~IView() {
		view = nullptr;
		delete SHMwidth;
		delete SHMheight;
		delete SHMurl;
		delete SHMisloaded;
		delete image;
	}
	void OnFinishLoading(View* caller) {
		SHMisloaded->Data()[0] = 1;
	}
};

void SendImage(IView iview) {
	memcpy(iview.Get(), iview.view->bitmap()->LockPixels(), iview.width * iview.height * 4);
	iview.view->bitmap()->UnlockPixels();
}
//#include <cstdlib>
int main() {
	std::cout << "starting renderer" << std::endl;
	std::vector<IView> views;
	Shm ul_o_rpc{ "ul_o_rpc", 128 };
	Shm ul_i_rpc{ "ul_i_rpc", 128 };
	Shm ul_o_createview{ "ul_o_createview", 255 };
	ul_i_rpc.Create();
	Config config;
	config.device_scale_hint = 1.0;
	config.font_family_standard = "Arial";
	Platform::instance().set_config(config);
	RefPtr<Renderer> renderer = Renderer::Create();
	while (true) {
		std::this_thread::sleep_for(std::chrono::milliseconds(1000));
		std::cout << "opening rpc" << std::endl;
		ul_o_rpc.Open();
		if (ul_o_rpc.Data()[0] != 0) break;
		ul_o_createview.Open();
		for (uint8_t i = 0; i < 255; i++)
		{
			uint8_t id = ul_o_createview.Data()[i];
			if (id != 0) {
				views.push_back(IView(id, renderer));
			}
		}

	}
	std::cout << "closing..." << std::endl;
	renderer = nullptr;
	return 0;
}