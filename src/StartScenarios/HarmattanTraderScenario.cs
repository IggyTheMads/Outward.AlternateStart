using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class HarmattanTraderScenario : Scenario
    {
        public override Scenarios Type => Scenarios.HarmattanTrader;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioAreas Area => ScenarioAreas.AntiquePlateau;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Harmattan;
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
            character.Inventory.ReceiveItemReward(9000010, 612, false); //bonus mega gold
            character.Inventory.ReceiveItemReward(3000190, 1, true); //chest scholar
            character.Inventory.ReceiveItemReward(3000004, 1, true); //legs trader
        }

        public override void OnStartSpawn()
        {
            CharacterManager.Instance.GetWorldHostCharacter().Inventory.QuestKnowledge.ReceiveQuest(VanillaQuestsHelper.enrollmentQ);
        }

        public override void OnStartSpawn(Character character)
        {
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
