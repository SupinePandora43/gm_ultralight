rd include /S /Q
mkdir include

git clone https://github.com/ultralight-ux/Ultralight-API ./include/

mkdir include\GarrysMod
mkdir include\GarrysMod\Lua

cd include

rd .git /Q /S

curl https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/Interface.h -o GarrysMod/Lua/Interface.h https://raw.githubusercontent.com/dlfcn-win32/dlfcn-win32/master/dlfcn.h -o dlfcn.h https://raw.githubusercontent.com/dlfcn-win32/dlfcn-win32/master/dlfcn.c -o dlfcn.c https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/LuaBase.h -o GarrysMod/Lua/LuaBase.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/SourceCompat.h -o GarrysMod/Lua/SourceCompat.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/Types.h -o GarrysMod/Lua/Types.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/UserData.h -o GarrysMod/Lua/UserData.h
cd ..

mkdir bin
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/AppCore.dll -o bin/AppCore.dll
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/Ultralight.dll -o bin/Ultralight.dll
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/UltralightCore.dll -o bin/UltralightCore.dll
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/WebCore.dll -o bin/WebCore.dll

mkdir lib
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/AppCore.lib -o lib/AppCore.lib
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/Ultralight.lib -o lib/Ultralight.lib
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/UltralightCore.lib -o lib/UltralightCore.lib
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/WebCore.lib -o lib/WebCore.lib
