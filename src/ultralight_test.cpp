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
Shm* ul_io_rpc;
Shm* ul_i_image;
Shm* ul_o_url;
#include <windows.h>
VOID startup(LPCTSTR lpApplicationName)
{
	// additional information
	STARTUPINFO si;
	PROCESS_INFORMATION pi;

	// set the size of the structures
	ZeroMemory(&si, sizeof(si));
	si.cb = sizeof(si);
	ZeroMemory(&pi, sizeof(pi));

	// start the program up
	CreateProcess(lpApplicationName,   // the path
		"",        // Command line
		NULL,           // Process handle not inheritable
		NULL,           // Thread handle not inheritable
		FALSE,          // Set handle inheritance to FALSE
		0,              // No creation flags
		NULL,           // Use parent's environment block
		NULL,           // Use parent's starting directory 
		&si,            // Pointer to STARTUPINFO structure
		&pi             // Pointer to PROCESS_INFORMATION structure (removed extra parentheses)
	);
	// Close process and thread handles. 
	CloseHandle(pi.hProcess);
	CloseHandle(pi.hThread);
}
void threadStarter() {
	std::cout << std::system("ultralight_renderer.exe") << std::endl;
}
int main() {
	std::cout << "c++: Starting IPC\n";

	if (ul_io_rpc != nullptr) delete ul_io_rpc;

	ul_io_rpc = new Shm{ "ul_io_rpc", 128 };
	ul_io_rpc->Create();

	const char* url = "https://youtube.com";

	if (ul_o_url != nullptr) delete ul_o_url;
	ul_o_url = new Shm{ "ul_o_url", 512 };
	ul_o_url->Create();

	std::memcpy(ul_o_url->Data(), url, std::string(url).length()); // put url

	std::cout << "c++: Starting renderer\n";
	//Shm shm{ "ultralight_o_url", 512 };
	//shm.Create();
	//memcpy(shm.Data(), url, std::strlen(url));
	std::thread launchyer = std::thread(threadStarter);

	//startup("ultralight_renderer");
	launchyer.join();
	std::cout << "c++: Started renderer\n";
	return 0;
}