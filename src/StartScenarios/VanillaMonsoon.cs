using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class VanillaMonsoon : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_VanillaMonsoon;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioPassives Area => ScenarioPassives.VanillaMonsoon;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.HallowedMarsh; //NEED GEAR UPDATE
        public override Vector3 SpawnPosition => new(481.6f, -63.9f, 492.1f);
        public override Vector3 SpawnRotation => new(0, 63.1f, 0);

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
