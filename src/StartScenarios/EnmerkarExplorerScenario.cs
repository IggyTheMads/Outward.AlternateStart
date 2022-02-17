using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class EnmerkarExplorerScenario : Scenario
    {
        public override Scenarios Type => Scenarios.EmercarExplorer;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioTheme Theme => ScenarioTheme.Freeform;
        public override ScenarioAreas Area => ScenarioAreas.EnmerkarForest;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Emercar;
        public override Vector3 SpawnPosition => new(581.3f, 14.2f, 391f);
        public override Vector3 SpawnRotation => new(0, 207f, 0);

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        public override void OnScenarioBegin()
        {
            VanillaQuestsHelper.StartHouseTimer();
        }

        public override void OnStartSpawn(Character character)
        {
            character.Stats.AffectHealth(-28);
            character.StatusEffectMngr.AddStatusEffect("Poisoned");
            character.StatusEffectMngr.AddStatusEffect("Bleeding");

            character.Inventory.ReceiveItemReward(9000010, 123, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000010, 1, true); //padded chest
            character.Inventory.ReceiveItemReward(3000012, 1, true); //padded legs
            character.Inventory.ReceiveItemReward(2100080, 1, true); //fang cub
            character.Inventory.ReceiveItemReward(4400010, 3, false); //bandage
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
