namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit { }
}

namespace RepoLastStandMod.Utility
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    internal record ProbabilityItem<T>(T Item, double Probability);

    internal static class ProbabilitySelector
    {
        private static readonly System.Random _random = new();

        public static T SelectRandomItem<T>(IEnumerable<ProbabilityItem<T>> items)
        {
            if (items is null || !items.Any())
            {
                return default;
            }

            // Validate probabilities
            double totalProbability = items.Sum(i => i.Probability);
            if (totalProbability <= 0)
            {
                return default;
            }

            // Generate a random value between 0 and total probability
            double randomValue = _random.NextDouble() * totalProbability;

            // Accumulate probabilities to select the item
            double cumulativeProbability = 0;
            foreach (var item in items)
            {
                cumulativeProbability += item.Probability;
                if (randomValue <= cumulativeProbability)
                {
                    return item.Item;
                }
            }

            // Fallback (should never happen, but included for completeness)
            return items.Last().Item;
        }
    }

    internal static class Extensions
    {
        public static int GetInternalHaulGoal(this RoundDirector roundDirector)
        {
            var haulGoalFieldInfo = typeof(RoundDirector).GetField("haulGoal", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)haulGoalFieldInfo.GetValue(roundDirector);
        }

        public static int GetInternalExtractionPoints(this RoundDirector roundDirector)
        {
            var extractionPointsFieldInfo = typeof(RoundDirector).GetField("extractionPoints", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)extractionPointsFieldInfo.GetValue(roundDirector);
        }

        public static int GetInternalExtractionPointsCompleted(this RoundDirector roundDirector)
        {
            var extractionPointsCompletedFieldInfo = typeof(RoundDirector).GetField("extractionPointsCompleted", BindingFlags.Instance | BindingFlags.NonPublic);
            return (int)extractionPointsCompletedFieldInfo.GetValue(roundDirector);
        }
    }
}

namespace RepoLastStandMod
{
    using BepInEx;
    using BepInEx.Logging;
    using global::RepoLastStandMod.Utility;
    using HarmonyLib;
    using System.Collections.Generic;
    using UnityEngine;

    internal record Prefab(string Name, string Path)
    {
        internal GameObject GameObject => Resources.Load<GameObject>(Path);
    }

    internal class StateManager
    {
        internal static StateManager Instance { get; } = new StateManager();

        internal bool LastStandActive;
        internal IEnumerable<ProbabilityItem<Prefab>> PityWeapons;
    }

    [BepInPlugin(pluginGuid, pluginName, pluginVersion)]
    public class RepoLastStandMod : BaseUnityPlugin
    {
        private const string pluginGuid = "umbreon222.repo.laststand";
        private const string pluginName = "Repo Last Stand Mod";
        private const string pluginVersion = "1.0.0";

        internal static ManualLogSource LoggerInstance;
        private readonly Harmony _harmony = new(pluginGuid);

        public void Awake()
        {
            LoggerInstance = Logger;

            InitializeState();

            _harmony.PatchAll();
            LoggerInstance.LogInfo("Repo Last Stand Mod has loaded!");
        }

        private void InitializeState()
        {
            StateManager.Instance.LastStandActive = false;
            StateManager.Instance.PityWeapons = [.. BuildPityWeapons()]; // Iterate to ensure config generation
        }

        private IEnumerable<ProbabilityItem<Prefab>> BuildPityWeapons()
        {
            // Pistol
            var pistolProbability = Config.Bind("General", "HandgunProbability", 0.15f, "Probability between 0 and 1 of spawning a handgun");
            yield return new ProbabilityItem<Prefab>(new("Handgun", "items/Item Gun Handgun"), pistolProbability.Value);

            // Tranquilizer
            var tranquilizerProbability = Config.Bind("General", "TranqGunProbability", 0.3f, "Probability between 0 and 1 of spawning a tranq gun");
            yield return new ProbabilityItem<Prefab>(new("Tranq Gun", "items/Item Gun Tranq"), tranquilizerProbability.Value);

            // Duct taped grenade
            var ductTapedGrenadeProbability = Config.Bind("General", "DuctTapedGrenadeProbability", 0.4f, "Probability between 0 and 1 of spawning a duct taped grenade");
            yield return new ProbabilityItem<Prefab>(new("Duct Taped Grenade", "items/Item Grenade Duct Taped"), ductTapedGrenadeProbability.Value);

            // Grenade
            var grenadeProbability = Config.Bind("General", "GrenadeProbability", 0.5f, "Probability between 0 and 1 of spawning a grenade");
            yield return new ProbabilityItem<Prefab>(new("Grenade", "items/Item Grenade Explosive"), grenadeProbability.Value);

            // Shotgun
            var shotgunProbability = Config.Bind("General", "ShotgunProbability", 0.05f, "Probability between 0 and 1 of spawning a shotgun");
            yield return new ProbabilityItem<Prefab>(new("Shotgun", "items/Item Gun Shotgun"), shotgunProbability.Value);

            // Baseball bat
            var baseballBatProbability = Config.Bind("General", "BaseballBatProbability", 0.5f, "Probability between 0 and 1 of spawning a baseball bat");
            yield return new ProbabilityItem<Prefab>(new("Baseball Bat", "items/Item Melee Baseball Bat"), baseballBatProbability.Value);

            // Frying pan
            var fryingPanProbability = Config.Bind("General", "FryingPanProbability", 0.5f, "Probability between 0 and 1 of spawning a frying pan");
            yield return new ProbabilityItem<Prefab>(new("Frying Pan", "items/Item Melee Frying Pan"), fryingPanProbability.Value);

            // Inflatable hammer
            var inflatableHammerProbability = Config.Bind("General", "InflatableHammerProbability", 0.4f, "Probability between 0 and 1 of spawning an inflatable hammer");
            yield return new ProbabilityItem<Prefab>(new("Inflatable Hammer", "items/Item Melee Inflatable Hammer"), inflatableHammerProbability.Value);

            // Sledge hammer
            var sledgeHammerProbability = Config.Bind("General", "SledgeHammerProbability", 0.3f, "Probability between 0 and 1 of spawning a sledge hammer");
            yield return new ProbabilityItem<Prefab>(new("Sledge Hammer", "items/Item Melee Sledge Hammer"), sledgeHammerProbability.Value);

            // Sword
            var swordProbability = Config.Bind("General", "SwordProbability", 0.3f, "Probability between 0 and 1 of spawning a sword");
            yield return new ProbabilityItem<Prefab>(new("Sword", "items/Item Melee Sword"), swordProbability.Value);

            // Explosive mine
            var explosiveMineProbability = Config.Bind("General", "ExplosiveMineProbability", 0.5f, "Probability between 0 and 1 of spawning an explosive mine");
            yield return new ProbabilityItem<Prefab>(new("Mine", "items/Item Mine Explosive"), explosiveMineProbability.Value);

            // Rubber duck
            var rubberDuckProbability = Config.Bind("General", "RubberDuckProbability", 0.01f, "Probability between 0 and 1 of spawning a rubber duck");
            yield return new ProbabilityItem<Prefab>(new("Rubber Duck", "items/Item Rubber Duck"), rubberDuckProbability.Value);

            // Clown
            var clownBombProbability = Config.Bind("General", "ValuableClownProbability", 0.1f, "Probability between 0 and 1 of spawning a valuable clown");
            yield return new ProbabilityItem<Prefab>(new("Valuable Clown", "valuables/03 medium/Valuable Clown"), clownBombProbability.Value);
        }
    }
}

namespace RepoLastStandMod.Patches
{
    using global::RepoLastStandMod.Utility;
    using HarmonyLib;
    using Photon.Pun;
    using System.Linq;
    using UnityEngine;

    [HarmonyPatch(typeof(PhysGrabObjectImpactDetector))]
    public static class PhysGrabObjectImpactDetectorPatches
    {
        [HarmonyPatch("DestroyObjectRPC")]
        [HarmonyPrefix]
        public static void DestroyObjectRPCPrefix(bool effects, ValuableObject ___valuableObject)
        {
            if (SemiFunc.RunIsLevel() && ___valuableObject is not null)
            {
                OnValuableObjectDestroyed(___valuableObject);
            }
        }

        private static void OnValuableObjectDestroyed(ValuableObject valuableObject)
        {
            var roundHaulGoal = RoundDirector.instance.GetInternalHaulGoal();
            // RepoLastStandMod.LoggerInstance.LogInfo($"Round haul goal: {roundHaulGoal}");

            var extractionHaulGoal = roundHaulGoal / RoundDirector.instance.GetInternalExtractionPoints();
            // RepoLastStandMod.LoggerInstance.LogInfo($"Extraction haul goal: {extractionHaulGoal}");

            var extractedValuablesValue = RoundDirector.instance.GetInternalExtractionPointsCompleted() * extractionHaulGoal;
            // RepoLastStandMod.LoggerInstance.LogInfo($"Extracted valuables value: {extractedValuablesValue}");

            var currentLevelValuablesValue = GetLevelValuablesValue();
            // RepoLastStandMod.LoggerInstance.LogInfo($"Current level valuables value: {currentLevelValuablesValue}");

            var levelValuablesValueAfterValuableDestroyed = currentLevelValuablesValue - valuableObject.dollarValueCurrent;
            // RepoLastStandMod.LoggerInstance.LogInfo($"Level valuables value after valuable destroyed: {levelValuablesValueAfterValuableDestroyed}");

            var adjustedLevelValuablesValueAfterValuableDestroyed = levelValuablesValueAfterValuableDestroyed + extractedValuablesValue;
            // RepoLastStandMod.LoggerInstance.LogInfo($"Adjusted level valuables value after valuable destroyed: {adjustedLevelValuablesValueAfterValuableDestroyed}");

            var canStillExtract = adjustedLevelValuablesValueAfterValuableDestroyed >= roundHaulGoal;
            // RepoLastStandMod.LoggerInstance.LogInfo($"Can still extract: {canStillExtract}");

            if (canStillExtract || StateManager.Instance.LastStandActive)
            {
                RepoLastStandMod.LoggerInstance.LogDebug($"Round is still completable or last stand is already active");
                return;
            }

            StateManager.Instance.LastStandActive = true;
            RepoLastStandMod.LoggerInstance.LogInfo($"Last stand activated!");

            // Would be cool to trigger lights off as well or atleast play a sound effect of some sort
            SemiFunc.UIBigMessage("LAST STAND ACTIVATED", "{!}", 25f, Color.red, Color.red);
            SemiFunc.UIFocusText("Not enough loot to complete the level! Take your last stand!", Color.red, Color.red, 3f);

            var randomPityWeapon = ProbabilitySelector.SelectRandomItem(StateManager.Instance.PityWeapons);
            if (randomPityWeapon is null)
            {
                RepoLastStandMod.LoggerInstance.LogError("Couldn't randomly select a pity weapon. Are probabilities all 0?");
                return;
            }

            RepoLastStandMod.LoggerInstance.LogInfo($"Rolled a \"{randomPityWeapon.Name}\" as a pity weapon; Spawning...");
            if (!SemiFunc.IsMasterClientOrSingleplayer())
            {
                RepoLastStandMod.LoggerInstance.LogError($"Failed to spawn \"{randomPityWeapon.Name}\". You are not the host.");
                return;
            }

            var positionInfrontOfPlayer = PlayerController.instance.transform.position + PlayerController.instance.transform.up + PlayerController.instance.transform.forward;
            // RepoLastStandMod.LoggerInstance.LogInfo($"Spawning pity weapon at position: {positionInfrontOfPlayer}");

            if (SemiFunc.IsMultiplayer())
            {
                PhotonNetwork.InstantiateRoomObject(randomPityWeapon.Path, positionInfrontOfPlayer, PlayerController.instance.transform.rotation, 0, null);
            }
            else
            {
                Object.Instantiate(randomPityWeapon.GameObject, positionInfrontOfPlayer, PlayerController.instance.transform.rotation);
            }

            GameDirector.instance.CameraImpact.ShakeDistance(3f, 3f, 8f, positionInfrontOfPlayer, 0.1f);
            GameDirector.instance.CameraShake.ShakeDistance(3f, 3f, 8f, positionInfrontOfPlayer, 0.1f);
        }

        private static float GetLevelValuablesValue()
        {
            return Object.FindObjectsOfType<ValuableObject>().Aggregate(0f, (sum, valuable) => sum + valuable.dollarValueCurrent);
        }
    }

    [HarmonyPatch(typeof(RoundDirector))]
    public static class RoundDirectorPatches
    {
        [HarmonyPatch("StartRoundLogic")]
        [HarmonyPrefix]
        public static void StartRoundLogicPrefix(int value)
        {
            StateManager.Instance.LastStandActive = false;
        }

        [HarmonyPatch("ExtractionCompletedAllRPC")]
        [HarmonyPrefix]
        public static void ExtractionCompletedAllRPCPrefix()
        {
            StateManager.Instance.LastStandActive = false;
        }
    }
}
