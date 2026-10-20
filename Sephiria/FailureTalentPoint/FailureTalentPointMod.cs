using System;
using UnityEngine;

namespace SephiriaMods.FailureTalentPoint
{
    /// <summary>
    /// Entry point loaded by Sephiria's built-in AddOnLoader.
    /// </summary>
    public sealed class FailureTalentPointMod : HorayModBase
    {
        private GameObject controllerObject;

        protected override void OnModLoaded()
        {
            controllerObject = new GameObject("FailureTalentPointController");
            controllerObject.hideFlags = HideFlags.HideAndDontSave;
            UnityEngine.Object.DontDestroyOnLoad(controllerObject);
            controllerObject.AddComponent<FailureTalentPointController>();

            Debug.Log("[FailureTalentPoint] Loaded. Normal solo defeats grant one permanent Talent point.");
        }

        protected override void OnModUnloaded()
        {
            if (controllerObject != null)
            {
                UnityEngine.Object.Destroy(controllerObject);
                controllerObject = null;
            }
        }
    }

    /// <summary>
    /// Tracks the local solo player, awards points at game over, and reapplies the
    /// saved bonus whenever a new network player object is created.
    /// </summary>
    public sealed class FailureTalentPointController : MonoBehaviour
    {
        private const int NormalDefeatResult = 0;
        private const string BonusKey = "FailureTalentPoint_Bonus";
        private const string LastAwardedRetryKey = "FailureTalentPoint_LastAwardedRetry";

        private PlayerSpawner localSpawner;
        private PlayerAvatar appliedAvatar;
        private int appliedBonus;
        private bool handledCurrentGameOver;
        private bool warnedAboutMultiplayer;

        private void Update()
        {
            PlayerSpawner nextSpawner = FindLocalSpawner();
            if (nextSpawner != localSpawner)
            {
                Connect(nextSpawner);
            }

            ApplySavedBonus();
        }

        private void OnDestroy()
        {
            Connect(null);
        }

        private static PlayerSpawner FindLocalSpawner()
        {
            if (PlayerSpawner.MultiplayerList == null)
            {
                return null;
            }

            foreach (PlayerSpawner spawner in PlayerSpawner.MultiplayerList)
            {
                if (spawner != null && spawner.isLocalPlayer)
                {
                    return spawner;
                }
            }

            return null;
        }

        private void Connect(PlayerSpawner nextSpawner)
        {
            if (localSpawner != null)
            {
                localSpawner.OnGameOverServerside -= HandleGameOver;
                localSpawner.OnGameRestartedClientSide -= HandleGameRestarted;
            }

            localSpawner = nextSpawner;
            appliedAvatar = null;
            appliedBonus = 0;
            handledCurrentGameOver = false;
            warnedAboutMultiplayer = false;

            if (localSpawner != null)
            {
                localSpawner.OnGameOverServerside += HandleGameOver;
                localSpawner.OnGameRestartedClientSide += HandleGameRestarted;
            }
        }

        private void HandleGameRestarted()
        {
            handledCurrentGameOver = false;
        }

        private void HandleGameOver()
        {
            if (handledCurrentGameOver || !IsSupportedSoloSession())
            {
                return;
            }

            DungeonManager dungeon = DungeonManager.Instance;
            if (dungeon == null || dungeon.victoryType != NormalDefeatResult)
            {
                // 0 is a normal defeat. In particular, do not count story "fake
                // failures" (2) or any of the clear result types (1 and 3+).
                return;
            }

            SaveData save = SaveManager.Current;
            if (save == null)
            {
                Debug.LogError("[FailureTalentPoint] Cannot award a point because no profile save is loaded.");
                return;
            }

            handledCurrentGameOver = true;

            int currentRetry = save.GetInt("DeathCount", 0);
            if (localSpawner.LocalDataStorage != null)
            {
                currentRetry = Math.Max(currentRetry, localSpawner.LocalDataStorage.deathCount);
            }

            int awardedRetry = currentRetry + 1;
            int lastAwardedRetry = save.GetInt(LastAwardedRetryKey, -1);
            if (lastAwardedRetry == awardedRetry)
            {
                Debug.LogWarning(
                    "[FailureTalentPoint] Duplicate game-over notification ignored for retry "
                    + awardedRetry
                    + ".");
                return;
            }

            int bonus = Math.Max(0, save.GetInt(BonusKey, 0)) + 1;
            save.SetInt(BonusKey, bonus);
            save.SetInt(LastAwardedRetryKey, awardedRetry);

            Debug.Log(
                "[FailureTalentPoint] Failed run "
                + awardedRetry
                + " awarded +1 Talent point. Total mod bonus: "
                + bonus
                + ".");

            // UI_GameOverLabel writes and saves the rest of the run settlement
            // immediately after this event. We intentionally update the same
            // in-memory SaveData and let that normal save operation persist it.
            ApplySavedBonus();
        }

        private void ApplySavedBonus()
        {
            if (!IsSupportedSoloSession() || SaveManager.Current == null)
            {
                return;
            }

            PlayerAvatar avatar = localSpawner.PlayerAvatar;
            if (avatar == null)
            {
                return;
            }

            if (appliedAvatar != avatar)
            {
                appliedAvatar = avatar;
                appliedBonus = 0;
            }

            int savedBonus = Math.Max(0, SaveManager.Current.GetInt(BonusKey, 0));
            int delta = savedBonus - appliedBonus;
            if (delta == 0)
            {
                return;
            }

            avatar.NetworkmaxPassivePoint = avatar.maxPassivePoint + delta;
            appliedBonus = savedBonus;

            Debug.Log(
                "[FailureTalentPoint] Applied "
                + savedBonus
                + " saved bonus Talent point(s). Current maximum: "
                + avatar.maxPassivePoint
                + ".");
        }

        private bool IsSupportedSoloSession()
        {
            if (localSpawner == null || !localSpawner.isServer)
            {
                return false;
            }

            int activePlayers = 0;
            if (PlayerSpawner.MultiplayerList != null)
            {
                foreach (PlayerSpawner spawner in PlayerSpawner.MultiplayerList)
                {
                    if (spawner != null)
                    {
                        activePlayers++;
                    }
                }
            }

            if (activePlayers == 1)
            {
                return true;
            }

            if (activePlayers > 1 && !warnedAboutMultiplayer)
            {
                warnedAboutMultiplayer = true;
                Debug.LogWarning(
                    "[FailureTalentPoint] Multiplayer detected. Awards are disabled to avoid save and network desyncs.");
            }

            return false;
        }
    }
}
