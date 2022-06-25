using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class VanillaAlternateScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_VanillaAlt;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioPassives Area => ScenarioPassives.VanillaAlt;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.CierzoOutside;
        public override Vector3 SpawnPosition => new(1440.4f, 4.9f, 527.6f);
        public override Vector3 SpawnRotation => new(0, 45.81f, 0);

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
