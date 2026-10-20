using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(FellAndSell.AutoCollect), "Auto Collect Register Gold", "1.0.0", "Local Mod")]
[assembly: MelonGame("Art Games Studio SA", "Fell & Sell")]
[assembly: MelonGame("Art Games Studio SA", "Fell _ Sell")]

namespace FellAndSell;

public sealed class AutoCollect : MelonMod
{
    private MelonPreferences_Entry<bool> enabled;
    private float nextCheck;
    private bool faulted;

    public override void OnInitializeMelon()
    {
        enabled = MelonPreferences.CreateCategory("AutoCollectRegisterGold")
            .CreateEntry("Enabled", true, description: "Automatically collect completed register payments.");
        LoggerInstance.Msg("Ready. Collects register payments using the game's normal collection routine.");
    }

    public override void OnUpdate()
    {
        if (faulted || !enabled.Value)
            return;

        try
        {
            // Unity and the payment coroutine must run on the game's main thread.
            if (Time.timeScale <= 0 || Time.unscaledTime < nextCheck)
                return;
            nextCheck = Time.unscaledTime + 0.25f;

            var saves = SaveManager.Instance;
            if (saves != null && (saves.isRestoringFromSave || saves.isRestoringSceneState))
                return;

            // Re-read the singleton so scene changes never leave a cached, destroyed counter.
            var checkout = CheckoutQueue.Instance;
            if (checkout != null && checkout.isActiveAndEnabled && checkout.CanCollect())
                checkout.CollectCoins();
        }
        catch (Exception error)
        {
            // Stop for this session rather than repeat a failed payment operation every frame.
            faulted = true;
            LoggerInstance.Error($"Auto-collect disabled for this session: {error}");
        }
    }
}
