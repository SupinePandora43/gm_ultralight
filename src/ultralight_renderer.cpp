#include <Ultralight/Ultralight.h>
#include <chrono> // sleep
#include <thread> // thread
#include <string>
#include <iostream>
#include <cstring> // memcpy
#if defined(__linux__) || defined(__APPLE__)
#include <dlfcn.h> // dlsym
#include <fcntl.h>     // for O_* constants
#include <sys/mman.h>  // mmap, munmap
#include <sys/stat.h>  // for mode constants
#include <unistd.h>    // unlink
#if defined(__APPLE__)
#include <errno.h>
#endif
#include <stdexcept>
#endif
#ifdef _WIN64
#include <libloaderapi.h> // GetProcAddres
#include <windows.h> // CreateFileMapping
#include <io.h>
#endif
using namespace ultralight;

enum ShoomError {
	kOK = 0,
	kErrorCreationFailed = 100,
	kErrorMappingFailed = 110,
	kErrorOpeningFailed = 120,
};
class Shm {
	std::string path_;
	uint8_t* data_ = nullptr;
	size_t size_ = 0;
#if defined(_WIN32)
	HANDLE handle_;
#else
	int fd_ = -1;
#endif
public:
	//Shm(std::string path, size_t size) : path_(path), size_(size) {};
	Shm(std::string path, size_t size) {
#ifdef WIN32
		path_ = path;
#else
		path_ = "/" + path;
#endif
		size_ = size;
	};
	ShoomError Create() { return CreateOrOpen(true); }
	ShoomError Open() { return CreateOrOpen(false); }
	uint8_t* Data() { return data_; }
	ShoomError CreateOrOpen(bool create) {
#ifdef WIN32
		if (create) {
			DWORD size_high_order = 0;
			DWORD size_low_order = static_cast<DWORD>(size_);
			handle_ = CreateFileMappingA(INVALID_HANDLE_VALUE,  // use paging file
				NULL,                  // default security
				PAGE_READWRITE,        // read/write access
				size_high_order, size_low_order,
				path_.c_str()  // name of mapping object
			);
			if (!handle_) {
				return kErrorCreationFailed;
			}
		}
		else {
			handle_ = OpenFileMappingA(FILE_MAP_READ,  // read access
				FALSE,          // do not inherit the name
				path_.c_str()   // name of mapping object
			);

			if (!handle_) {
				return kErrorOpeningFailed;
			}
		}
		DWORD access = create ? FILE_MAP_ALL_ACCESS : FILE_MAP_READ;
		data_ = static_cast<uint8_t*>(MapViewOfFile(handle_, access, 0, 0, size_));
		if (!data_) {
			return kErrorMappingFailed;
		}
		return kOK;
#else
		if (create) {
			int ret = shm_unlink(path_.c_str());
			if (ret < 0) {
				if (errno != ENOENT) {
					return kErrorCreationFailed;
				}
			}
		}
		int flags = create ? (O_CREAT | O_RDWR) : O_RDONLY;
		fd_ = shm_open(path_.c_str(), flags, 0755);
		if (fd_ < 0) {
			if (create) {
				return kErrorCreationFailed;
			}
			else {
				return kErrorOpeningFailed;
			}
		}
		if (create) {
			int ret = ftruncate(fd_, size_);
			if (ret != 0) {
				return kErrorCreationFailed;
			}
		}
		int prot = create ? (PROT_READ | PROT_WRITE) : PROT_READ;
		auto memory = mmap(nullptr,     // addr
			size_,       // length
			prot,        // prot
			MAP_SHARED,  // flags
			fd_,         // fd
			0            // offset
		);
		if (memory == MAP_FAILED) {
			return kErrorMappingFailed;
		}
		data_ = static_cast<uint8_t*>(memory);
		if (!data_) {
			return kErrorMappingFailed;
		}
		return kOK;
#endif
	}
	~Shm() {
#ifndef WIN32
		munmap(data_, size_);
		close(fd_);
		shm_unlink(path_.c_str());
#else
		if (data_) {
			UnmapViewOfFile(data_);
			data_ = nullptr;
		}
		CloseHandle(handle_);
#endif
	}
};


class App : public LoadListener {
	bool done = false;
public:
	RefPtr<Renderer> renderer;
	RefPtr<View> view;
	App() {
		Config config;
		config.device_scale_hint = 2.0;
		config.font_family_standard = "Arial";
		Platform::instance().set_config(config);
		renderer = Renderer::Create();
		view = renderer->CreateView(4096, 4096, false); // https://github.com/ultralight-ux/Ultralight/issues/257#issuecomment-636330995
		view->set_load_listener(this);
		view->Resize(1024, 1024);
	}
	void SetURL(String url) {
		view->LoadURL(url);
	}
	~App() {
		view = nullptr;
		renderer = nullptr;
	}
	void Run() {
		done = false;
		while (!done) {
			renderer->Update();
		}
	}
	void OnFinishLoading(View* caller) {
		done = true;
		renderer->Render();
		view->bitmap()->WritePNG("jooj.png");
	}
};
char* url = "https://github.com";
int main(int argc, char* argv[]) {
	App app;
	Shm ul_io_rpc{ "ul_io_rpc", 64 };
	Shm ul_i_image{ "ul_i_image", 1024 * 1024 * 4 };
	Shm ul_o_url{ "ul_o_url", 512 };
	ul_i_image.Create();
	while (true) {
		std::this_thread::sleep_for(std::chrono::milliseconds(1000)); // sleep :D
		ul_io_rpc.Open();
		ul_o_url.Open();
		if (ul_io_rpc.Data()[0] != 0) break; // stop renderer
		if (ul_o_url.Data() != nullptr) {
			std::cout << (char*)ul_o_url.Data() << std::endl;
			if ((uint8_t*)url != ul_o_url.Data()) {
				url = (char*)ul_o_url.Data();
				app.SetURL(url);
				app.Run();
				std::cout << "app->Run - succeful" << std::endl;
				try {
					std::cout << app.view->bitmap()->size() << std::endl;
					memcpy(ul_i_image.Data(), app.view->bitmap()->LockPixels(), app.view->bitmap()->size() / 4); // https://stackoverflow.com/questions/2963898/faster-alternative-to-memcpy
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