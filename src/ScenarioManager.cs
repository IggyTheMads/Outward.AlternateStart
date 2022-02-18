using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SideLoader;
using HarmonyLib;
using UnityEngine.UI;
using System.Collections;
using SideLoader.Managers;
using AlternateStart.StartScenarios;
using UnityEngine.SceneManagement;
using System.IO;

namespace AlternateStart
{
    public static class ScenarioManager
    {
        // We will fill this dictionary on Init()
        static readonly Dictionary<Scenarios, Scenario> startScenarios = new();

        // Our quest event to check if we already started a scenario.
        internal static QuestEventSignature QE_DestinyChosen;
        const string QE_DESTINY_CHOSEN_UID = "iggythemad.altstart.destinyChosen";

        const string FULL_STOP_STATUS_IDENTIFIER = "fullstop";

        internal static void Init()
        {
            QE_DestinyChosen = CustomQuests.CreateQuestEvent(QE_DESTINY_CHOSEN_UID, false, false, true, Plugin.QUEST_EVENT_FAMILY_NAME);

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;

            // Get all types in this dll
            foreach (Type type in typeof(Scenario).Assembly.GetTypes())
            {
                // If the type is subclass of Scenario...
                if (type.IsSubclassOf(typeof(Scenario)))
                {
                    // Make an instance of the class, like "new Scenario();"
                    var scenario = Activator.CreateInstance(type) as Scenario;
                    scenario.Init();
                    // Add it to our dictionary
                    startScenarios.Add(scenario.Type, scenario);
                }
            }
        }

        // Patch on the method that loads the first tutorial level.
        // Swap it to DreamWorld if its a new save.
        [HarmonyPatch(typeof(NetworkLevelLoader), "LoadLevel", new Type[] { typeof(int), typeof(int), typeof(float), typeof(bool) })]
        public class NetworkLevelLoader_LoadLevel
        {
            internal static void Prefix(ref int _buildIndex)
            {
                try
                {
                    CharacterSave chosenSave = SplitScreenManager.Instance.LocalPlayers[0].ChosenSave;
                    if (chosenSave.PSave.NewSave || string.IsNullOrEmpty(chosenSave.PSave.AreaName))
                    {
                        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
                        {
                            var scene = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
                            if (scene == default)
                                continue;
                            if (scene == "DreamWorld")
                            {
                                _buildIndex = i;
                                break;
                            }
                        }
                    }
                }
                catch //(Exception ex)
                {
                    //Plugin.LogWarning($"Exception on load level patch: {ex}");
                }
            }
        }

        private static void SL_OnGameplayResumedAfterLoading()
        {
            if (SceneManagerHelper.ActiveSceneName == "DreamWorld" && !QuestEventManager.Instance.HasQuestEvent(QE_DestinyChosen))
            {
                SetFullStop(true);
                foreach (var uid in CharacterManager.Instance.PlayerCharacters.Values)
                {
                    var character = CharacterManager.Instance.GetCharacter(uid);
                    character.Inventory.RemoveMoney(27, true);
                }
            }
        }

        // Helper to add or remove the FullStop status on the players.
        internal static void SetFullStop(bool add)
        {
            // Only the host needs to add and remove the statuses.
            if (PhotonNetwork.isNonMasterClientInRoom)
                return;

            foreach (var uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                var character = CharacterManager.Instance.GetCharacter(uid);

                if (add)
                    character.StatusEffectMngr.AddStatusEffect(FULL_STOP_STATUS_IDENTIFIER);
                else
                    character.StatusEffectMngr.RemoveStatusWithIdentifierName(FULL_STOP_STATUS_IDENTIFIER);
            }
        }

        // ------------- Picking and starting a scenario ------------------

        [HarmonyPatch(typeof(CharacterSkillKnowledge), "AddItem", new Type[] { typeof(Item) })]
        static class CharacterSkillKnowledge_AddItem
        {
            internal static void Postfix(Item _item)
            {
                if (QuestEventManager.Instance.HasQuestEvent(QE_DESTINY_CHOSEN_UID))
                    return;

                // Make sure we are in DreamWorld, and we are the host.
                if (SceneManagerHelper.ActiveSceneName != "DreamWorld" || PhotonNetwork.isNonMasterClientInRoom)
                    return;

                // TODO add logic to remove passives if ones are already chosen!!!

                Plugin.Instance.StartCoroutine(CheckStartPassives(_item.OwnerCharacter));
            }
        }

        // Called when we acquire a passive
        static IEnumerator CheckStartPassives(Character character)
        {
            yield return new WaitForSeconds(0.2f);

            // The player acquired a passive. Let's just check if both a difficulty and area are chosen...
            // (or its the test)
            if (character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioAreas.Test)
                || (IsAnyChosen<ScenarioDifficulty>(character) && IsAnyChosen<ScenarioAreas>(character)))
            {
                // We are ready to pick and start our scenario.
                Plugin.Instance.StartCoroutine(PickAndStartScenario());
            }
        }

        // Just a helper to call TryGetChoice without actually getting the choice.
        // We just want to know if any choice was chosen.
        static bool IsAnyChosen<T>(Character character) where T : Enum
        {
            return TryGetChoice<T>(character, out _);
        }

        // Checks the player's skill knowledge for all values in the provided enum.
        // If the player has a skill that matches a value in the enum, return true and that value.
        static bool TryGetChoice<T>(Character character, out T choice) where T : Enum
        {
            // Iterate over the values in the enum
            foreach (object enumValue in Enum.GetValues(typeof(T)))
            {
                // The enum value is a skill item id. Check if the player has that skill.
                if (character.Inventory.SkillKnowledge.IsItemLearned((int)enumValue))
                {
                    // Set the choice to the enum value and return true
                    choice = (T)enumValue;
                    return true;
                }
            }

            // The player doesn't have any of the enum skills learned.
            choice = default;
            return false;
        }

        // Our method to actually begin the chosen scenario.
        // This is only called when both choices are definitely set.
        static IEnumerator PickAndStartScenario()
        {
            var host = CharacterManager.Instance.GetWorldHostCharacter();

            // Check for test scenario
            // Change this to test a specific scenario
            if (host.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioAreas.Test))
            {
                Plugin.LogWarning($"~~~ Starting test scenario ~~~");
                Plugin.Instance.StartCoroutine(startScenarios[Scenarios.SandBandit].StartScenario());  // Change this to TEST a specific scenario
                yield break;
            }

            // Get our choices
            TryGetChoice(host, out ScenarioDifficulty difficultyChoice);
            TryGetChoice(host, out ScenarioAreas areaChoice);

            // Determine eligable Scenarios based on choices.
            var eligable = new List<Scenario>();
            foreach (Scenario entry in startScenarios.Values)
            {
                // If the Scenario's difficulty doesn't match our choice, skip.
                if (difficultyChoice != ScenarioDifficulty.ANY && entry.Difficulty != difficultyChoice)
                    continue;

                // If the Scenario's area doesn't match our choice, skip.
                if (areaChoice != ScenarioAreas.ANY && entry.Area != areaChoice)
                    continue;

                // It's eligable, add it.
                eligable.Add(entry);
            }

            // Make sure there is at least ONE available scenario for our choice combination!
            if (!eligable.Any())
            {
                CharacterManager.Instance
                    .GetWorldHostCharacter()
                    .CharacterUI
                    .ShowInfoNotification($"Sorry, there are no {difficultyChoice} {areaChoice} scenarios!");

                yield break; // Don't start the scenario!
            }

            // for debug
            Plugin.Log($"Eligable scenarios: {string.Join(",", eligable.Select(it => it.GetType().Name))}");

            // Pick a random scenario
            Scenario scenario = eligable[UnityEngine.Random.Range(0, eligable.Count)];
            Plugin.Log($"Chosen scenario: {scenario.GetType().Name}");

            // Start it!
            Plugin.Instance.StartCoroutine(scenario.StartScenario());
        }


        #region Iggy's Tests

        //// Block zone switch

        //[HarmonyPatch(typeof(CharacterManager), "RequestAreaSwitch")]
        //public class CharacterManager_RequestAreaSwitch
        //{
        //    [HarmonyPrefix]
        //    public static bool Prefix(CharacterManager __instance, Character _character, Area _areaToSwitchTo, int _longTravelTime, int _spawnPoint, float _offset, string _overrideLocKey)
        //    {
        //        //Debug.Log("HEREEEEEEEEEEEEEEEEEEEEEEE----------------------------------");
        //        if (_areaToSwitchTo.SceneName == "Berg")
        //        {
        //            Instance.players[0].CharacterUI.ShowInfoNotification("You are not welcome here");
        //            return false;
        //        }
        //        else { return true; }
        //        //return false;
        //    }
        //}

        //// Faction swapping test

        //public const int sparkID = 8200040;
        //static Character.Factions factionTesting = Character.Factions.NONE;

        //static void TESTO()
        //{
        //    factionTesting += 1;
        //    if (factionTesting == Character.Factions.COUNT)
        //        factionTesting = Character.Factions.Player;
        //
        //    var character = CharacterManager.Instance.GetFirstLocalCharacter();
        //    character.ChangeFaction(factionTesting);
        //    character.CharacterUI.ShowInfoNotification($"YOU ARE NOW A {factionTesting}");
        //}

        //[HarmonyPatch(typeof(Item), "Use", new Type[] { typeof(Character) })]
        //public class Item_Usage
        //{
        //    [HarmonyPrefix]
        //    public static void Postfix(Item __instance, Character _character)
        //    {
        //        if (__instance.ItemID == 8100072)
        //            TESTO();
        //    }
        //}

        #endregion
    }
}
