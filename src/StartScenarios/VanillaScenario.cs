using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class VanillaScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_Vanilla;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.VANILLA;
        public override ScenarioPassives Area => ScenarioPassives.Vanilla;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Tutorial;
        public override Vector3 SpawnPosition => default;

        public override bool HasQuest => false;

        public override void OnStartSpawn()
        {
        }

        public override void OnStartSpawn(Character character)
        {
            character.Inventory.AddMoney(27);
            character.Stats.FullStamina();
        }

        public override void UpdateQuestProgress(Quest quest)
        {
        }
    }
}
