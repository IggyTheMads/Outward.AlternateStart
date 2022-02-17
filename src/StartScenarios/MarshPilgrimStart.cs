using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class MarshPilgrimStart : Scenario
    {
        public override Scenarios Type => Scenarios.MarshPilgrim;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioTheme Theme => ScenarioTheme.Freeform;
        public override ScenarioAreas Area => ScenarioAreas.HallowedMarsh;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.HallowedDungeon7;
        public override Vector3 SpawnPosition => new(-55.4f, -40f, -3.1f);
        public override Vector3 SpawnRotation => new(0, 20.4f, 0);

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        public override void OnScenarioBegin()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false);
        }

        public override void OnStartSpawn(Character character)
        {
            character.Inventory.ReceiveItemReward(3000081, 1, true); //worker chest
            character.Inventory.ReceiveItemReward(3000083, 1, true); //worker legs
            character.Inventory.ReceiveItemReward(2020050, 1, true); //fang cub
            character.Inventory.ReceiveItemReward(5100060, 1, true); //torch
            character.Inventory.ReceiveItemReward(4100550, 3, false); //rations
            character.Inventory.RemoveMoney(27, true);
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
