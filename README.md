# gm_ultralight ![.NET Core](https://github.com/SupinePandora43/gm_ultralight/workflows/.NET%20Core/badge.svg)

[Ultralight](https://github.com/ultralight-ux/Ultralight) gmod integration

# WIP

* Crashes on 2nd load - FIX in ultralight 1.3 (IPC)
* use ISurface - help,help,help,help,help,help,help,help,help,help,help,help,help,help,help,help,help - GmodDotNet 2.0

# HOW TO USE
**THAT's ONLY MY CODE CONCEPT, what i want to achieve**
**IT's NOT IMPLEMENTED YET**

```lua
local textureRT = GetRenderTarget( "ExampleRTwithAlpha", 1024, 1024 )
local mat = CreateMaterial( "ExampleRTwithAlpha_Mat", "UnlitGeneric", {
  ['$basetexture'] = textureRT:GetName(),
  ["$translucent"] = "1"
});
local view = Ultralight.CreateView(1024,1024,true)
view.LoadURL("https://github.com")
local viewLoaded = false
view.SetLoadingCallback(function()
  viewLoaded = true
end)
while(!viewLoaded) do
  Ultralight.Update()
end
Ultralight.Render()
local rendered = false
function renderView()
  render.PushRenderTarget( textureRT )
  cam.Start2D()
  render.Clear(0,0,0,0)
  view.DrawWhole()
  cam.End2D()
  render.PopRenderTarget()
  rendered = true
end
hook.Add( "HUDPaint", "ExampleRTwithAlpha_Render", function()
  if(!rendered) then renderView() end
  surface.SetDrawColor(color_white)
  surface.SetMaterial(mat)
  surface.DrawTexturedRect(50,50,512,512)
end)
```
# Contributors
[SupinePandora43](https://github.com/SupinePandora43)

[GlebChili](https://github.com/GlebChili)

**`NULL`@`mijyuoon#6666`** - [`vgui::ISurface->DrawSetTextureRGBA`](https://discord.com/channels/565105920414318602/565108080300261398/723218859322114161)

**`Eclipse`@`Eclipse#2437`** - [`Sys_LoadInterface("vguimatsurface", "VGUI_Surface030", NULL, (void**)&surface)`](https://discord.com/channels/565105920414318602/567672652714475530/723205466838270024)

[Uros Spasojevic](https://app.slack.com/client/TC4C8F4CT/CC492VBLL/user_profile/ULE28P1AL) - uint8_t
