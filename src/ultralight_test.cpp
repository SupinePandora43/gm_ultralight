#include <string>
#include <iostream>
#include <thread>
#include <cstring> // memcpy


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
class IView {
	Shm* SHMwidth;
	Shm* SHMheight;
	Shm* SHMurl;
	Shm* SHMupdate;
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
		SHMupdate = new Shm{ std::string("ul_o_update_").append(std::to_string(id)), 16 };
		SHMupdate->Create();
	}
	void SetURL(char* url) {
		memcpy(SHMurl->Data(), url, std::string(url).length());
	}
	void Update() {
		SHMupdate->Data()[0] = 1;
	}
	bool IsLoaded() {
		SHMisloaded->Open();
		return SHMisloaded->Data()[0];
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
		delete SHMupdate;
		delete SHMisloaded;
	}
};
int main() {
	LOG("c++: Starting IPC");

	Shm ul_o_rpc{ "ul_o_rpc", 128 };
	ul_o_rpc.Create();
	Shm ul_o_createview{ "ul_o_createview", 255 };
	ul_o_createview.Create();
	IView view(1, 2048, 2048);
	view.SetURL("https://github.com");
	ul_o_createview.Data()[1] = 1;
	const char* url = "https://youtube.com";

	//std::memcpy(ul_o_url->Data(), url, std::string(url).length()); // put url

	LOG("c++: Starting renderer");
	//std::thread launchyer = std::thread(threadStarter);

	LOG("c++: Started renderer");

	uint16_t shutdown = 0;
	while (true) {
		std::cin >> shutdown;
		std::cout << shutdown;
		view.Update();
		ul_o_rpc.Data()[0] = shutdown;
		//if (shutdown != 0) break;
	}
	LOG("shutting down, joining thread");
	//launchyer.detach();
	LOG("renderer end");
	//std::system("taskkill /F /T /IM ultralight_renderer.exe");
	return 0;
}