-- FUObjectHashTables::Get(), verified against the build 25898267 PDB.
-- Include the hash-map initialization: the lock-only prologue matches 23 functions.
function Register()
    return "40 53 48 83 EC 20 65 48 8B 04 25 58 00 00 00 B9 ? ? ? ? 48 8B 00 8B 04 01 39 05 ? ? ? ? 7F 0D 48 8D 05 ? ? ? ? 48 83 C4 20 5B C3 48 8D 0D ? ? ? ? E8 ? ? ? ? 83 3D ? ? ? ? FF 75 DE 48 8D 1D ? ? ? ? 48 8B CB FF 15 ? ? ? ? BA A0 0F 00 00 48 8B CB FF 15 ? ? ? ? 66 0F 6F 0D ? ? ? ? 48 8D 0D ? ? ? ? 33 C0 0F 57 C0 48 89 05 ? ? ? ? 48 89 05 ? ? ? ? 48 89 05 ? ? ? ? F3 0F 7F 0D ? ? ? ?"
end

function OnMatchFound(MatchAddress)
    return MatchAddress
end
