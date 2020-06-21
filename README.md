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

```lua
require("ultralight")
ul_Start() -- запустить ultralight_renderer.exe
if not ulid then -- проверить если уже существует
	ulid=ul_CreateView(512, 512) -- Create View 512x512
end
ul_SetURL(ulid, "https://thispersondoesnotexist.com") -- load url
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
	-- необходимо реализовать аналог (пофиксить работу SHMsync, переименовать в SHMdirty) view->is_bitmap_dirty()
	render.PushRenderTarget(textureRT)
	cam.Start2D()
	render.Clear(0, 0, 0, 0)
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

**`NULL`@`mijyuoon#6666`** - [`vgui::ISurface->DrawSetTextureRGBA`](https://discord.com/channels/565105920414318602/565108080300261398/723218859322114161)


**`Eclipse`@`Eclipse#2437`** - [`Sys_LoadInterface("vguimatsurface", "VGUI_Surface030", NULL, (void**)&surface)`](https://discord.com/channels/565105920414318602/567672652714475530/723205466838270024)

[Uros Spasojevic](https://app.slack.com/client/TC4C8F4CT/CC492VBLL/user_profile/ULE28P1AL) - reading pixel buffer
