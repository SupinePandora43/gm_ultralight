mkdir bin

curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/libAppCore.so -o bin/libAppCore.so
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/libUltralight.so -o bin/libUltralight.so
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/libUltralightCore.so -o bin/libUltralightCore.so
curl -L https://github.com/SupinePandora43/gm_ultralight/releases/download/bin/libWebCore.so -o bin/libWebCore.so

file bin/libUltralight.so
