# gm_ultralight ![C/C++ CI](https://github.com/SupinePandora43/gm_ultralight/workflows/C/C++%20CI/badge.svg)

[Ultralight](https://github.com/ultralight-ux/Ultralight) gmod integration

# HELP ME PLS

`make` don't want to work, until it get figured out, on linux/osx surface's functions called via `LUA`

create fork, edit `.github/workflows/ccpp.yml`, if linux/osx build work, make PR.

# TODO

* [security](https://github.com/okdshin/PicoSHA2)

# Contributors
[SupinePandora43](https://github.com/SupinePandora43) - offline
[GlebChili](https://github.com/GlebChili) - online

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
-- renderer->Update() // считай что это OnThink страницы view. // если не будет вызываться, OnFinishLoading НИКОГДА не будет вызван! тк рендерер не загрузит страницу
-- https://docs.ultralig.ht/docs/integrating-with-games#render-loop-integration
-- ^^^ ОНИ ОБЩИЕ !!! НИКОГДА не вставляй эти методы в view.thonk(), тк это будет лишняя нагрузка, ведь рендерер ОДИН для ВСЕХ!!! - он обеспечивает загрузку / рендер ВСЕХ view помни это.
ul_SetURL(ulid, "https://thispersondoesnotexist.com")
hook.Remove("HUDPaint", "ExampleRTwithAlpha_Render")
local textureRT = GetRenderTarget( "ExampleRTwithAlpha", 512, 512 )
local mat = CreateMaterial("ExampleRTwithAlpha_Mat", "UnlitGeneric", {
	['$basetexture'] = textureRT:GetName(),
	["$translucent"] = "1"
})
local function renderu()
	if not ul_IsLoaded(ulid) then -- проверить если страница загружена
		print("waiting until loads")
		return
	end
	-- необходимо реализовать аналог view->is_bitmap_dirty(), точнее пофиксить работу SHMsync, переименовать в SHMdirty
	render.PushRenderTarget(textureRT)
	cam.Start2D()
	render.Clear(0, 0, 0, 0)
	-- отрендерить картинку из буфера
	ul_RenderView(ulid)
	cam.End2D()
	render.PopRenderTarget()
end
-- обновлять картинку раз 2 секунды
timer.Create("ul_updater", 2, 0, function()
	renderu()
end)
hook.Add( "HUDPaint", "ExampleRTwithAlpha_Render", function()
	surface.SetDrawColor( color_white )
	surface.SetMaterial( mat )
	surface.DrawTexturedRect( 50, 50, 512, 512 )
end)
```
первый созданный view, в моих локальных тестах никогда не был загружен
(ulid=0)

**`NULL`@`mijyuoon#6666`** - [`vgui::ISurface->DrawSetTextureRGBA`](https://discord.com/channels/565105920414318602/565108080300261398/723218859322114161)


**`Eclipse`@`Eclipse#2437`** - [`Sys_LoadInterface("vguimatsurface", "VGUI_Surface030", NULL, (void**)&surface)`](https://discord.com/channels/565105920414318602/567672652714475530/723205466838270024)

[Uros Spasojevic](https://app.slack.com/client/TC4C8F4CT/CC492VBLL/user_profile/ULE28P1AL) - reading pixel buffer
