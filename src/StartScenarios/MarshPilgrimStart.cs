﻿using System;
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
        public override ScenarioAreas Area => ScenarioAreas.HallowedMarsh;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.HallowedDungeon7;
        public override Vector3 SpawnPosition => new(-55.4f, -40f, -3.1f);
        public override Vector3 SpawnRotation => new(0, 20.4f, 0);

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
            character.Inventory.ReceiveItemReward(3000081, 1, true); //worker chest
            character.Inventory.ReceiveItemReward(3000083, 1, true); //worker legs
            character.Inventory.ReceiveItemReward(2020050, 1, true); //fang cub
            character.Inventory.ReceiveItemReward(5100060, 1, true); //torch
            character.Inventory.ReceiveItemReward(4100550, 3, false); //rations
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
