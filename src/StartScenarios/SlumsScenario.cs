using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class SlumsScenario : Scenario
    {
        public override Scenarios Type => Scenarios.LevantSlums;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioTheme Theme => ScenarioTheme.Stamina;
        public override ScenarioAreas Area => ScenarioAreas.Abrassar;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Levant;
        public override Vector3 SpawnPosition => new(-161.3f, 4.4f, 66.3f);
        public override Vector3 SpawnRotation => new(0, 141.5f, 0);

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        public override void PreScenarioBegin()
        {

        }
        public override void OnScenarioBegin()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false);
        }

        public override void OnStartSpawn(Character character)
        {
            character.Inventory.ReceiveSkillReward(8205070); //slow metabolism
            character.Inventory.ReceiveItemReward(9000010, 34, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000171, 1, true); //light chest
            character.Inventory.ReceiveItemReward(3000174, 1, true); //sandals
            character.Inventory.ReceiveItemReward(2000060, 1, false); //machete
            character.Inventory.ReceiveItemReward(5110003, 1, false); //shiv
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
