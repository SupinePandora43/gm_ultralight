namespace GmodUltralight
{
    public enum KeyCode
    {
        // GK_BACK (08) BACKSPACE key
        GK_BACK = 0x08,

        // GK_TAB (09) TAB key
        GK_TAB = 0x09,

        // GK_CLEAR (0C) CLEAR key
        GK_CLEAR = 0x0C,

        // GK_RETURN (0D)
        GK_RETURN = 0x0D,

        // GK_SHIFT (10) SHIFT key
        GK_SHIFT = 0x10,

        // GK_CONTROL (11) CTRL key
        GK_CONTROL = 0x11,

        // GK_MENU (12) ALT key
        GK_MENU = 0x12,

        // GK_PAUSE (13) PAUSE key
        GK_PAUSE = 0x13,

        // GK_CAPITAL (14) CAPS LOCK key
        GK_CAPITAL = 0x14,

        // GK_KANA (15) Input Method Editor (IME) Kana mode
        GK_KANA = 0x15,

        // GK_HANGUEL (15) IME Hanguel mode (maintained for compatibility; use GK_HANGUL)
        // GK_HANGUL (15) IME Hangul mode
        GK_HANGUL = 0x15,

        // GK_JUNJA (17) IME Junja mode
        GK_JUNJA = 0x17,

        // GK_FINAL (18) IME final mode
        GK_FINAL = 0x18,

        // GK_HANJA (19) IME Hanja mode
        GK_HANJA = 0x19,

        // GK_KANJI (19) IME Kanji mode
        GK_KANJI = 0x19,

        // GK_ESCAPE (1B) ESC key
        GK_ESCAPE = 0x1B,

        // GK_CONVERT (1C) IME convert
        GK_CONVERT = 0x1C,

        // GK_NONCONVERT (1D) IME nonconvert
        GK_NONCONVERT = 0x1D,

        // GK_ACCEPT (1E) IME accept
        GK_ACCEPT = 0x1E,

        // GK_MODECHANGE (1F) IME mode change request
        GK_MODECHANGE = 0x1F,

        // GK_SPACE (20) SPACEBAR
        GK_SPACE = 0x20,

        // GK_PRIOR (21) PAGE UP key
        GK_PRIOR = 0x21,

        // GK_NEXT (22) PAGE DOWN key
        GK_NEXT = 0x22,

        // GK_END (23) END key
        GK_END = 0x23,

        // GK_HOME (24) HOME key
        GK_HOME = 0x24,

        // GK_LEFT (25) LEFT ARROW key
        GK_LEFT = 0x25,

        // GK_UP (26) UP ARROW key
        GK_UP = 0x26,

        // GK_RIGHT (27) RIGHT ARROW key
        GK_RIGHT = 0x27,

        // GK_DOWN (28) DOWN ARROW key
        GK_DOWN = 0x28,

        // GK_SELECT (29) SELECT key
        GK_SELECT = 0x29,

        // GK_PRINT (2A) PRINT key
        GK_PRINT = 0x2A,

        // GK_EXECUTE (2B) EXECUTE key
        GK_EXECUTE = 0x2B,

        // GK_SNAPSHOT (2C) PRINT SCREEN key
        GK_SNAPSHOT = 0x2C,

        // GK_INSERT (2D) INS key
        GK_INSERT = 0x2D,

        // GK_DELETE (2E) DEL key
        GK_DELETE = 0x2E,

        // GK_HELP (2F) HELP key
        GK_HELP = 0x2F,

        // (30) 0 key
        GK_0 = 0x30,

        // (31) 1 key
        GK_1 = 0x31,

        // (32) 2 key
        GK_2 = 0x32,

        // (33) 3 key
        GK_3 = 0x33,

        // (34) 4 key
        GK_4 = 0x34,

        // (35) 5 key
        GK_5 = 0x35,

        // (36) 6 key
        GK_6 = 0x36,

        // (37) 7 key
        GK_7 = 0x37,

        // (38) 8 key
        GK_8 = 0x38,

        // (39) 9 key
        GK_9 = 0x39,

        // (41) A key
        GK_A = 0x41,

        // (42) B key
        GK_B = 0x42,

        // (43) C key
        GK_C = 0x43,

        // (44) D key
        GK_D = 0x44,

        // (45) E key
        GK_E = 0x45,

        // (46) F key
        GK_F = 0x46,

        // (47) G key
        GK_G = 0x47,

        // (48) H key
        GK_H = 0x48,

        // (49) I key
        GK_I = 0x49,

        // (4A) J key
        GK_J = 0x4A,

        // (4B) K key
        GK_K = 0x4B,

        // (4C) L key
        GK_L = 0x4C,

        // (4D) M key
        GK_M = 0x4D,

        // (4E) N key
        GK_N = 0x4E,

        // (4F) O key
        GK_O = 0x4F,

        // (50) P key
        GK_P = 0x50,

        // (51) Q key
        GK_Q = 0x51,

        // (52) R key
        GK_R = 0x52,

        // (53) S key
        GK_S = 0x53,

        // (54) T key
        GK_T = 0x54,

        // (55) U key
        GK_U = 0x55,

        // (56) V key
        GK_V = 0x56,

        // (57) W key
        GK_W = 0x57,

        // (58) X key
        GK_X = 0x58,

        // (59) Y key
        GK_Y = 0x59,

        // (5A) Z key
        GK_Z = 0x5A,

        // GK_LWIN (5B) Left Windows key (Microsoft Natural keyboard)
        GK_LWIN = 0x5B,

        // GK_RWIN (5C) Right Windows key (Natural keyboard)
        GK_RWIN = 0x5C,

        // GK_APPS (5D) Applications key (Natural keyboard)
        GK_APPS = 0x5D,

        // GK_SLEEP (5F) Computer Sleep key
        GK_SLEEP = 0x5F,

        // GK_NUMPAD0 (60) Numeric keypad 0 key
        GK_NUMPAD0 = 0x60,

        // GK_NUMPAD1 (61) Numeric keypad 1 key
        GK_NUMPAD1 = 0x61,

        // GK_NUMPAD2 (62) Numeric keypad 2 key
        GK_NUMPAD2 = 0x62,

        // GK_NUMPAD3 (63) Numeric keypad 3 key
        GK_NUMPAD3 = 0x63,

        // GK_NUMPAD4 (64) Numeric keypad 4 key
        GK_NUMPAD4 = 0x64,

        // GK_NUMPAD5 (65) Numeric keypad 5 key
        GK_NUMPAD5 = 0x65,

        // GK_NUMPAD6 (66) Numeric keypad 6 key
        GK_NUMPAD6 = 0x66,

        // GK_NUMPAD7 (67) Numeric keypad 7 key
        GK_NUMPAD7 = 0x67,

        // GK_NUMPAD8 (68) Numeric keypad 8 key
        GK_NUMPAD8 = 0x68,

        // GK_NUMPAD9 (69) Numeric keypad 9 key
        GK_NUMPAD9 = 0x69,

        // GK_MULTIPLY (6A) Multiply key
        GK_MULTIPLY = 0x6A,

        // GK_ADD (6B) Add key
        GK_ADD = 0x6B,

        // GK_SEPARATOR (6C) Separator key
        GK_SEPARATOR = 0x6C,

        // GK_SUBTRACT (6D) Subtract key
        GK_SUBTRACT = 0x6D,

        // GK_DECIMAL (6E) Decimal key
        GK_DECIMAL = 0x6E,

        // GK_DIVIDE (6F) Divide key
        GK_DIVIDE = 0x6F,

        // GK_F1 (70) F1 key
        GK_F1 = 0x70,

        // GK_F2 (71) F2 key
        GK_F2 = 0x71,

        // GK_F3 (72) F3 key
        GK_F3 = 0x72,

        // GK_F4 (73) F4 key
        GK_F4 = 0x73,

        // GK_F5 (74) F5 key
        GK_F5 = 0x74,

        // GK_F6 (75) F6 key
        GK_F6 = 0x75,

        // GK_F7 (76) F7 key
        GK_F7 = 0x76,

        // GK_F8 (77) F8 key
        GK_F8 = 0x77,

        // GK_F9 (78) F9 key
        GK_F9 = 0x78,

        // GK_F10 (79) F10 key
        GK_F10 = 0x79,

        // GK_F11 (7A) F11 key
        GK_F11 = 0x7A,

        // GK_F12 (7B) F12 key
        GK_F12 = 0x7B,

        // GK_F13 (7C) F13 key
        GK_F13 = 0x7C,

        // GK_F14 (7D) F14 key
        GK_F14 = 0x7D,

        // GK_F15 (7E) F15 key
        GK_F15 = 0x7E,

        // GK_F16 (7F) F16 key
        GK_F16 = 0x7F,

        // GK_F17 (80H) F17 key
        GK_F17 = 0x80,

        // GK_F18 (81H) F18 key
        GK_F18 = 0x81,

        // GK_F19 (82H) F19 key
        GK_F19 = 0x82,

        // GK_F20 (83H) F20 key
        GK_F20 = 0x83,

        // GK_F21 (84H) F21 key
        GK_F21 = 0x84,

        // GK_F22 (85H) F22 key
        GK_F22 = 0x85,

        // GK_F23 (86H) F23 key
        GK_F23 = 0x86,

        // GK_F24 (87H) F24 key
        GK_F24 = 0x87,

        // GK_NUMLOCK (90) NUM LOCK key
        GK_NUMLOCK = 0x90,

        // GK_SCROLL (91) SCROLL LOCK key
        GK_SCROLL = 0x91,

        // GK_LSHIFT (A0) Left SHIFT key
        GK_LSHIFT = 0xA0,

        // GK_RSHIFT (A1) Right SHIFT key
        GK_RSHIFT = 0xA1,

        // GK_LCONTROL (A2) Left CONTROL key
        GK_LCONTROL = 0xA2,

        // GK_RCONTROL (A3) Right CONTROL key
        GK_RCONTROL = 0xA3,

        // GK_LMENU (A4) Left MENU key
        GK_LMENU = 0xA4,

        // GK_RMENU (A5) Right MENU key
        GK_RMENU = 0xA5,

        // GK_BROWSER_BACK (A6) Windows 2000/XP: Browser Back key
        GK_BROWSER_BACK = 0xA6,

        // GK_BROWSER_FORWARD (A7) Windows 2000/XP: Browser Forward key
        GK_BROWSER_FORWARD = 0xA7,

        // GK_BROWSER_REFRESH (A8) Windows 2000/XP: Browser Refresh key
        GK_BROWSER_REFRESH = 0xA8,

        // GK_BROWSER_STOP (A9) Windows 2000/XP: Browser Stop key
        GK_BROWSER_STOP = 0xA9,

        // GK_BROWSER_SEARCH (AA) Windows 2000/XP: Browser Search key
        GK_BROWSER_SEARCH = 0xAA,

        // GK_BROWSER_FAVORITES (AB) Windows 2000/XP: Browser Favorites key
        GK_BROWSER_FAVORITES = 0xAB,

        // GK_BROWSER_HOME (AC) Windows 2000/XP: Browser Start and Home key
        GK_BROWSER_HOME = 0xAC,

        // GK_VOLUME_MUTE (AD) Windows 2000/XP: Volume Mute key
        GK_VOLUME_MUTE = 0xAD,

        // GK_VOLUME_DOWN (AE) Windows 2000/XP: Volume Down key
        GK_VOLUME_DOWN = 0xAE,

        // GK_VOLUME_UP (AF) Windows 2000/XP: Volume Up key
        GK_VOLUME_UP = 0xAF,

        // GK_MEDIA_NEXT_TRACK (B0) Windows 2000/XP: Next Track key
        GK_MEDIA_NEXT_TRACK = 0xB0,

        // GK_MEDIA_PREV_TRACK (B1) Windows 2000/XP: Previous Track key
        GK_MEDIA_PREV_TRACK = 0xB1,

        // GK_MEDIA_STOP (B2) Windows 2000/XP: Stop Media key
        GK_MEDIA_STOP = 0xB2,

        // GK_MEDIA_PLAY_PAUSE (B3) Windows 2000/XP: Play/Pause Media key
        GK_MEDIA_PLAY_PAUSE = 0xB3,

        // GK_LAUNCH_MAIL (B4) Windows 2000/XP: Start Mail key
        GK_MEDIA_LAUNCH_MAIL = 0xB4,

        // GK_LAUNCH_MEDIA_SELECT (B5) Windows 2000/XP: Select Media key
        GK_MEDIA_LAUNCH_MEDIA_SELECT = 0xB5,

        // GK_LAUNCH_APP1 (B6) Windows 2000/XP: Start Application 1 key
        GK_MEDIA_LAUNCH_APP1 = 0xB6,

        // GK_LAUNCH_APP2 (B7) Windows 2000/XP: Start Application 2 key
        GK_MEDIA_LAUNCH_APP2 = 0xB7,

        // GK_OEM_1 (BA) Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ';:' key
        GK_OEM_1 = 0xBA,

        // GK_OEM_PLUS (BB) Windows 2000/XP: For any country/region, the '+' key
        GK_OEM_PLUS = 0xBB,

        // GK_OEM_COMMA (BC) Windows 2000/XP: For any country/region, the ',' key
        GK_OEM_COMMA = 0xBC,

        // GK_OEM_MINUS (BD) Windows 2000/XP: For any country/region, the '-' key
        GK_OEM_MINUS = 0xBD,

        // GK_OEM_PERIOD (BE) Windows 2000/XP: For any country/region, the '.' key
        GK_OEM_PERIOD = 0xBE,

        // GK_OEM_2 (BF) Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '/?' key
        GK_OEM_2 = 0xBF,

        // GK_OEM_3 (C0) Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '`~' key
        GK_OEM_3 = 0xC0,

        // GK_OEM_4 (DB) Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '[{' key
        GK_OEM_4 = 0xDB,

        // GK_OEM_5 (DC) Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the '\|' key
        GK_OEM_5 = 0xDC,

        // GK_OEM_6 (DD) Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the ']}' key
        GK_OEM_6 = 0xDD,

        // GK_OEM_7 (DE) Used for miscellaneous characters; it can vary by keyboard. Windows 2000/XP: For the US standard keyboard, the 'single-quote/double-quote' key
        GK_OEM_7 = 0xDE,

        // GK_OEM_8 (DF) Used for miscellaneous characters; it can vary by keyboard.
        GK_OEM_8 = 0xDF,

        // GK_OEM_102 (E2) Windows 2000/XP: Either the angle bracket key or the backslash key on the RT 102-key keyboard
        GK_OEM_102 = 0xE2,

        // GK_PROCESSKEY (E5) Windows 95/98/Me, Windows NT 4.0, Windows 2000/XP: IME PROCESS key
        GK_PROCESSKEY = 0xE5,

        // GK_PACKET (E7) Windows 2000/XP: Used to pass Unicode characters as if they were keystrokes. The GK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT,SendInput, WM_KEYDOWN, and WM_KEYUP
        GK_PACKET = 0xE7,

        // GK_ATTN (F6) Attn key
        GK_ATTN = 0xF6,

        // GK_CRSEL (F7) CrSel key
        GK_CRSEL = 0xF7,

        // GK_EXSEL (F8) ExSel key
        GK_EXSEL = 0xF8,

        // GK_EREOF (F9) Erase EOF key
        GK_EREOF = 0xF9,

        // GK_PLAY (FA) Play key
        GK_PLAY = 0xFA,

        // GK_ZOOM (FB) Zoom key
        GK_ZOOM = 0xFB,

        // GK_NONAME (FC) Reserved for future use
        GK_NONAME = 0xFC,

        // GK_PA1 (FD) PA1 key
        GK_PA1 = 0xFD,

        // GK_OEM_CLEAR (FE) Clear key
        GK_OEM_CLEAR = 0xFE,

        GK_UNKNOWN = 0,
    }
    public enum Modifiers : byte
    {
        /// Whether or not an ALT key is down
        kMod_AltKey = 1 << 0,

        /// Whether or not a Control key is down
        kMod_CtrlKey = 1 << 1,

        /// Whether or not a meta key (Command-key on Mac, Windows-key on Win) is down
        kMod_MetaKey = 1 << 2,

        /// Whether or not a Shift key is down
        kMod_ShiftKey = 1 << 3,
    };
}
