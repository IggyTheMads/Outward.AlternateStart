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
        public override Scenarios Type => Scenarios.Vanilla;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioTheme Theme => ScenarioTheme.None;
        public override ScenarioAreas Area => ScenarioAreas.Chersonese;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Tutorial;
        public override Vector3 SpawnPosition => default;

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        public override void OnScenarioBegin()
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
