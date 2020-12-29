hook.Add("Tick", "CloseServer", engine.CloseServer)
-- load GmodDotNet
require("dotnet")

local function test(name, fn)
	local testStart = SysTime()
	local result = fn()
	local testEnd = SysTime()

	local time = testEnd-testStart

	print(name+" ("+ tostring(time) + ")")

	return result
end

-- test function
local function run_test()
	assert(test("Load", function()
		return dotnet.load("GmodUltralight")
	end))
	---------------------------------------------------
	PrintTable(Ultralight)

	view = view or test("CreateView", function()
		return Ultralight.CreateView(512,512,false)
	end)
	
	test("LoadURL", function()
		view:LoadURL("https://supinepandora43.github.io/electron-app/")
	end)
	
	assert(test("UpdateUntilLoads", function()
		return view:UpdateUntilLoads(view)
	end))
	
	test("Render", function()
		Ultralight.Render()
	end)
	
	test("GetPixel", function()
		return view:GetPixel(0,0)
	end)

	test("Bake", function()
		view:Bake("bake");
	end)
	
	local dohavelines = false

	surface = {}
	surface.SetDrawColor = function(r,g,b,a) end
	surface.DrawRect = function(x,y,w,h) if w~=1 then dohavelines=true end end

	test("DrawToSurfaceByLines", function()
		view:DrawToSurfaceByLines()
	end)

	test("DrawToSurface", function()
		view:DrawToSurface()
	end)
	
	test("ToJsonRGBXY", function()
		view:ToJsonRGBXY()
	end)

	test("FireKeyEvent", function()
		view:FireKeyEvent(Ultralight.KeyEventType.Char, "a", "a")
	end)

	test("FireScrollEvent", function()
		view:FireScrollEvent(1,0,1)
	end)


	test("null", function()
		view = null
		collectgarbage()
	end)

	assert(dohavelines)
	---------------------------------------------------
    assert(test("Unload", function()
		return dotnet.unload("GmodUltralight")
	end))
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
