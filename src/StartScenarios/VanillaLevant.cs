using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class VanillaLevant : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_VanillaLevant;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioPassives Area => ScenarioPassives.VanillaLevant;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Abrassar; //NEED GEAR UPDATE
        public override Vector3 SpawnPosition => new(225.8f, -148.5f, -499.4f);
        public override Vector3 SpawnRotation => new(0, 73f, 0);

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
            character.Inventory.ReceiveItemReward(3000280, 1, true); //leather chest
            character.Inventory.ReceiveItemReward(3000282, 1, true); //leather legs
            character.Inventory.ReceiveItemReward(2130030, 1, false); //stick weapon
            character.Inventory.ReceiveItemReward(5110003, 1, true); //shiv
        }

        public override void OnStartSpawn()
        {
        }

        public override void OnStartSpawn(Character character)
        {
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            throw new NotImplementedException();
        }
    }
}
