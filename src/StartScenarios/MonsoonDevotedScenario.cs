using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class MonsoonDevotedScenario : Scenario
    {
        public override Scenarios Type => Scenarios.MonsoonDevoted;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioAreas Area => ScenarioAreas.HallowedMarsh;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Monsoon;
        public override Vector3 SpawnPosition => default;

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false);
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Stats.FullStamina();
            character.Inventory.ReceiveSkillReward(8100250); //chakram arc
            character.Inventory.ReceiveItemReward(9000010, 190, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000071, 1, true); //apprentice helm
            character.Inventory.ReceiveItemReward(3000070, 1, true); //apprentice chest
            character.Inventory.ReceiveItemReward(3000174, 1, true); //sandals
            character.Inventory.ReceiveItemReward(2020010, 1, false); //mace
            character.Inventory.ReceiveItemReward(5110030, 1, true); //chakram
        }

        public override void OnStartSpawn()
        {
        }

        public override void OnStartSpawn(Character character)
        {
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
