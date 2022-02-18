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
        public override ScenarioTheme Theme => ScenarioTheme.Freeform;
        public override ScenarioAreas Area => ScenarioAreas.Chersonese;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon3;
        public override Vector3 SpawnPosition => new(-29.4f, 0, -40.9f);
        public override Vector3 SpawnRotation => new(0, 182.0f, 0);

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        //extras
        public int keyID = -2350;
        public Vector3 KeyPosition => new(6.2f, -53f, -67.1f);
        public GhostTrapScenario()
        {
            Instance = this;
        }

        public override void PreScenarioBegin()
        {

        }
        public override void OnScenarioBegin()
        {
            VanillaQuestsHelper.StartHouseTimer();
        }

        public override void OnStartSpawn(Character character)
        {    
             //character.Stats.IncreaseBurntHealth(700, 1);
             //character.Inventory.ReceiveItemReward(3000133, 1, true); //beggarB head
             character.Inventory.ReceiveItemReward(3000130, 1, true); //beggarB chest
             character.Inventory.ReceiveItemReward(3000136, 1, true); //beggarB legs

            character.ChangeFaction(Character.Factions.Deer); //neutral ghosts
            Item ghostKey = ItemManager.Instance.GenerateItemNetwork(keyID);
            ghostKey.transform.position = KeyPosition;
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }

        /*[HarmonyPatch(typeof(CharacterManager), "RequestAreaSwitch")]
        public class CharacterManager_RequestAreaSwitch
        {
            [HarmonyPrefix]
            public static bool Prefix*/

        [HarmonyPatch(typeof(InteractionTriggerBase), "TryActivateBasicAction", new Type[] { typeof(Character), typeof(int) })]
        public class InteractionTriggerBase_TryActivateBasicAction
        {
            [HarmonyPrefix]
            public static bool Prefix(InteractionTriggerBase __instance, Character _character, int _toggleState)
            {
                if (!Instance.IsActiveScenario) return true;
                if (PhotonNetwork.isNonMasterClientInRoom) return true;
                if (_character == null) return true;

                if(SceneManagerHelper.ActiveSceneName != "Chersonese_Dungeon3") return true;

                if (__instance.CurrentTriggerManager as InteractionActivator == true)
                {
                    InteractionActivator activator = __instance.CurrentTriggerManager as InteractionActivator;
                    if (activator.BasicInteraction != null)
                    {
                        var interaction = activator.BasicInteraction;
                        if (interaction as InteractionOpenContainer == true)
                        {
                            return false;
                        }
                        else if(interaction as InteractionToggleContraption == true && Vector3.Distance(_character.CenterPosition, Instance.SpawnPosition) < 5f)
                        {
                            _character.CharacterUI.ShowInfoNotification("Lever is stuck...");
                            return false;
                        }
                        else if(interaction as InteractionSwitchArea == true && !_character.Inventory.OwnsOrHasEquipped(Instance.keyID))
                        {
                            //update quest to requiere key maybe
                            _character.CharacterUI.ShowInfoNotification("It is locked...");
                            return false;
                        }
                    }
                }

                //usefull Stuff
                    //InteractionOpenContainer
                    //InteractionSwitchArea
                    //InteractionWarp
                    //InteractionToggleContraption
                    //InteractionRevive


                return true;
            }
        }

        
    }
}
