mkdir include
cd include
git clone https://github.com/ultralight-ux/Ultralight-API .
cd ..
mkdir include\GarrysMod
mkdir include\GarrysMod\Lua

curl https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/Interface.h -o include/GarrysMod/Lua/Interface.h
curl https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/LuaBase.h -o include/GarrysMod/Lua/LuaBase.h
curl https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/SourceCompat.h -o include/GarrysMod/Lua/SourceCompat.h
curl https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/Types.h -o include/GarrysMod/Lua/Types.h
curl https://raw.githubusercontent.com/Facepunch/gmod-module-base/development/include/GarrysMod/Lua/UserData.h -o include/GarrysMod/Lua/UserData.h