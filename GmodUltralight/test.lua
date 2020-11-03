dotnet_load("GmodUltralight")
util.AddNetworkString("Ultralight_DrawDirty")
local view = Ultralight.CreateView(512,512,true)
print(view)
Ultralight.View_LoadURL(view, "https://github.com")

Ultralight.View_UpdateUntilLoads(view)

Ultralight.View_SV_DrawDirty(view)

-- CLIENT

local textureRT = GetRenderTarget( "UltralightView", 512, 512 )
local mat = CreateMaterial( "UltralightView_mat", "UnlitGeneric", {
  ['$basetexture'] = textureRT:GetName(),
  ["$translucent"] = "1"
});

net.Receive("Ultralight_DrawDirty", function()
	render.PushRenderTarget( textureRT )
	cam.Start2D()
	local pixels = util.JSONToTable(util.Decompress(net.ReadString())) -- DOESN't WORK
	for k,v in pairs(pixels) do
		surface.SetDrawColor(v.r,v.g,v.b,v.a)
		surface.DrawRect(v.x,v.y)
	end
	cam.End2D()
	render.PopRenderTarget()
end)

hook.Add( "HUDPaint", "Ultralight_draw_view", function()
  surface.SetDrawColor(color_white)
  surface.SetMaterial(mat)
  surface.DrawTexturedRect(50,50,512,512)
end)
