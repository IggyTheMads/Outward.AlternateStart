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
        public override ScenarioTheme Theme => ScenarioTheme.Magic;
        public override ScenarioAreas Area => ScenarioAreas.AntiquePlateau;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Harmattan;
        public override Vector3 SpawnPosition => default;

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        public override void OnStartSpawn(Character character)
        {
            character.Stats.FullStamina();
            character.Inventory.ReceiveItemReward(9000010, 612, false); //bonus mega gold
            character.Inventory.ReceiveItemReward(3000190, 1, true); //chest scholar
            character.Inventory.ReceiveItemReward(3000004, 1, true); //legs trader
        }

        public override void OnScenarioBegin()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false);
            CharacterManager.Instance.GetWorldHostCharacter().Inventory.QuestKnowledge.ReceiveQuest(VanillaQuestsHelper.enrollmentQ);
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
