using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SideLoader;

namespace AlternateStart.StartScenarios
{
    public class RobbedTraderScenario : Scenario
    {
        public override Scenarios Type => Scenarios.RobbedTrader;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioAreas Area => ScenarioAreas.Chersonese;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.CierzoOutside;
        public override Vector3 SpawnPosition => new(1157.1f, 10.6f, 1059.7f);
        public override Vector3 SpawnRotation => new(0, 48.8f, 0);

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };


        //variables
        public string enemyID = "com.iggy.banditgeneric";

        public override void Init()
        {
            base.Init();

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
        }
        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.StartHouseTimer();
        }

        public override void OnScenarioChosen(Character character)
        {
            //character.Stats.IncreaseBurntHealth(200, 1);
            character.Inventory.ReceiveItemReward(9000010, 116, false); //bonus gold
            character.Inventory.ReceiveItemReward(3000190, 1, true); //chest scholar
            character.Inventory.ReceiveItemReward(3000004, 1, true); //legs trader
            character.Inventory.ReceiveItemReward(-2353, 1, true); //bag of goods
        }

        public override void OnStartSpawn()
        {

            VanillaQuestsHelper.StartHouseTimer();
        }

        public override void OnStartSpawn(Character character)
        {
            ShowUIMessage("Get the goods safely to Cierzo!");
            character.StatusEffectMngr.AddStatusEffect("AdrenalineRush");
        }

        private void SL_OnGameplayResumedAfterLoading()
        {
            var myChar = SL.GetSLPack("iggythemad AlternateStart").CharacterTemplates[enemyID];
            var banditAmount = 3;
            for (int i = 0; i < banditAmount; i++)
            {
                var enemyOffset = new Vector3(UnityEngine.Random.Range(20f, 30f), 4f, UnityEngine.Random.Range(20f, 30f));
                var enemySpawn = SpawnPosition + enemyOffset;
                myChar.Spawn(enemySpawn, (enemySpawn + SpawnPosition).normalized, UID.Generate());
            }
        }

        public override void UpdateQuestProgress(Quest quest)
        {

        }
    }
}
