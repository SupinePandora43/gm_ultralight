#!/bin/bash
echo rd include /S /Q
mkdir include

git clone https://github.com/ultralight-ux/Ultralight-API ./include/

mkdir include\GarrysMod
mkdir include\GarrysMod\Lua

cd include

echo rd .git /Q /S

curl https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/Interface.h -o GarrysMod\Lua\Interface.h https://raw.githubusercontent.com/dlfcn-win32/dlfcn-win32/master/dlfcn.h -o dlfcn.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/LuaBase.h -o GarrysMod/Lua/LuaBase.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/SourceCompat.h -o GarrysMod/Lua/SourceCompat.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/Types.h -o GarrysMod/Lua/Types.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/UserData.h -o GarrysMod/Lua/UserData.h
cd ..