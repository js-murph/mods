-- PatternSleuth's ConsoleManagerSingleton resolver expects a function referencing
-- r.DumpingMovie, NOT the singleton pointer. Four such functions exist in this
-- build; this identifies one unique initializer (RVA 0x01BA0220 in build 25898267).
function Register()
    return "40 55 48 83 EC 30 E8 ? ? ? ? 83 78 58 00 74 06 48 8B 68 50 EB 07 48 8D 2D ? ? ? ? 48 8B 0D ? ? ? ? 48 85 C9 0F 85 FB 01 00 00 48 89 7C 24 50 BA 10 00 00 00 B9 F0 01 00 00 48 89 74 24 48"
end

function OnMatchFound(MatchAddress)
    return MatchAddress
end
