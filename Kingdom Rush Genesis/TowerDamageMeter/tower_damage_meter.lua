-- Entry point for Genesis's -custom_script launch option.
custom_script = {}

local owners

local function remember(_, e)
    if e.tower and not e.tower_holder then
        owners[e.id] = owners[e.id] or {total = 0}
    elseif not (e.enemy or e.hero) then
        local parent = (e.soldier and e.soldier.tower_id) or e.tower_id
            or (e.tower_ref and e.tower_ref.id) or (e.bullet and e.bullet.source_id)
            or (e.modifier and e.modifier.source_id) or (e.aura and e.aura.source_id)
            or e.source_id
        if parent then owners[e.id] = owners[parent] end
    end
    return true -- simulation insert observers must not veto the insertion
end

local function count_hits(health, dt, ts, store)
    local queue = store.damage_queue
    if #queue == 0 then return health:on_update(dt, ts, store) end

    local hp = {}
    for _, hit in ipairs(queue) do
        if hit.damage_applied == nil and not hp[hit.target_id] then
            local target = store.entities[hit.target_id]
            if target and target.enemy and target.health then
                hp[hit.target_id] = math.max(0, target.health.hp)
            end
        end
    end

    health:on_update(dt, ts, store)

    -- Retail resolves newest first, removes last tick's results, and leaves
    -- this tick's hits in the queue. Newly queued retaliation has no result yet.
    for i = #queue, 1, -1 do
        local hit = queue[i]
        local left = hp[hit.target_id]
        if left and left > 0 and hit.damage_applied and hit.damage_applied > 0 then
            local dealt = math.min(left, hit.damage_applied)
            hp[hit.target_id] = left - dealt -- hero hits also consume enemy HP
            local owner = owners[hit.source_id]
            if owner then owner.total = owner.total + dealt end
        end
    end
end

local function add_readout(bar)
    local label = GGLabel:new({x = 164, y = 22})
    label.pos = {x = (bar.size.x - label.size.x) / 2, y = -24}
    label.font_name = "fla_h2"
    label.font_size = 14
    label.text_align = "center"
    label.vertical_align = "middle"
    label.colors.background = {48, 34, 23, 240}
    label.colors.text = {255, 228, 166, 255}
    label.hidden = true
    bar:ci("infobar_part_a"):add_child(label)

    local update_stats = bar.update_stats
    function bar:update_stats(e)
        update_stats(self, e)
        local counter = e.tower and not e.tower_holder and owners[e.id]
        label.hidden = not counter
        if counter then
            local digits = tostring(math.floor(counter.total)):reverse():gsub("(%d%d%d)", "%1,")
            label.text = "Damage dealt: " .. digits:reverse():gsub("^,", "")
        end
    end
end

function custom_script:init()
    local signal = require("hump.signal")
    signal.register("game-start", function(store)
        owners = {}
        local sim = game.simulation
        table.insert(sim.systems_on_insert, {on_insert = remember})
        -- The simulation rebuilds its callback lists for every mission/restart.
        for i, system in ipairs(sim.systems_on_update) do
            if system.name == "health" then
                sim.systems_on_update[i] = {on_update = function(_, dt, ts, s)
                    count_hits(system, dt, ts, s)
                end}
                break
            end
        end
        add_readout(game.game_gui.window:ci("infobar_view"))
    end)
    signal.register("tower-upgraded", function(new, old)
        -- Share the counter: projectiles already in flight still belong to it.
        if not new.tower_holder then owners[new.id] = owners[old.id] end
    end)
end
