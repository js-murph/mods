-- Entry point for Genesis's -custom_script launch option.
custom_script = {}

local rooms = {hero_room_view = "heroes", tower_room_view = "towers", power_room_view = "spells"}

local function roll(room, slot)
    local selected = slot[room.savegame_group][room.savegame_selection]
    local pool = {}
    for _, unit in ipairs(room.units_order) do
        if room:content_lock_state(unit, slot) == "available" then
            pool[#pool + 1] = unit
        end
    end

    -- The UI shows slots beyond the saved selection as locked. Don't open them
    -- or partially replace a loadout if the eligible roster can't fill it.
    if #pool < #selected then return end
    for i = 1, #selected do
        local j = love.math.random(i, #pool)
        pool[i], pool[j] = pool[j], pool[i]
        if pool[i] ~= selected[i] then
            screen_map:equip_unit(room.savegame_group, pool[i], i)
        end
    end
end

local function randomize(room, all)
    local window = room:get_window()
    if window.responder ~= room or room.showing_upgrades or room.picking_units_slot then return end
    local slot = require("storage"):load_slot()
    if all then
        for id in pairs(rooms) do roll(window:ci(id), slot) end
    else
        roll(room, slot)
    end
    room:refresh_all_rosters()
    local equipped = room:get_selected_units()
    if equipped[1] then room:show_unit(equipped[1]) end
end

local function add_button(header, ctx, x, title, on_click)
    local template = require("klove.kui_db"):get_table("button_room_select", ctx)
    local button = GG5Button:new_from_table(template)
    button.pos = {x = x, y = -309}
    button.scale = {x = 1.1, y = 0.93}
    local label = button:ci("label_button_select")
    label.font_size = 26
    label.pos = {x = -181, y = -32}
    label.size.x = 315
    label.text = title
    button.on_click = on_click
    header:add_child(button)
end

function custom_script:init()
    require("hump.signal").register(SGN_DIRECTOR_ITEM_SHOWN, function(name, screen)
        if name ~= "map" then return end
        local ctx = require("screen_utils").new_screen_ctx(screen)
        for id, category in pairs(rooms) do
            local room = screen.window:ci(id)
            local header = room:ci("l11_room_desktop_bg")
            add_button(header, ctx, -240, "Randomize " .. category, function() randomize(room, false) end)
            add_button(header, ctx, 700, "Randomize all", function() randomize(room, true) end)
        end
    end)
end
