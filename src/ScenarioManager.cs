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
        static readonly Dictionary<ScenarioPassives, Scenario> startScenarios = new();

        // Our quest event to check if we already started a scenario.
        internal static QuestEventSignature QE_DestinyChosen;
        const string QE_DESTINY_CHOSEN_UID = "iggythemad.altstart.destinyChosen";

        const string FULL_STOP_STATUS_IDENTIFIER = "fullstop";
        const string startTag = "StartTag";

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
                    Scenario scenario = Activator.CreateInstance(type) as Scenario;
                    scenario.Init();
                    // Add it to our dictionary
                    startScenarios.Add(scenario.Area, scenario);
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
                            string scene = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i));
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

                // Remove starting silver
                foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
                {
                    Character character = CharacterManager.Instance.GetCharacter(uid);
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

            foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                Character character = CharacterManager.Instance.GetCharacter(uid);

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
                //
                //
                //
                ////////////////////////////
                Character host = CharacterManager.Instance.GetWorldHostCharacter();
                if (host == _item.OwnerCharacter)
                {
                    Plugin.Instance.StartCoroutine(CheckStartPassives(_item.OwnerCharacter, _item));
                }
            }
        }

        // Called when we acquire a passive
        static IEnumerator CheckStartPassives(Character character, Item _item)
        {
            yield return new WaitForSeconds(0.2f);

            ///////////////////This part does not work
            /*List<Character> characterList = new List<Character>();
            while (characterList.Count < Global.Lobby.PlayersInLobby.Count)
            {
                yield return new WaitForSeconds(1f);
                foreach (PlayerSystem player in Global.Lobby.PlayersInLobby)
                {
                    if (player.ControlledCharacter.StatusEffectMngr.HasStatusEffect(startTag) && !characterList.Contains(player.ControlledCharacter))
                    {
                        Debug.Log("PlayersLobby: " + Global.Lobby.PlayersInLobby.Count);
                        Debug.Log("PlayerCount: " + characterList.Count);
                        characterList.Add(player.ControlledCharacter);
                    }
                }
            }*/
            /////////////////////

            //Bellow part works 

            // The player acquired a passive. If its random or it is a specific one
            if (character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Random))
            {
                // We are ready to pick and start our scenario.
                Plugin.Instance.StartCoroutine(PickAndStartScenario());
            }
            else if ((IsAnyChosen<ScenarioPassives>(character)))
            {
                int scenarioID = _item.ItemID;
                var scenario = (ScenarioPassives)scenarioID;
                //Plugin.Instance.StartCoroutine(scenario.StartScenario());
                Plugin.Instance.StartCoroutine(startScenarios[scenario].StartScenario());
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
            Character host = CharacterManager.Instance.GetWorldHostCharacter();
            var knows = host.Inventory.SkillKnowledge;
            if (host.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioDifficulty.VANILLA))
            {
                // They chose the Vanilla scenario.
                Plugin.Instance.StartCoroutine(startScenarios[ScenarioPassives.Vanilla].StartScenario());
                yield break;
            }

            // Get our choices
            //TryGetChoice(host, out ScenarioDifficulty difficultyChoice);
            TryGetChoice(host, out ScenarioPassives areaChoice);

            // Determine eligable Scenarios based on choices.
            List<Scenario> eligable = new();
            foreach (Scenario entry in startScenarios.Values)
            {
                if (entry.Area == ScenarioPassives.Random ||
                    entry.Area == ScenarioPassives.Vanilla ||
                    entry.Area == ScenarioPassives.VanillaAlt ||
                    entry.Area == ScenarioPassives.VanillaBerg ||
                    entry.Area == ScenarioPassives.VanillaLevant ||
                    entry.Area == ScenarioPassives.VanillaMonsoon ||
                    entry.Area == ScenarioPassives.VanillaHarmattan)
                {
                    // We don't want to randomly pick the Vanilla-like scenarios.
                    continue;
                }

                // If the Scenario's difficulty doesn't match our choice, skip.
                /*if (difficultyChoice != ScenarioDifficulty.ANY && entry.Difficulty != difficultyChoice)
                    continue;*/

                // If the Scenario's area doesn't match our choice, skip.
                /*if (areaChoice != ScenarioPassives.Random && entry.Area != areaChoice)
                    continue;*/

                // It's eligable, add it.
                eligable.Add(entry);
            }

            // Make sure there is at least ONE available scenario for our choice combination!
            if (!eligable.Any())
            {
                CharacterManager.Instance
                    .GetWorldHostCharacter()
                    .CharacterUI
                    .NotificationPanel
                    .ShowNotification($"Sorry, there are no {areaChoice} scenarios!");

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


        // FOR DEBUG

        internal static void OnGUI()
        {
            if (!NetworkLevelLoader.Instance.AllPlayerDoneLoading)
                return;

            if (SceneManagerHelper.ActiveSceneName == "DreamWorld"
                && !QuestEventManager.Instance.HasQuestEvent(QE_DestinyChosen))
            {
                GUILayout.BeginArea(new Rect(25, 25, 250, 25 * startScenarios.Count), GUI.skin.box);

                foreach (KeyValuePair<ScenarioPassives, Scenario> scenario in startScenarios)
                {
                    if (GUILayout.Button(scenario.Key.ToString()))
                    {
                        Plugin.Instance.StartCoroutine(scenario.Value.StartScenario());
                    }
                }

                GUILayout.EndArea();
            }
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

        //TESTING FACTION CHANGES
        /*static void TESTO()
        {
            
            var character = CharacterManager.Instance.GetFirstLocalCharacter();
            var factionTesting = character.Faction + 1;
            if (factionTesting == Character.Factions.COUNT)
                factionTesting = Character.Factions.Player;
        
            character.ChangeFaction(factionTesting);
            character.CharacterUI.ShowInfoNotification($"YOU ARE NOW A {factionTesting}");
        }

        [HarmonyPatch(typeof(Item), "Use", new Type[] { typeof(Character) })]
        public class Item_Usage
        {
            [HarmonyPrefix]
            public static void Postfix(Item __instance, Character _character)
            {
                if (__instance.ItemID == 8100072)
                    TESTO();
            }
        }*/

        #endregion
    }
}
