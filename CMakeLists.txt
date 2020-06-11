cmake_minimum_required (VERSION 3.9)
project (gmsv_ultralight CXX)
set (CMAKE_CXX_STANDARD 11)
add_compile_definitions("GMMODULE" "COMPILER_MSVC64" "WIN32" "ARCHITECTURE_X86_64")

include_directories("./thirdparty/Ultralight-API")
set(ULTRALIGHT_LIBRARY_DIR "lib")                  # .lib libraries
if(NOT CMAKE_SYSTEM_NAME MATCHES "Windows")
    set(ULTRALIGHT_LIBRARY_DIR "bin")              # .so libraries
	set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -ldl") # dlsym.h
endif()
link_directories("${ULTRALIGHT_LIBRARY_DIR}")

link_libraries(UltralightCore Ultralight WebCore)
add_library(gmsv_ultralight SHARED "src/gmsv_ultralight.cpp")

#include_directories("./thirdparty/garrysmod_common/sourcesdk-minimal/public")
include_directories("./thirdparty/garrysmod_common/include")
link_directories("./thirdparty/garrysmod_common/sourcesdk-minimal/lib/public/x64")
link_libraries(tier0)

set(ultralight "gmsv_ultralight_")
if(WIN32)
	set_target_properties(gmsv_ultralight PROPERTIES OUTPUT_NAME "${ultralight}win64")
	add_compile_definitions("_AMD64_")
elseif(UNIX)
	set_target_properties(gmsv_ultralight PROPERTIES OUTPUT_NAME "${ultralight}linux64")
elseif(APPLE)
	set_target_properties(gmsv_ultralight PROPERTIES OUTPUT_NAME "${ultralight}osx64")
endif()
