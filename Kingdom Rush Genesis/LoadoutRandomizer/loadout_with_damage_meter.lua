-- Genesis accepts one custom script; register both mods' event listeners.
require("tower_damage_meter")
local meter = custom_script
require("loadout_randomizer")
local randomizer = custom_script
custom_script = {init = function()
    meter:init()
    randomizer:init()
end}
