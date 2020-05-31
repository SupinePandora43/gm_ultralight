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
wget "https://github-production-release-asset-2e65be.s3.amazonaws.com/266918412/446c6900-a32e-11ea-9cb4-bb3a6b175f69?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIWNJYAX4CSVEH53A%2F20200531%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20200531T063316Z&X-Amz-Expires=300&X-Amz-Signature=e7585ac325b4889007069584a75899dedea628ec34a62c63a0f3e697518b89b8&X-Amz-SignedHeaders=host&actor_id=36124472&repo_id=266918412&response-content-disposition=attachment%3B%20filename%3DlibAppCore.so&response-content-type=application%2Foctet-stream" -o bin/libAppCore.so
wget "https://github-production-release-asset-2e65be.s3.amazonaws.com/266918412/3dddf180-a32e-11ea-9219-2cf6c4eba70e?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIWNJYAX4CSVEH53A%2F20200531%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20200531T063339Z&X-Amz-Expires=300&X-Amz-Signature=f044296f38beb378a8556333bcce3e47a7cf7916e07452450e957411434e25d5&X-Amz-SignedHeaders=host&actor_id=36124472&repo_id=266918412&response-content-disposition=attachment%3B%20filename%3DlibUltralight.so&response-content-type=application%2Foctet-stream" -o bin/libUltralight.so
wget "https://github-production-release-asset-2e65be.s3.amazonaws.com/266918412/3fa7b500-a32e-11ea-93ab-46e241a96610?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIWNJYAX4CSVEH53A%2F20200531%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20200531T063405Z&X-Amz-Expires=300&X-Amz-Signature=07717d989d0183160341106c5b10a442e0991dab87e72737d89b2c952d01c807&X-Amz-SignedHeaders=host&actor_id=36124472&repo_id=266918412&response-content-disposition=attachment%3B%20filename%3DlibUltralightCore.so&response-content-type=application%2Foctet-stream" -o bin/libUltralightCore.so
wget "https://github-production-release-asset-2e65be.s3.amazonaws.com/266918412/40404b80-a32e-11ea-8d06-a96a972ce966?X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIWNJYAX4CSVEH53A%2F20200531%2Fus-east-1%2Fs3%2Faws4_request&X-Amz-Date=20200531T063432Z&X-Amz-Expires=300&X-Amz-Signature=0a3892b3a530f02591c61e128dee2a8b0247eb3db8b1f4576bf111eba4fc4674&X-Amz-SignedHeaders=host&actor_id=36124472&repo_id=266918412&response-content-disposition=attachment%3B%20filename%3DlibWebCore.so&response-content-type=application%2Foctet-stream" -o bin/libWebCore.so

file bin/libUltralight.so
