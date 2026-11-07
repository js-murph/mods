-- Offline contract tests. These doubles do not establish UE4SS/game compatibility.
package.path = "scripts/?.lua;" .. package.path
local plan = require("squad_plan")
local count = 0
local function test(name, run)
    run()
    count = count + 1
    print("ok - " .. name)
end

test("restricted slots can displace an earlier unrestricted pick", function()
    local choices = {{"A", "B"}, {"A"}}
    local result = plan(choices, function(i) return i end)
    assert(result[1] == "B" and result[2] == "A")
    assert(choices[1][1] == "A" and choices[1][2] == "B")
    assert(plan({{"A"}, {"A"}}) == nil)
    assert(plan({{}}) == nil)
    assert(#plan({}) == 0)
end)

local function object(values)
    values = values or {}
    function values:IsValid() return self.valid ~= false end
    function values:GetAddress() return tostring(self) end
    return values
end

local null = {IsValid = function() return false end}
local function array(values)
    return {ForEach = function(_, visit)
        for i, value in ipairs(values) do
            visit(i, {get = function() return value end})
        end
    end}
end

local function character(id, status)
    local value = object({id = id, equipment = "unchanged"})
    function value:GetCharacterID() return {A = id, B = 7, C = 11, D = 13} end
    function value:IsAvailable() return status ~= "unavailable" end
    function value:IsDead() return status == "dead" end
    function value:IsDismissed() return status == "dismissed" end
    function value:IsCriticallyInjured() return status == "critical" end
    return value
end

local selectionState = "br.UI.Menu.Strategy.Holotable.Region.Location.TacticalMission.MissionBriefing.SquadSelect.Selection"
local function fixture()
    local f = {logs = {}, queue = {}, state = selectionState}
    f.hero, f.a, f.b = character(1), character(2), character(3)
    f.characters = {f.hero, f.a, f.b, character(2), character(4, "dead"),
        character(5, "dismissed"), character(6, "critical"), character(7, "unavailable")}
    f.slots = {}
    local function slot(current, allowed)
        local value = object({AssignedCharacterVM = current, index = #f.slots * 3})
        function value:GetSlotIndex() return self.index end
        function value:DoesCharacterSatisfyRequirement(candidate)
            return not allowed or allowed[candidate.id] == true
        end
        function value:ClearSlot() self.AssignedCharacterVM = null end
        function value:AssignCharacterToSlot(candidate)
            if self.reject then return end
            if self.failOnce then self.failOnce = false; error("Simulated native failure") end
            assert(self:DoesCharacterSatisfyRequirement(candidate))
            for _, other in ipairs(f.slots) do
                if other ~= self and other.AssignedCharacterVM.id == candidate.id then
                    return -- Simulate the game refusing a duplicate.
                end
            end
            self.AssignedCharacterVM = candidate
        end
        f.slots[#f.slots + 1] = value
        return value
    end
    f.required = slot(f.hero, {[1] = true})
    f.first, f.second = slot(f.a), slot(f.b)
    f.screen = object({active = true, visible = true, enabled = true, isBuildOnComplete = true})
    function f.screen:IsActivated() return self.active end
    function f.screen:IsVisible() return self.visible end
    function f.screen:GetIsEnabled() return self.enabled end
    function f.screen:GetOwningPlayer() return object() end
    local panel = object()
    function panel:AddChildToHorizontalBox(button)
        if f.panelFailure then return null end
        button.parent = self
        return object({SetPadding = function() end, SetVerticalAlignment = function() end})
    end
    f.screen.EditSquadButton = object({ButtonType = 0, ButtonOrientation = 0, ButtonSize = 1,
        GetParent = function() return panel end})
    f.buttons = {}
    local widgets = object()
    function widgets:Create()
        local button = object({visibility = 0, enabled = true})
        function button:SetVisibility(value) self.visibility = value end
        function button:IsVisible() return self.visibility == 0 end
        function button:GetIsEnabled() return self.enabled end
        function button:RemoveFromParent() self.parent = nil end
        function button:SetShouldUseFallbackDefaultInputAction() end
        function button:SetHideInputAction() end
        function button:SetRequiresHold() end
        function button:UpdateButtonStyle() end
        function button:SetButtonText(text) self.text = text end
        function button:SetToolTipText(text) self.tooltip = text end
        f.buttons[#f.buttons + 1] = button
        return button
    end
    local mission = object({RequiredSlotVMs = array({f.required}), FlexibleSlotVMs = array({f.first, f.second})})
    local function displaySlot(native, required)
        local value = object({Index = native.index, isRequired = required,
            CharacterVM = native.AssignedCharacterVM, ActivityVM = mission})
        function value:SetCharacter(characterVM)
            if self.failOnce then self.failOnce = false; error("Simulated display failure") end
            self.CharacterVM = characterVM or null
        end
        return value
    end
    f.requiredDisplay = displaySlot(f.required, true)
    f.firstDisplay, f.secondDisplay = displaySlot(f.first, false), displaySlot(f.second, false)
    -- The display's array order is not the native slot index.
    f.displaySlots = {f.secondDisplay, f.requiredDisplay, f.firstDisplay}
    local slotList = object({ActivitySlots = array(f.displaySlots)})
    function f.screen:UpdateCharacterHologram()
        f.holograms = {f.firstDisplay.CharacterVM, f.secondDisplay.CharacterVM}
    end
    local roster = object({RosterCharacterViewModels = array(f.characters)})
    local ui = object({State = {TagName = {ToString = function() return f.state end}}})
    local library = object()
    function library:GetViewModel(screen, class, name)
        assert(screen == f.screen and name == "Default")
        if f.missingBinding then return null end
        if class.path == "/Script/Bruno.BrunoMissionViewModel" then return mission end
        if class.path == "/Script/Bruno.BrunoRosterViewModel" then return roster end
        if class.path == "/Game/Game/UI/Strategy/GalaxyMap/BPs/VM_ActivitySlotList.VM_ActivitySlotList_C" then return slotList end
        assert(class.path == "/Game/Game/UI/Strategy/SquadSelect/BPs/VM_SquadSelect.VM_SquadSelect_C")
        return ui
    end
    local environment = setmetatable({
        Key = {F8 = 119},
        FName = function(value) return value end,
        FText = function(value) return value end,
        print = function(message) f.logs[#f.logs + 1] = message end,
        FindAllOf = function(class)
            assert(class == "WBP_Menu_MissionSquadSelect_C")
            return {f.screen}
        end,
        StaticFindObject = function(path)
            if path == "/Script/MDViewModel.Default__MDViewModelFunctionLibrary" then return library end
            if path == "/Script/UMG.Default__WidgetBlueprintLibrary" then return widgets end
            return object({path = path})
        end,
        ExecuteInGameThread = function(callback) f.queue[#f.queue + 1] = callback end,
        RegisterKeyBind = function(key, callback) assert(key == 119); f.press = callback end,
        RegisterHook = function(path, callback)
            assert(path == "/Script/CommonUI.CommonButtonBase:HandleButtonClicked")
            f.click = function(button) callback({get = function() return button end}) end
        end,
        LoopInGameThreadWithDelay = function(_, callback) f.refresh = callback; return 1 end,
        CancelDelayedAction = function() f.refresh = nil end,
    }, {__index = _G})
    assert(loadfile("scripts/main.lua", "t", environment))()
    function f:run()
        self.press()
        table.remove(self.queue, 1)()
    end
    function f:original()
        assert(self.required.AssignedCharacterVM == self.hero)
        assert(self.first.AssignedCharacterVM == self.a)
        assert(self.second.AssignedCharacterVM == self.b)
    end
    function f:message(text)
        assert(self.logs[#self.logs]:find(text, 1, true), self.logs[#self.logs])
    end
    return f
end

test("hotkey works in Selection and Confirmation states while preserving eligibility", function()
    math.randomseed(271828)
    for _, state in ipairs({selectionState, selectionState:gsub("Selection$", "Confirmation"), selectionState:upper()}) do
        local f = fixture()
        f.state = state
        local outcomes = {}
        for _ = 1, 30 do
            f:run()
            assert(f.required.AssignedCharacterVM == f.hero)
            local a, b = f.first.AssignedCharacterVM.id, f.second.AssignedCharacterVM.id
            assert((a == 2 and b == 3) or (a == 3 and b == 2))
            outcomes[a] = true
            for _, value in ipairs(f.characters) do assert(value.equipment == "unchanged") end
            f:message("Randomized 2 flexible slots")
        end
        assert(outcomes[2] and outcomes[3])
    end
end)

test("rerolls update the displayed and pending squad, not just native mission slots", function()
    local f = fixture()
    local recruit = character(9)
    table.insert(f.characters, recruit)
    -- Force a new recruit, not merely a permutation of the original team.
    function f.first:DoesCharacterSatisfyRequirement(c) return c.id == 9 end
    function f.second:DoesCharacterSatisfyRequirement(c) return c.id == 2 end
    f:run()
    f:message("Randomized 2 flexible slots")
    assert(f.firstDisplay.CharacterVM == recruit and f.secondDisplay.CharacterVM == f.a)
    assert(f.requiredDisplay.CharacterVM == f.hero)
    assert(f.holograms[1] == recruit and f.holograms[2] == f.a)
    -- The game's confirmation copies pending display choices into the mission.
    f.first:ClearSlot()
    f.second:ClearSlot()
    f.first:AssignCharacterToSlot(f.firstDisplay.CharacterVM)
    f.second:AssignCharacterToSlot(f.secondDisplay.CharacterVM)
    assert(f.first.AssignedCharacterVM == recruit and f.second.AssignedCharacterVM == f.a)
end)

test("inactive screens and individual-slot assignment cannot change the squad", function()
    for _, property in ipairs({"active", "visible", "enabled"}) do
        local f = fixture()
        f.screen[property] = false
        f:run()
        f:original()
        f:message("Open the mission squad screen")
    end
    for _, state in ipairs({selectionState:gsub("Selection$", "Assignment"), "None", "Other.Confirmation"}) do
        local f = fixture()
        f.state = state
        f:run()
        f:original()
        f:message("Cannot randomize in squad screen state: " .. state)
    end
end)

test("incomplete required slots and unavailable bindings fail before mutation", function()
    local f = fixture()
    f.required.AssignedCharacterVM = null
    f:run()
    assert(f.required.AssignedCharacterVM == null)
    assert(f.first.AssignedCharacterVM == f.a and f.second.AssignedCharacterVM == f.b)
    f:message("fill required slots first")
    f = fixture()
    f.missingBinding = true
    f:run()
    f:original()
    f:message("no supported binding found")
    for _, mismatch in ipairs({"Index", "ActivityVM", "isRequired"}) do
        f = fixture()
        if mismatch == "Index" then f.firstDisplay.Index = 99 end
        if mismatch == "ActivityVM" then f.firstDisplay.ActivityVM = object() end
        if mismatch == "isRequired" then f.firstDisplay.isRequired = true end
        f:run()
        f:original()
        assert(f.firstDisplay.CharacterVM == f.a and f.secondDisplay.CharacterVM == f.b)
        assert(not f.logs[#f.logs]:find("Randomized", 1, true))
    end
end)

test("impossible slot requirements leave the original squad unchanged", function()
    local f = fixture()
    function f.second:DoesCharacterSatisfyRequirement() return false end
    f:run()
    f:original()
    f:message("squad unchanged")
end)

test("partial application errors restore the original squad, including empty slots", function()
    for _, initiallyEmpty in ipairs({false, true}) do
        local f = fixture()
        if initiallyEmpty then f.first.AssignedCharacterVM = null end
        f.second.failOnce = true
        f:run()
        assert(f.required.AssignedCharacterVM == f.hero)
        assert(f.first.AssignedCharacterVM == (initiallyEmpty and null or f.a))
        assert(f.second.AssignedCharacterVM == f.b)
        f:message("original squad restored")
        f:run()
        f:message("Randomized 2 flexible slots")
    end
end)

test("display failures restore both pending choices and native mission slots", function()
    local f = fixture()
    -- Pending edits can differ from native slots until the game copies them.
    f.firstDisplay.CharacterVM = null
    f.secondDisplay.CharacterVM = f.a
    f.secondDisplay.failOnce = true
    f:run()
    f:message("original squad restored")
    f:original()
    assert(f.firstDisplay.CharacterVM == null and f.secondDisplay.CharacterVM == f.a)
    assert(f.holograms[1] == null and f.holograms[2] == f.a)
end)

test("silent native rejection and failed restoration never report success", function()
    local f = fixture()
    f.second.reject = true
    f:run()
    f:message("Assignment failed AND restore failed")
    assert(f.required.AssignedCharacterVM == f.hero)
end)

test("repeated keypresses cannot queue overlapping rerolls", function()
    local f = fixture()
    f.press()
    f.press()
    f:original()
    assert(#f.queue == 1)
    table.remove(f.queue, 1)()
    f:message("Randomized 2 flexible slots")
    f:run()
    f:message("Randomized 2 flexible slots")
end)

test("Randomize button shares F8 behavior and is limited to the full lineup", function()
    local f = fixture()
    f:refresh()
    assert(#f.buttons == 0)
    f.state = selectionState:gsub("Selection$", "Confirmation")
    f:refresh()
    local button = f.buttons[1]
    assert(button.text == "Randomize" and button:IsVisible() and button.parent)
    for _ = 1, 4 do f:refresh() end
    assert(#f.buttons == 1)
    f.click(f.screen.EditSquadButton)
    assert(#f.queue == 0)
    f.click(button)
    f.press()
    assert(#f.queue == 1)
    table.remove(f.queue, 1)()
    f:message("Randomized 2 flexible slots")
    assert(f.firstDisplay.CharacterVM == f.first.AssignedCharacterVM)
    assert(f.secondDisplay.CharacterVM == f.second.AssignedCharacterVM)
    for _, state in ipairs({selectionState, (selectionState:gsub("Selection$", "Assignment"))}) do
        f.state = state
        f.click(button) -- A stale click before the visibility refresh is also rejected.
        assert(#f.queue == 0)
        f:refresh()
        assert(not button:IsVisible())
    end
    f.state = selectionState:gsub("Selection$", "Confirmation")
    f.screen.active = false
    f:refresh()
    f.click(button)
    assert(not button:IsVisible() and #f.queue == 0)
    f.screen.active = true
    f:refresh()
    assert(#f.buttons == 1 and button:IsVisible())
    button.valid = false -- Rebuild after the widget has been collected.
    f:refresh()
    assert(#f.buttons == 2 and f.buttons[2]:IsVisible())
end)

test("button setup failure leaves F8 available and removes the partial widget", function()
    local f = fixture()
    f.state = selectionState:gsub("Selection$", "Confirmation")
    f.panelFailure = true
    f:refresh()
    f:message("Button unavailable; F8 still works")
    assert(f.refresh == nil and f.buttons[1].parent == nil)
    f:run()
    f:message("Randomized 2 flexible slots")
end)

print(string.format("Passed %d offline tests (not an in-game test).", count))
