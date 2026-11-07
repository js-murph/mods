-- A complete, duplicate-free assignment, or nil. Inputs are never modified.
-- Each entry lists the character IDs eligible for that slot. Required characters
-- must already have been excluded by the caller.
return function(eligible, random)
    random = random or math.random
    local function shuffled(values)
        local copy = {table.unpack(values)}
        for i = #copy, 2, -1 do
            local j = random(i)
            copy[i], copy[j] = copy[j], copy[i]
        end
        return copy
    end

    local choices, order = {}, {}
    for i, candidates in ipairs(eligible) do
        choices[i] = shuffled(candidates)
        order[i] = i
    end

    local owner, selected = {}, {}
    local function assign(slot, visited)
        for _, id in ipairs(choices[slot]) do
            if not visited[id] then
                visited[id] = true
                -- Move an earlier pick if a restricted slot needs that recruit.
                if not owner[id] or assign(owner[id], visited) then
                    owner[id] = slot
                    selected[slot] = id
                    return true
                end
            end
        end
        return false
    end

    for _, slot in ipairs(shuffled(order)) do
        if not assign(slot, {}) then
            return nil
        end
    end
    return selected
end
