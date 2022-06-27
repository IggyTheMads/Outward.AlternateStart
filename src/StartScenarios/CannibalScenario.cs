using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class CannibalScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_Cannibal;
        public override ScenarioType Difficulty => ScenarioType.WIPtest;
        public override ScenarioPassives Passive => ScenarioPassives.Cannibal;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.HallowedDungeon7;
        public override Vector3 SpawnPosition => new(-55.4f, -40f, -3.1f);
        public override Vector3 SpawnRotation => new(0, 20.4f, 0);
        public override void Gear(Character character)
        {
            character.Inventory.ReceiveItemReward(3000081, 1, true); //worker chest
            character.Inventory.ReceiveItemReward(3000083, 1, true); //worker legs
            character.Inventory.ReceiveItemReward(2020050, 1, true); //fang cub
            character.Inventory.ReceiveItemReward(5100060, 1, true); //torch
            character.Inventory.ReceiveItemReward(4100550, 3, false); //rations
        }
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
