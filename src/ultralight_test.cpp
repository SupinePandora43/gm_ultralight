#include <string>
#include <iostream>
#include <thread>
#include <cstring> // memcpy
#include <vector>

#include "shoom/shm.h"

#include "log/fslog.h"

#define URLLEN 512

#ifdef _DEBUG
const char* ulpath = "..\\..\\..\\..\\..\\out\\ultralight_renderer.exe";
#else
const char* ulpath = "ultralight_renderer.exe";
#endif
void threadStarter() {
	LOG(std::to_string(std::system(ulpath)).c_str());
}
#define URLLEN 512

class IView {
	Shm* SHMwidth;
	Shm* SHMheight;
	Shm* SHMurl;
	Shm* SHMupdate;
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
		SHMupdate->Data()[0] = 0;
		image->Open();
		return image->Data();
	}
	~IView() {
		delete SHMwidth;
		delete SHMheight;
		delete SHMurl;
		delete SHMupdate;
		delete SHMisloaded;
		delete SHMsync;
		delete image;
	}
};
std::vector<IView*> views;
int main() {
	LOG("c++: Starting IPC");

	Shm ul_o_rpc{ "ul_o_rpc", 128 };
	ul_o_rpc.Create();
	Shm ul_o_createview{ "ul_o_createview", 255 };
	ul_o_createview.Create();

	std::cout << "creating view" << std::endl;
	IView* view = new IView(0, 2048, 2048);
	//view->SetURL("https://github.com");
	ul_o_createview.Data()[0] = 1;
	views.push_back(view);

	//std::memcpy(ul_o_url->Data(), url, std::string(url).length()); // put url

	LOG("c++: Starting renderer");
	//std::thread launchyer = std::thread(threadStarter);

	LOG("c++: Started renderer");

	uint16_t shutdown = 0;
	while (true) {
		std::cout << "IsLoaded: " << views.at(0)->IsLoaded() <<std::endl;
		std::cout << "HasNewFrame: " << views.at(0)->HasNewFrame() << std::endl;
		std::string url;
		std::cin >> url;
		if (std::string(url) != std::string("")) {
			views.at(0)->SetURL(url);
		}
	}
	LOG("shutting down, joining thread");
	//launchyer.detach();
	LOG("renderer end");
	//std::system("taskkill /F /T /IM ultralight_renderer.exe");
	return 0;
}