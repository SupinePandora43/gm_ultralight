hook.Add("Tick", "CloseServer", engine.CloseServer)
-- load GmodDotNet
require("dotnet")
-- test function
local function run_test()
	print("Running test")

	local module_loaded = dotnet.load("GmodUltralight")
	assert(module_loaded==true)
	---------------------------------------------------
	PrintTable(Ultralight)

	print("Creating View")
	
	view = view || Ultralight.CreateView(128,128,true)
	
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
	
	--print("ToAscii")
	--print(view:ToAscii())
	
	local dohavelines = false
	surface = {}
	surface.SetDrawColor = function(r,g,b,a) end
	surface.DrawRect = function(x,y,w,h) if w~=1 then dohavelines=true end end

	print("DrawToSurfaceByLines")
	local DrawToSurfaceByLinesStart = SysTime()
	view:DrawToSurfaceByLines()
	local DrawToSurfaceByLinesEnd = SysTime()
	print("DrawToSurfaceByLines", DrawToSurfaceByLinesEnd-DrawToSurfaceByLinesStart)

	print("DrawToSurface")
	local DrawToSurfaceStart = SysTime()
	view:DrawToSurface()
	local DrawToSurfaceEnd = SysTime()
	print("DrawToSurface", DrawToSurfaceEnd-DrawToSurfaceStart)

	assert(dohavelines)

	local ToJsonRGBXY_start = SysTime()
	local RGBXY = view:ToJsonRGBXY()
	local ToJsonRGBXY_end = SysTime()

	print("RGBXY", ToJsonRGBXY_end-ToJsonRGBXY_start)

	print(RGBXY)

	print("Dispose")
	--view:Dispose()
	view = null
	collectgarbage()

	---------------------------------------------------
    local module_unloaded = dotnet.unload("GmodUltralight")
	assert(module_unloaded==true)
end

run_test()
print("tests are successful!")
file.Write("success.txt", "done")

if CLIENT then

	Ultralight = Ultralight or {}
	function Ultralight.DrawFromDataRGBXY(data)
		local jsonString = util.Decompress(data)
		local tblToDraw = util.JSONToTable(jsonString)

		local i = 0
		local length = #tblToDraw
        while i < length do
            i = i +1
            local obj = tblToDraw[i]
			surface.SetDrawColor(obj[1],obj[2],obj[3])
			surface.DrawRect(obj[4],obj[5],1,1)
        end
	end
	-- TODO:
	function Ultralight.DrawFromDataRGBAXY(data)
		local jsonString = util.Decompress(data)
		local tblToDraw = util.JSONToTable(jsonString)

		local i = 0
		local length = #tblToDraw
        while i < length do
            i = i +1
            local obj = tblToDraw[i]
			surface.SetDrawColor(obj[1],obj[2],obj[3],obj[4])
			surface.DrawRect(obj[5],obj[6],1,1)
        end
	end
	function Ultralight.DrawFromDataRGBXYWH(data)
		local jsonString = util.Decompress(data)
		local tblToDraw = util.JSONToTable(jsonString)

		local i = 0
		local length = #tblToDraw
        while i < length do
            i = i +1
            local obj = tblToDraw[i]
			surface.SetDrawColor(obj[1],obj[2],obj[3])
			surface.DrawRect(obj[4],obj[5],obj[6],obj[7])
        end
	end
	function Ultralight.DrawFromDataRGBAXYWH(data)
		local jsonString = util.Decompress(data)
		local tblToDraw = util.JSONToTable(jsonString)

		local i = 0
		local length = #tblToDraw
        while i < length do
            i = i +1
            local obj = tblToDraw[i]
			surface.SetDrawColor(obj[1],obj[2],obj[3],obj[4])
			surface.DrawRect(obj[5],obj[6],obj[7],obj[8])
        end
	end
end

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
