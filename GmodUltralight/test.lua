dotnet_load("GmodUltralight")
util.AddNetworkString("Ultralight_DrawSingle")
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

net.Receive("Ultralight_DrawAll", function(len)
	render.PushRenderTarget( textureRT )
	cam.Start2D()
	local readedlen = 0
	while readlen < len do
		surface.SetDrawColor(net.ReadUInt(8),net.ReadUInt(8),net.ReadUInt(8),net.ReadUInt(8))
		surface.DrawRect(net.ReadUInt(32),net.ReadUInt(32),1,1)
		readedlen = readedlen+96
	end
	cam.End2D()
	render.PopRenderTarget()
end)
net.Receive("Ultralight_DrawSingle", function()
	render.PushRenderTarget( textureRT )
	cam.Start2D()
	surface.SetDrawColor(net.ReadUInt(8),net.ReadUInt(8),net.ReadUInt(8),net.ReadUInt(8))
	surface.DrawRect(net.ReadUInt(32),net.ReadUInt(32),1,1)
	cam.End2D()
	render.PopRenderTarget()
end)
hook.Add( "HUDPaint", "Ultralight_draw_view", function()
  surface.SetDrawColor(color_white)
  surface.SetMaterial(mat)
  surface.DrawTexturedRect(50,50,512,512)
end)
