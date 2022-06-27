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
        public override ScenarioQuest Type => ScenarioQuest.Quest_LevantSlums;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.LevantSlums;
        public override void Gear(Character character)
        {
            //character.Inventory.ReceiveSkillReward(8205070); //slow metabolism
            character.Inventory.ReceiveItemReward(9000010, 11, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000171, 1, true); //light chest
            character.Inventory.ReceiveItemReward(3000174, 1, true); //sandals
            character.Inventory.ReceiveItemReward(2000060, 1, false); //machete
            character.Inventory.ReceiveItemReward(5110003, 1, false); //shiv
        }
        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Levant;
        public override Vector3 SpawnPosition => new(-161.3f, 4.4f, 66.3f);
        public override Vector3 SpawnRotation => new(0, 141.5f, 0);

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnScenarioChosen(Character character)
        {

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
