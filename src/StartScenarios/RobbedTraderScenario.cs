using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SideLoader;
using HarmonyLib;

namespace AlternateStart.StartScenarios
{
    public class RobbedTraderScenario : Scenario
    {
        internal static RobbedTraderScenario Instance { get; private set; }
        public override ScenarioQuest Type => ScenarioQuest.Quest_RobbedTrader;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioPassives Area => ScenarioPassives.RobbedTrader;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.CierzoOutside;
        public override Vector3 SpawnPosition => new(300.3f, 37.3f, 1423.9f);
        public override Vector3 SpawnRotation => new(0, 285.7f, 0);

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };


        //variables
        public string enemyID = "com.iggy.banditgeneric";
        public Vector3 traderPosition = new(1408.7f, 5.9f, 1663.8f);
        public Vector3 alchemistPosition = new(1384.9f, 7.8f, 1649.1f);
        public Vector3 chersoneseFailSpawn = new(181.5f, 32.6f, 1444.4f);
        public Vector3 cierzoEntrance = new(131.9f, 34.9f, 1461.7f);

        public override void Init()
        {
            base.Init();

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
        }

        public RobbedTraderScenario()
        {
            Instance = this;
        }
        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnScenarioChosen(Character character)
        {
            //character.Stats.IncreaseBurntHealth(200, 1);
            character.Inventory.ReceiveItemReward(9000010, 56, false); //bonus gold
            character.Inventory.ReceiveItemReward(3000190, 1, true); //chest scholar
            character.Inventory.ReceiveItemReward(3000004, 1, true); //legs trader
            character.Inventory.ReceiveItemReward(-2353, 1, true); //bag of goods
        }

        public override void OnStartSpawn()
        {
            
        }

        public override void OnStartSpawn(Character character)
        {
            ShowUIMessage("I can see the lighthouse! Got to sell the goods in Cierzo!");
            //character.StatusEffectMngr.AddStatusEffect("AdrenalineRush");
        }

        private void SL_OnGameplayResumedAfterLoading()
        {
            if (!Instance.IsActiveScenario || PhotonNetwork.isNonMasterClientInRoom)
                return;

            Character player = CharacterManager.Instance.GetWorldHostCharacter();
            if (SceneManagerHelper.ActiveSceneName == "ChersoneseNewTerrain" && player.Inventory.OwnsOrHasEquipped(-2353))
            {
                SL_Character myChar = SL.GetSLPack("iggythemad AlternateStart").CharacterTemplates[enemyID];
                int banditAmount = 2;
                for (int i = 0; i < banditAmount; i++)
                {
                    Vector3 enemyOffset = new(UnityEngine.Random.Range(30f, 40f), 4f, UnityEngine.Random.Range(20f, 30f));
                    Vector3 enemySpawn = SpawnPosition + enemyOffset;
                    Character enemyChar = myChar.Spawn(enemySpawn, (enemySpawn + SpawnPosition).normalized, UID.Generate());
                    //ToDo: randomize bandit weapons from list
                }
            }
            else if(SceneManagerHelper.ActiveSceneName == "ChersoneseNewTerrain") //need to fix for multiplayer
            {
                foreach (PlayerSystem players in Global.Lobby.PlayersInLobby)
                {
                    players.ControlledCharacter.Teleport(chersoneseFailSpawn, (chersoneseFailSpawn + cierzoEntrance).normalized);
                    players.ControlledCharacter.SpellCastProcess((int)Character.SpellCastType.GetUpBelly, (int)Character.SpellCastModifier.Immobilized, 0, 0);
                    Instance.ShowUIMessage("I lost the goods... I should speak to my fellow trader in Cierzo");
                }
                
            }
        }

        [HarmonyPatch(typeof(DefeatScenariosManager), "StartDefeat")]
        public class DefeatScenariosManager_StartDefeat
        {
            [HarmonyPrefix]
            public static bool Prefix(DefeatScenariosManager __instance)
            {
                if (!Instance.IsActiveScenario
                    || PhotonNetwork.isNonMasterClientInRoom)
                    return true;

                foreach(PlayerSystem player in Global.Lobby.PlayersInLobby)
                {
                    player.ControlledCharacter.Resurrect();
                }
                NetworkLevelLoader.Instance.RequestSwitchArea(AreaManager.Instance.GetArea(AreaManager.AreaEnum.CierzoOutside).SceneName, 0, 1.5f);
                return false;

            }
        }

        [HarmonyPatch(typeof(InteractionTriggerBase), "TryActivateBasicAction", new Type[] { typeof(Character), typeof(int) })]
        public class InteractionTriggerBase_TryActivateBasicAction
        {
            [HarmonyPrefix]
            public static bool Prefix(InteractionTriggerBase __instance, Character _character, int _toggleState)
            {
                if (!Instance.IsActiveScenario
                    || PhotonNetwork.isNonMasterClientInRoom
                    || !_character)
                    return true;
                
                if (__instance.CurrentTriggerManager as InteractionActivator == true)
                {
                    InteractionActivator activator = __instance.CurrentTriggerManager as InteractionActivator;
                    if (activator.BasicInteraction != null)
                    {
                        IInteraction interaction = activator.BasicInteraction;
                        if (SceneManagerHelper.ActiveSceneName == "CierzoNewTerrain")
                        {
                            if (interaction is InteractionMerchantDialogue && Vector3.Distance(_character.transform.position, Instance.traderPosition) < 5f)
                            {
                                if (_character.Inventory.OwnsOrHasEquipped(-2353))
                                { Instance.ShowUIMessage("I need to sell the goods before leaving."); }
                                else
                                {
                                    Instance.ShowUIMessage("Going back to Harmattan!");
                                    CharacterManager.Instance.GetWorldHostCharacter().Inventory.QuestKnowledge.ReceiveQuest(VanillaQuestsHelper.enrollmentQ);
                                    NetworkLevelLoader.Instance.RequestSwitchArea(AreaManager.Instance.GetArea(AreaManager.AreaEnum.Harmattan).SceneName, 0, 1.5f);
                                }
                                return false;
                            }
                            else if (interaction is InteractionMerchantDialogue && Vector3.Distance(_character.transform.position, Instance.alchemistPosition) < 5f)
                            {
                                //Instance.ShowUIMessage("Dont you have something else to do?");
                                return true;
                            }
                            else
                            {
                                if (_character.Inventory.OwnsOrHasEquipped(-2353))
                                { Instance.ShowUIMessage("I have no time for this..."); }
                                else { Instance.ShowUIMessage("I should go back to harmattan..."); }
                                return false;
                            }
                        }
                        else if (SceneManagerHelper.ActiveSceneName == "ChersoneseNewTerrain")
                        {
                            if(Vector3.Distance(_character.transform.position, Instance.cierzoEntrance) < 5f) { return true; }
                            else
                            {
                                Instance.ShowUIMessage("There is no time for this.");
                                return false;
                            }
                        }

                    }
                }
                return true;
            }
        }

        public override void UpdateQuestProgress(Quest quest)
        {

        }
    }
}
