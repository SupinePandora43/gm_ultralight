#include <Ultralight/Ultralight.h>
#include <chrono> // sleep
#include <thread> // thread
#include <string>
#include <iostream>
#include <cstring> // memcpy

#include "shoom/shm.h"

using namespace ultralight;

class App : public LoadListener {
	bool done = false;
public:
	RefPtr<Renderer> renderer;
	RefPtr<View> view;
	App() {
		Config config;
		config.device_scale_hint = 1.0;
		config.font_family_standard = "Arial";
		Platform::instance().set_config(config);
		renderer = Renderer::Create();
		view = renderer->CreateView(2048, 2048, false); // https://github.com/ultralight-ux/Ultralight/issues/257#issuecomment-636330995
		view->set_load_listener(this);
		view->Resize(512, 512);
	}
	void SetURL(String url) {
		view->LoadURL(url);
	}
	~App() {
		view = nullptr;
		renderer = nullptr;
	}
	void Run() {
		uint32_t timeout = 0;
		done = false;
		while (timeout < 1000000000 && !done) {
			timeout++;
			renderer->Update();
		}
		std::cout << timeout << std::endl;
	}
	void OnFinishLoading(View* caller) {
		done = true;
		renderer->Render();
		view->bitmap()->WritePNG("jooj.png");
	}
};

#include <cstdlib>
char* url = "https://github.com";
int main(int argc, char* argv[]) {
	App app;
	Shm ul_io_rpc{ "ul_io_rpc", 64 };
	Shm ul_i_image{ "ul_i_image", (512 * 512 * 4) + 1 };
	Shm ul_o_url{ "ul_o_url", 512 };
	ul_i_image.Create();
	while (true) {
		std::this_thread::sleep_for(std::chrono::milliseconds(1000)); // sleep :D
		std::cout << "opening rpc/url" << std::endl;
		ul_io_rpc.Open();
		ul_o_url.Open();
		if (ul_o_url.Data() != nullptr) {
			std::cout << (char*)ul_o_url.Data() << std::endl;
			if (ul_io_rpc.Data() == nullptr || ul_io_rpc.Data()[0] != 0){ // stop renderer
				std::cout << "rpc[0]=1 -> shutting down" << std::endl;
		}
		if (ul_o_url.Data() != nullptr) {
			std::cout << (char*)ul_o_url.Data() << std::endl;
			if (url != (char*)ul_o_url.Data()) {
				url = (char*)ul_o_url.Data();
				app.SetURL(url);
				app.Run();
				std::cout << "app->Run - succeful" << std::endl;
				try {
					std::cout << app.view->bitmap()->size() << std::endl;
					memcpy(ul_i_image.Data(), (uint8_t*)app.view->bitmap()->LockPixels(), 512 * 512 * 4); // https://stackoverflow.com/questions/2963898/faster-alternative-to-memcpy
					app.view->bitmap()->UnlockPixels();
				}
				catch (std::exception e) {
					std::cout << e.what() << std::endl;
				}
			}
		}
		else {
			std::cout << "ul_o_url->Data() is nullptr" << std::endl;
		}
	}
	std::cout << "closing..." << std::endl;
	return 0;
}