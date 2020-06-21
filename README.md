# gm_ultralight ![C/C++ CI](https://github.com/SupinePandora43/gm_ultralight/workflows/C/C++%20CI/badge.svg)

[Ultralight](https://github.com/ultralight-ux/Ultralight) gmod integration

# HELP ME PLS

`make` don't want to work, until it get figured out, on linux/osx surface's functions called via `LUA`

create fork, edit `.github/workflows/ccpp.yml`, if linux/osx build work, make PR.

# TODO

* [security](https://github.com/okdshin/PicoSHA2)
* [CMAKE with compatibility mode](https://github.com/SupinePandora43/gm_ultralight/blob/e9535c7598d3db17c7a063d2a9a80091d34bd7fb/src/gmsv_ultralight.cpp)

# Contributors
[SupinePandora43](https://github.com/SupinePandora43) - offline
[GlebChili](https://github.com/GlebChili) - online

@GlebChili я надеюсь ты сможешь разобраться в коде который я написал
это так как оно задумывалось:
```lua
require("ultralight")
ul_Start() -- Start renderer thread
if not ulid then -- check is view already exists
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
	if not ul_IsLoaded(ulid) then -- подождать пока загрузится
		print("waiting until loads")
		return
	end
	-- нужна какая-то имплементация view->is_bitmap_dirty()
	render.PushRenderTarget(textureRT)
	cam.Start2D()
	render.Clear(0, 0, 0, 0)
	ul_RenderView(ulid)
	cam.End2D()
	render.PopRenderTarget()
end
-- перерисовывать картинку каждые 2 секунды
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
