using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;


namespace AlternateStart.StartScenarios
{
    public class GhostTrapScenario : Scenario
    {
        internal static GhostTrapScenario Instance { get; private set; }

        public override Scenarios Type => Scenarios.GhostTrapScenario;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioAreas Area => ScenarioAreas.Chersonese;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon3;
        public override Vector3 SpawnPosition => new(-29.4f, 0, -40.9f);
        public override Vector3 SpawnRotation => new(0, 182.0f, 0);

        public override bool HasQuest => true;
        public override string QuestName => "Ghost Quest";

        const string LogSignature_A = "ghostquest.objective.a";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "This is a log entry"
            }
        };

        //extras
        public int keyID = -2350;
        public Vector3 KeyPosition => new(10.2f, -53f, -70.2f);
        public Vector3 lockedLever => new(6.3f, 0f, -117f);
        public Vector3 lockedDoor => new(0.4f, 0f, -142f);
        public string ghostlyTimerID = "GhostlyTimer";

        public GhostTrapScenario()
        {
            Instance = this;
        }

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.StartHouseTimer();
        }

        public override void OnScenarioChosen(Character character)
        {
            //character.Stats.IncreaseBurntHealth(700, 1);
            //character.Inventory.ReceiveItemReward(3000133, 1, true); //beggarB head
            character.Inventory.ReceiveItemReward(3000130, 1, true); //beggarB chest
            character.Inventory.ReceiveItemReward(3000136, 1, true); //beggarB legs

            character.ChangeFaction(Character.Factions.Deer); //neutral ghosts
        }

        public override void OnStartSpawn()
        {
            Item ghostKey = ItemManager.Instance.GenerateItemNetwork(keyID);
            ghostKey.transform.position = KeyPosition;
            Item torch = ItemManager.Instance.GenerateItemNetwork(5100070); //cold torch
            torch.transform.position = SpawnPosition + new Vector3(0,1f,0);

        }

        public override void OnStartSpawn(Character character)
        {
            character.StatusEffectMngr.AddStatusEffect(ghostlyTimerID);
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            ////////////////////////
            ///
            /// ADD quest that tells you to find a way out. If finding a door, update to find the key
            ///     finding key and exiting completes the quest and you can keep the key (weapon). 
            ///     Dying (running out of time) throws you out, but without the special ghost weapon
            /// 
            ////////////////////////

        }

        [HarmonyPatch(typeof(InteractionTriggerBase), "TryActivateBasicAction", new Type[] { typeof(Character), typeof(int) })]
        public class InteractionTriggerBase_TryActivateBasicAction
        {
            [HarmonyPrefix]
            public static bool Prefix(InteractionTriggerBase __instance, Character _character, int _toggleState)
            {
                if (!Instance.IsActiveScenario 
                    || PhotonNetwork.isNonMasterClientInRoom 
                    || !_character
                    || SceneManagerHelper.ActiveSceneName != "Chersonese_Dungeon3") 
                    return true;

                if (__instance.CurrentTriggerManager as InteractionActivator == true)
                {
                    InteractionActivator activator = __instance.CurrentTriggerManager as InteractionActivator;
                    if (activator.BasicInteraction != null)
                    {
                        var interaction = activator.BasicInteraction;
                        if (interaction is InteractionOpenContainer)
                        {
                            return false;
                        }
                        else if (interaction is InteractionToggleContraption
                            && (Vector3.Distance(_character.CenterPosition, Instance.SpawnPosition) < 5f) || Vector3.Distance(_character.CenterPosition, Instance.lockedLever) < 5f) //need to stuck center lever too
                        {
                            _character.CharacterUI.ShowInfoNotification("The lever is stuck...");
                            return false;
                        }
                        else if (interaction is InteractionSwitchArea 
                            && (!_character.Inventory.OwnsOrHasEquipped(Instance.keyID) || Vector3.Distance(_character.CenterPosition, Instance.lockedDoor) < 8f))
                        {
                            //update quest to requiere key maybe
                            _character.CharacterUI.ShowInfoNotification("It is locked...");
                            return false;
                        }
                    }
                }
                return true;
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

                foreach (var player in Global.Lobby.PlayersInLobby)
                {
                    player.ControlledCharacter.Resurrect();
                }
                NetworkLevelLoader.Instance.RequestSwitchArea(AreaManager.Instance.GetArea(AreaManager.AreaEnum.CierzoOutside).SceneName, 0, 1.5f);
                return false;
            }
        }
    }
}
