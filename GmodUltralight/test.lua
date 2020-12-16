print("Test loader loaded")
require("dotnet")

local testdone = false
local testfailed = false

local function test()
	print("Running test")

	local loadsucc = dotnet.load("GmodUltralight")
	if not loadsucc then
		testfailed = true
	end
	if testfailed then
		return
	end

	PrintTable(Ultralight)

	print("Creating View")
	
	view = view || Ultralight.CreateView(512,512,true)
	
	print(view)
	
	print("LoadURL")
	view:LoadURL("https://github.com")
	
	print("Loading URL")
	local loaded = view:UpdateUntilLoads(view)
	print(loaded)
	
	print("Render")
	Ultralight.Render()
	
	print("GetPixel")
	local a,r,g,b = view:GetPixel(0,0)
	print(a)
	print(r)
	print(g)
	print(b)
	
	print("FireScrollEvent")
	view:FireScrollEvent(1,0,1)

	print("Bake")
	view:Bake();
	
	print("Dispose")
	--view:Dispose()
	view = null
	collectgarbage()

    local unloaded = dotnet.unload("GmodUltralight")
	if not unloaded then
		print("Failed to unload")
		testfailed = true
	end
	if testfailed then
		return
	end
	-- todo :D
end

hook.Add("Tick","GmodUltralight_Test", function()
	if not testdone then
		testdone = true
		local succ, err = pcall(test)
		if not succ then
			ErrorNoHalt(err)
		end
		if not testfailed then
			file.Write("success.txt", "done")
		end
		print("Tests completed!")
	end
	engine.CloseServer()
end)

--Ultralight.View_SV_DrawDirty(view)

-- CLIENT

--[[local textureRT = GetRenderTarget( "UltralightView", 512, 512 )
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
]]
