# gm_ultralight ![C/C++ CI](https://github.com/SupinePandora43/gm_ultralight/workflows/C/C++%20CI/badge.svg)

[Ultralight](https://github.com/ultralight-ux/Ultralight) gmod integration

# HELP ME PLS

`make` don't want to work, until it get figured out, on linux/osx surface's functions called via `LUA`

create fork, edit `.github/workflows/ccpp.yml`, if linux/osx build work, make PR.

у меня меньше недели чтобы закончить этот проект

# TODO

* [security](https://github.com/okdshin/PicoSHA2)

# HOW TO USE

@GlebChili я надеюсь что ты поймешь написанный мною код

это то как оно задумывалось в действии
```
GarrysMod
|- hl2.exe
|- ultralight_renderer.exe
|- AppCore.dll
|- UltralightCore.dll
|- Ultralight.dll
|- WebCore.dll
|-garrysmod
  |-lua
    |-bin
      |-gmcl_ultralight_win64.dll
```
// наверное придется переместить `ultralight_renderer` в lua/bin, чтобы Lua (file.Read) мог проверить хеш (SHA256)

# API

`id` = View id, you can get it from `ul_CreateView`

* `ul_Start()` - start's renderer thread
* `ul_CreateView(int width, int height) = int` - creates view
* `ul_SetURL(int id, string url)` - set url
* `ul_DrawAtOnce(int id)` - draw whole picture to renderer context (`render.PushRenderTarget() + cam.Start2D()`)
* `ul_DrawLine(int id)` - draw line of picture to renderer context, pretty good at performance ! but need to be called many times, i hope you will call it each frame
* `ul_IsLoaded(int id) = bool` - is page loaded or not?
* `ul_size() = int` - return count of views... idk why you should use this


# USAGE
- *static* - renders picture only at once
```lua
require("ultralight")
ul_Start() -- запустить ultralight_renderer.exe
if not ulid then -- проверить если уже существует
	ulid=ul_CreateView(512, 512) -- Create View 512x512
end
-- Задать URL
-- view->LoadURL()
-- оно автоматически начнёт загрузку страницы
-- при окончании загрузки (OnFinishLoading) картинка будет (view->bitmap()->LockPixels()) белой, тк не была отрендерена (renderer->Render())
-- Я ВАМ ЗАПРЕЩАЮ renderer-Render() в OnFinishLoading, тк будут рендериться ВСЕ view

-- renderer->Render() // отрисовывает ВСЕ view, лучше использовать только если у какогото view, view->is_bitmap_dirty() = true. отвечает за рендер страницы (иначе всё будет белым)
-- renderer->Update() // считай что это OnThink HTML страницы. // если не будет вызываться, OnFinishLoading НИКОГДА не будет вызван! тк рендерер не загрузит страницу // этот метод отвечает за загрузку HTML, CSS, JS, и за их обновление
-- https://docs.ultralig.ht/docs/integrating-with-games#render-loop-integration
-- ^^^ ОНИ ОБЩИЕ !!! НИКОГДА не вставляй эти методы в view.thonk(), тк это будет лишняя нагрузка, ведь рендерер ОДИН для ВСЕХ!!! - он обеспечивает загрузку / рендер ВСЕХ view помни это.
ul_SetURL(ulid, "https://thispersondoesnotexist.com")
hook.Remove("HUDPaint", "ExampleRTwithAlpha_Render")
local textureRT = GetRenderTarget( "ExampleRTwithAlpha", 512, 512 )
local mat = CreateMaterial("ExampleRTwithAlpha_Mat", "UnlitGeneric", {
	['$basetexture'] = textureRT:GetName(),
	["$translucent"] = "1"
})
local rendered = false
local function renderu()
	if not ul_IsLoaded(ulid) then -- проверить если страница загружена
		print("waiting until loads")
		return
	end
	render.PushRenderTarget(textureRT)
	cam.Start2D()
	render.Clear(0, 0, 0, 0)
	-- отрендерить картинку
	-- draw whole picture at once
	ul_DrawAtOnce(ulid)
	cam.End2D()
	render.PopRenderTarget()
	rendered = true
end
hook.Add( "HUDPaint", "ExampleRTwithAlpha_Render", function()
	if rendered then
		surface.SetDrawColor( color_white )
		surface.SetMaterial( mat )
		surface.DrawTexturedRect( 50, 50, 512, 512 )
	else
		renderu()
	end
end)
```

- *dynamic* - renders line by line each frame

```lua
require("ultralight")
-- pls don't call ul_Start() more than one time
ul_Start() -- Start renderer thread
if not ulid then -- check is view already exists
	ulid=ul_CreateView(512,512) -- Create View 512x512
end
ul_SetURL(ulid, "https://thispersondoesnotexist.com") -- load url

hook.Remove("HUDPaint", "ExampleRTwithAlpha_Render")
local textureRT = GetRenderTarget( "ExampleRTwithAlpha", 512, 512 )
local mat = CreateMaterial("ExampleRTwithAlpha_Mat", "UnlitGeneric", {
	['$basetexture'] = textureRT:GetName(),
	["$translucent"] = "1"
})
local function renderu()
	if not ul_IsLoaded(ulid) then -- is page loaded ?
		print("waiting until loads")
		return
	end
	render.PushRenderTarget(textureRT)
	cam.Start2D()
	ul_DrawLine(ulid)
	cam.End2D()
	render.PopRenderTarget()
end
hook.Add( "HUDPaint", "ExampleRTwithAlpha_Render", function()
	renderu()
	surface.SetDrawColor( color_white )
	surface.SetMaterial( mat )
	surface.DrawTexturedRect( 50, 50, 512, 512 )
end)
```

# Contributors
[SupinePandora43](https://github.com/SupinePandora43)

[GlebChili](https://github.com/GlebChili)

**`NULL`@`mijyuoon#6666`** - [`vgui::ISurface->DrawSetTextureRGBA`](https://discord.com/channels/565105920414318602/565108080300261398/723218859322114161)

**`Eclipse`@`Eclipse#2437`** - [`Sys_LoadInterface("vguimatsurface", "VGUI_Surface030", NULL, (void**)&surface)`](https://discord.com/channels/565105920414318602/567672652714475530/723205466838270024)

[Uros Spasojevic](https://app.slack.com/client/TC4C8F4CT/CC492VBLL/user_profile/ULE28P1AL) - reading pixel buffer
