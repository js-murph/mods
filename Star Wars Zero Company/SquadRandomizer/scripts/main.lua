local plan = require("squad_plan")
local pending = false

local function log(message)
    print("[SquadRandomizer] " .. message .. "\n")
end

local function valid(object)
    return object ~= nil and object:IsValid()
end

local function requireObject(object, description)
    assert(valid(object), description .. " is unavailable; no supported binding found")
    return object
end

local function arrayValues(array)
    local values = {}
    array:ForEach(function(_, element)
        values[#values + 1] = requireObject(element:get(), "Array entry")
    end)
    return values
end

local function characterID(character)
    if not valid(character) then return nil end
    local id = character:GetCharacterID()
    assert(id.A ~= 0 or id.B ~= 0 or id.C ~= 0 or id.D ~= 0, "Character has no ID")
    return string.format("%s:%s:%s:%s", id.A, id.B, id.C, id.D)
end

local function binding(library, screen, path)
    local class = requireObject(StaticFindObject(path), "View-model class")
    return requireObject(library:GetViewModel(screen, class, FName("Default")), "Screen view model")
end

local function activeScreen()
    local screen
    for _, widget in ipairs(FindAllOf("WBP_Menu_MissionSquadSelect_C") or {}) do
        if valid(widget) and widget:IsActivated() and widget:IsVisible() and widget:GetIsEnabled() then
            assert(not screen, "Multiple active squad screens; refusing to choose one")
            screen = widget
        end
    end
    return screen
end

local function randomize()
    local screen = activeScreen()
    if not screen then
        log("Open the mission squad screen first.")
        return
    end

    local library = requireObject(StaticFindObject("/Script/MDViewModel.Default__MDViewModelFunctionLibrary"), "View-model library")
    local ui = binding(library, screen, "/Game/Game/UI/Strategy/SquadSelect/BPs/VM_SquadSelect.VM_SquadSelect_C")
    local state = ui.State.TagName:ToString()
    local normalized = state:lower() -- Unreal FNames are case-insensitive.
    local prefix = "br.ui.menu.strategy.holotable.region.location.tacticalmission.missionbriefing.squadselect."
    if normalized ~= prefix .. "selection" and normalized ~= prefix .. "confirmation" then
        log("Cannot randomize in squad screen state: " .. state .. ". Return to the squad overview first.")
        return
    end

    local mission = binding(library, screen, "/Script/Bruno.BrunoMissionViewModel")
    local roster = binding(library, screen, "/Script/Bruno.BrunoRosterViewModel")
    local required = arrayValues(mission.RequiredSlotVMs)
    local flexible = arrayValues(mission.FlexibleSlotVMs)
    if #flexible == 0 then
        log("This mission has no flexible squad slots.")
        return
    end

    -- The screen keeps pending choices separately and copies them into the
    -- mission on confirmation. Native assignments alone leave those choices stale.
    local slotList = binding(library, screen, "/Game/Game/UI/Strategy/GalaxyMap/BPs/VM_ActivitySlotList.VM_ActivitySlotList_C")
    local displayByIndex = {}
    local displaySlots = arrayValues(slotList.ActivitySlots)
    assert(#displaySlots == #required + #flexible, "Squad display is not ready")
    for _, display in ipairs(displaySlots) do
        assert(valid(display.ActivityVM) and display.ActivityVM:GetAddress() == mission:GetAddress(),
            "Squad display belongs to a different mission")
        assert(not displayByIndex[display.Index], "Squad display has duplicate slot indices")
        displayByIndex[display.Index] = display
    end
    local function displayFor(slot, isRequired)
        local display = requireObject(displayByIndex[slot:GetSlotIndex()], "Squad display slot")
        assert(display.isRequired == isRequired, "Squad display slot requirement mismatch")
        return display
    end

    -- Copy IDs, not references to temporary Unreal structs. Never clear or fill
    -- required slots: an unfilled story/type requirement needs manual selection.
    local reserved = {}
    for _, slot in ipairs(required) do
        local character = requireObject(slot.AssignedCharacterVM, "Required character (fill required slots first)")
        assert(slot:DoesCharacterSatisfyRequirement(character), "A required slot is not satisfied")
        local id = characterID(character)
        assert(characterID(displayFor(slot, true).CharacterVM) == id, "Required squad display is out of sync")
        assert(not reserved[id], "Required slots contain a duplicate character")
        reserved[id] = true
    end

    local candidates, candidateIDs = {}, {}
    for _, character in ipairs(arrayValues(roster.RosterCharacterViewModels)) do
        if character:IsAvailable() and not character:IsDead()
            and not character:IsDismissed() and not character:IsCriticallyInjured() then
            local id = characterID(character)
            if not reserved[id] and not candidates[id] then
                candidates[id] = character
                candidateIDs[#candidateIDs + 1] = id
            end
        end
    end

    local eligible, previous, previousIDs = {}, {}, {}
    local displays, previousDisplay, previousDisplayIDs = {}, {}, {}
    for i, slot in ipairs(flexible) do
        displays[i] = displayFor(slot, false)
        local displayed = displays[i].CharacterVM
        previousDisplay[i] = valid(displayed) and displayed or false
        previousDisplayIDs[i] = characterID(displayed) or false
        local current = slot.AssignedCharacterVM
        previous[i] = valid(current) and current or false
        previousIDs[i] = characterID(current) or false
        eligible[i] = {}
        for _, id in ipairs(candidateIDs) do
            if slot:DoesCharacterSatisfyRequirement(candidates[id]) then
                eligible[i][#eligible[i] + 1] = id
            end
        end
    end

    local selected = plan(eligible)
    if not selected then
        log("Not enough eligible recruits to fill every flexible slot; squad unchanged.")
        return
    end

    -- Use the same SetCharacter notification as the screen's AssignUnit event,
    -- without its navigation side effects. Refresh holograms once after the batch.
    local function clear()
        for _, slot in ipairs(flexible) do slot:ClearSlot() end
    end
    local ok, err = pcall(function()
        clear()
        for i, slot in ipairs(flexible) do
            slot:AssignCharacterToSlot(candidates[selected[i]])
        end
        for i, slot in ipairs(flexible) do
            assert(characterID(slot.AssignedCharacterVM) == selected[i], "Game rejected a squad assignment")
        end
        for i, display in ipairs(displays) do
            display:SetCharacter(candidates[selected[i]])
        end
        for i, display in ipairs(displays) do
            assert(characterID(display.CharacterVM) == selected[i], "Squad display rejected an assignment")
        end
        screen:UpdateCharacterHologram(false)
    end)
    if not ok then
        local restored, restoreError = pcall(function()
            clear()
            for i, slot in ipairs(flexible) do
                if previous[i] then slot:AssignCharacterToSlot(previous[i]) end
            end
            for i, slot in ipairs(flexible) do
                assert((characterID(slot.AssignedCharacterVM) or false) == previousIDs[i], "Original squad could not be restored")
            end
            for i, display in ipairs(displays) do
                display:SetCharacter(previousDisplay[i] or nil)
            end
            for i, display in ipairs(displays) do
                assert((characterID(display.CharacterVM) or false) == previousDisplayIDs[i], "Original squad display could not be restored")
            end
            screen:UpdateCharacterHologram(false)
        end)
        if not restored then
            error("Assignment failed AND restore failed. Manually reselect your squad before launching. " .. tostring(restoreError))
        end
        error("Assignment failed; original squad restored. " .. tostring(err))
    end
    log("Randomized " .. #flexible .. " flexible slots; kept " .. #required .. " required slots. Pending lineup verified; hologram refresh requested. Review before launching.")
end

local function requestRandomize()
    if pending then return end
    pending = true
    local queued, queueError = pcall(ExecuteInGameThread, function()
        local ok, err = pcall(randomize)
        pending = false
        if not ok then log(tostring(err)) end
    end)
    if not queued then
        pending = false
        log("Could not schedule randomization: " .. tostring(queueError))
    end
end

RegisterKeyBind(Key.F8, requestRandomize)

-- Reuse the game's styled button, without rebinding any existing game action.
-- The panel owns the widget; Lua references are checked again after screen teardown.
local button, buttonScreen
local function fullLineup(screen)
    if not valid(screen) or not screen.isBuildOnComplete then return false end
    local library = requireObject(StaticFindObject("/Script/MDViewModel.Default__MDViewModelFunctionLibrary"), "View-model library")
    local ui = binding(library, screen, "/Game/Game/UI/Strategy/SquadSelect/BPs/VM_SquadSelect.VM_SquadSelect_C")
    return ui.State.TagName:ToString():lower()
        == "br.ui.menu.strategy.holotable.region.location.tacticalmission.missionbriefing.squadselect.confirmation"
end

local function updateButton()
    local screen = activeScreen()
    if not screen or not fullLineup(screen) then
        if valid(button) then button:SetVisibility(1) end -- Collapsed
        return
    end
    if valid(button) and valid(buttonScreen) and buttonScreen:GetAddress() == screen:GetAddress() then
        button:SetVisibility(0) -- Visible, including hit testing
        return
    end
    if valid(button) then button:RemoveFromParent() end
    buttonScreen = screen
    local template = requireObject(screen.EditSquadButton, "Edit Squad button")
    local panel = requireObject(template:GetParent(), "Squad controls panel")
    local widgets = requireObject(StaticFindObject("/Script/UMG.Default__WidgetBlueprintLibrary"), "Widget library")
    local class = requireObject(StaticFindObject("/Game/Game/UI/Common/Buttons/WBP_BoundActionButton.WBP_BoundActionButton_C"), "Button class")
    button = requireObject(widgets:Create(screen, class, screen:GetOwningPlayer()), "Randomize button")
    button:SetVisibility(1)
    button.bSetButtonTextToInputActionName = false
    button:SetShouldUseFallbackDefaultInputAction(false)
    button:SetHideInputAction(true)
    button:SetRequiresHold(false)
    local slot = requireObject(panel:AddChildToHorizontalBox(button), "Randomize button slot")
    slot:SetPadding({Left = 16, Top = 0, Right = 0, Bottom = 0})
    slot:SetVerticalAlignment(2) -- Center
    button:UpdateButtonStyle(template.ButtonType, template.ButtonOrientation, template.ButtonSize)
    button:SetButtonText(FText("Randomize"))
    button:SetToolTipText(FText("Randomize available squad members (F8). Required members stay selected."))
    button:SetVisibility(0)
    log("Added Randomize button to the mission lineup.")
end

local installed, installError = pcall(function()
    RegisterHook("/Script/CommonUI.CommonButtonBase:HandleButtonClicked", function(context)
        local clicked = context:get()
        if not valid(button) or not valid(clicked) or clicked:GetAddress() ~= button:GetAddress() then return end
        local ok, err = pcall(function()
            local screen = activeScreen()
            if screen and valid(buttonScreen) and screen:GetAddress() == buttonScreen:GetAddress()
                and fullLineup(screen) and button:IsVisible() and button:GetIsEnabled() then
                requestRandomize()
            end
        end)
        if not ok then log("Randomize button: " .. tostring(err)) end
    end)
    local timer
    timer = LoopInGameThreadWithDelay(250, function()
        local ok, err = pcall(updateButton)
        if not ok then
            if valid(button) then button:RemoveFromParent() end
            button = nil
            CancelDelayedAction(timer)
            log("Button unavailable; F8 still works. " .. tostring(err))
        end
    end)
end)
if not installed then log("Button unavailable; F8 still works. " .. tostring(installError)) end

log("Loaded prototype with Randomize button and F8 shortcut. Review the lineup before launching.")
