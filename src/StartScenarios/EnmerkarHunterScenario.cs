using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class EnmerkarHunterScenario : Scenario
    {
        public override Scenarios Type => Scenarios.EmercarHunter;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioTheme Theme => ScenarioTheme.Stamina;
        public override ScenarioAreas Area => ScenarioAreas.EnmerkarForest;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.EmercarDungeonsSmall;
        public override Vector3 SpawnPosition => new(600.6f, 0.8f, 8.1f);
        public override Vector3 SpawnRotation => new(0, 29.3f, 0);

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        public override void OnScenarioBegin()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false);
        }

        public override void OnStartSpawn(Character character)
        {
            character.Stats.FullStamina();
            character.Inventory.ReceiveItemReward(9000010, 220, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000020, 1, true); //adventurer armor
            character.Inventory.ReceiveItemReward(3000022, 1, true); //adventurer boots
            character.Inventory.ReceiveItemReward(2200000, 1, false); //bow
            character.Inventory.ReceiveItemReward(5200001, 30, true); //arrows
            character.Inventory.ReceiveItemReward(4000060, 4, false); //meat
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
