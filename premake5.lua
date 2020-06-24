include("thirdparty/garrysmod_common/generator.v2.lua")
CreateWorkspace({name="ultralight"})
targetdir("out")
local defineT = {}
filter({"system:windows", "platforms:x86"})
	defines({"WIN32", "_WIN32"})
filter({"system:windows", "platforms:x86_64"})
	defines({"WIN64", "_WIN64"})
filter({"system:linux"})
	defines( {"__linux__"})
filter({"system:macosx"})
	defines( {"__APPLE__"})

project("ultralight_renderer")
architecture("x86_64")
kind("ConsoleApp")
language("C++")
filter({"system:windows", "platforms:x86_64"})
    defines({"_AMD64_", "_WIN64", "WIN64"})
files({
"src/**/*.h",
"src/**/*.hpp",
"src/**/*.hxx",
"src/ultralight_renderer.cpp"})
--links { "pthread", "rt" }
vpaths({
    ["Header files/*"] = {"src/**/*.h", "src/**/*.hpp", "src/**/*.hxx"},
    ["Source files/*"] = {"src/ultralight_renderer.cpp"}
})
includedirs({"thirdparty/Ultralight-API"})
libdirs ({ "libs", "lib" })
links({"UltralightCore",  "Ultralight", "WebCore"})

project("ultralight_test")
kind("ConsoleApp")
language("C++")
defines({"_AMD64_", "_WIN64", "WIN64"})
files({
"src/**/*.h",
"src/**/*.hpp",
"src/**/*.hxx",
"src/ultralight_test.cpp"})
--links { "pthread", "rt" }
vpaths({
    ["Header files/*"] = {"src/**/*.h", "src/**/*.hpp", "src/**/*.hxx"},
    ["Source files/*"] = {"src/ultralight_test.cpp"}
})

CreateProject({serverside=true})
configurations({"Release", "Debug"})
--defines({"GMMODULE"})
--[[defines({
	"GMMODULE",
	string.upper(string.gsub(_workspace.name, "%.", "_")) .. "_SERVER","_CLIENT",
	"IS_SERVERSIDE=" .. tostring(SHARED)
})]]
filter({"system:windows"})
    defines({"COMPILER_MSVC", "WIN32", "_WIN32"})
filter({"system:windows", "platforms:x86_64"})
    defines({"_AMD64_", "COMPILER_MSVC64", "_WIN64", "WIN64", "ARCHITECTURE_X86_64", "__x86_64__", "PERFORMANCE"})
defines(defineT)
defaultplatform("x86_64")
IncludeLuaShared() -- uses this repo path
files({
"src/**/*.h",
"src/**/*.hpp",
"src/**/*.hxx",
"src/gmsv_ultralight.cpp"})

--links { "pthread", "rt" }
vpaths({
    ["Header files/*"] = {"src/**/*.h", "src/**/*.hpp", "src/**/*.hxx"},
    ["Source files/*"] = {"src/gmsv_ultralight.cpp"}
})
-- IncludeDetouring() -- uses this repo detouring submodule
-- IncludeScanning() -- uses this repo scanning submodule

IncludeSDKCommon()
IncludeSDKTier0()
IncludeSDKTier1()
IncludeSDKTier2()
 IncludeSDKTier3()
IncludeSDKMathlib()
IncludeSDKRaytrace()
-- IncludeSteamAPI()
