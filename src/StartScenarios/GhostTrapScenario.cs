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
        public override Scenarios Type => Scenarios.GhostTrapScenario;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioTheme Theme => ScenarioTheme.Freeform;
        public override ScenarioAreas Area => ScenarioAreas.Chersonese;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon3;
        public override Vector3 SpawnPosition => new(-29.4f, 0, -40.9f);
        public Vector3 KeyPosition => new(6.2f, -55f, -67.1f);
        public override Vector3 SpawnRotation => new(0, 182.0f, 0);

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

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

            character.Faction = Character.Factions.Deer; //neutral ghosts
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }

        [HarmonyPatch(typeof(InteractionTriggerBase), "TryActivateBasicAction", new[] { typeof(Character), typeof(int) }), HarmonyPrefix]
        static bool InteractionTriggerBase_TryActivate_Pre(InteractionTriggerBase __instance, ref Character _character)
        {
            if (_character == null) return true;
            if(__instance.CurrentTriggerManager.TryAs<InteractionActivator>(out var activator))
            {
                __instance.isstring
            }
            if (activator.BasicInteraction.TryNonNull(out var interaction))
            if (interaction.IsNot<InteractionOpenContainer>() )
            if (interaction.IsNot<InteractionSwitchArea>() )
            if (interaction.IsNot<InteractionWarp>() )
            if(interaction.IsNot<InteractionToggleContraption>() )
            if(interaction.IsNot<InteractionRevive>() )
                return true;


            return false;
        }
    }
}
