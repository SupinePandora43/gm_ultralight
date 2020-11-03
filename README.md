# gm_ultralight ![.NET Core](https://github.com/SupinePandora43/gm_ultralight/workflows/.NET%20Core/badge.svg)

[Ultralight](https://github.com/ultralight-ux/Ultralight) gmod integration

# WIP

* Crashes on 2nd load - FIX in ultralight 1.3 (IPC)
* use ISurface - help,help,help,help,help,help,help,help,help,help,help,help,help,help,help,help,help - GmodDotNet 2.0

# HOW TO USE

###### TODO: userdata - GmodDotNet wait sample...

1. create view with `myview = Ultralight_createView(`width, height, transparent???`)`
2. load url `UltralightView_LoadURL(`myview, URL`)`
3. update renderer, until view is loaded `UltralightView_UpdateUntilLoads(`myview`)`
4. image is saved in `csresult.png` // what??? yep, i work on it OwO


# Contributors
[SupinePandora43](https://github.com/SupinePandora43)

[GlebChili](https://github.com/GlebChili)

**`NULL`@`mijyuoon#6666`** - [`vgui::ISurface->DrawSetTextureRGBA`](https://discord.com/channels/565105920414318602/565108080300261398/723218859322114161)

**`Eclipse`@`Eclipse#2437`** - [`Sys_LoadInterface("vguimatsurface", "VGUI_Surface030", NULL, (void**)&surface)`](https://discord.com/channels/565105920414318602/567672652714475530/723205466838270024)

[Uros Spasojevic](https://app.slack.com/client/TC4C8F4CT/CC492VBLL/user_profile/ULE28P1AL) - uint8_t
