#!/bin/bash
echo rd include /S /Q
mkdir include

git clone https://github.com/ultralight-ux/Ultralight-API ./include/

mkdir include/GarrysMod
mkdir include/GarrysMod/Lua

cd include

echo rd .git /Q /S

curl https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/Interface.h -o GarrysMod/Lua/Interface.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/LuaBase.h -o GarrysMod/Lua/LuaBase.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/SourceCompat.h -o GarrysMod/Lua/SourceCompat.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/Types.h -o GarrysMod/Lua/Types.h https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/UserData.h -o GarrysMod/Lua/UserData.h
cd ..

mkdir bin
curl https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/libAppCore.so -o lib/libAppCore.so
curl https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/libUltralight.so -o lib/libUltralight.so
curl https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/libUltralightCore.so -o lib/libUltralightCore.so
curl https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/libWebCore.so -o lib/libWebCore.so
