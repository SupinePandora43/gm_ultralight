git submodule update --init --recursive

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
