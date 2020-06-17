#include <string>
#include <iostream>
#include <thread>
#if defined(__linux__) || defined(__APPLE__)
#include <dlfcn.h> // dlsym
#include <cstring> // memcpy
#include <fcntl.h>     // for O_* constants
#include <sys/mman.h>  // mmap, munmap
#include <sys/stat.h>  // for mode constants
#include <unistd.h>    // unlink
#if defined(__APPLE__)
#include <errno.h>
#endif
#include <stdexcept>
#elif _WIN64
#include <libloaderapi.h> // GetProcAddres
#include <windows.h> // CreateFileMapping
#include <io.h>
#endif

#include "shoom/shm.h"

Shm* ul_io_rpc;
Shm* ul_i_image;
Shm* ul_o_url;

#include "log/fslog.h"

void threadStarter() {
	LOG(std::to_string(std::system("ultralight_renderer.exe")).c_str());
}
int main() {
	LOG("c++: Starting IPC");

	if (ul_io_rpc != nullptr) delete ul_io_rpc;

	ul_io_rpc = new Shm{ "ul_io_rpc", 128 };
	ul_io_rpc->Create();

	const char* url = "https://youtube.com";

	if (ul_o_url != nullptr) delete ul_o_url;
	ul_o_url = new Shm{ "ul_o_url", 512 };
	ul_o_url->Create();

	std::memcpy(ul_o_url->Data(), url, std::string(url).length()); // put url

	LOG("c++: Starting renderer");
	//Shm shm{ "ultralight_o_url", 512 };
	//shm.Create();
	//memcpy(shm.Data(), url, std::strlen(url));
	std::thread launchyer = std::thread(threadStarter);

	//startup("ultralight_renderer");
	LOG("c++: Started renderer");

	uint8_t shutdown = 0;
	while (true) {
		std::cin >> shutdown;
		ul_io_rpc->Data()[0] = shutdown;
		if (shutdown != 0) break;
	}
	LOG("shutting down, joining thread");
	launchyer.detach();
	LOG("renderer end");
	std::system("taskkill /F /T /IM ultralight_renderer.exe");
	return 0;
}