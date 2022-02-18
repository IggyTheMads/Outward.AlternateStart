using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SideLoader;

namespace AlternateStart.StartScenarios
{
    public class VendavelSlaveScenario : Scenario
    {
        public override Scenarios Type => Scenarios.VendavelSlave;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioTheme Theme => ScenarioTheme.Freeform;
        public override ScenarioAreas Area => ScenarioAreas.Chersonese;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon1;
        public override Vector3 SpawnPosition => new(-4.9f, -10f, 26.9f);
        public override Vector3 SpawnRotation => new(0, 356.3f, 0);

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        //variables
        public string enemyID = "com.iggy.vendavelbandit";
        public Vector3 prisonJump = new(43f,-9.7f, 34.7f);
        public Vector3 prisonDoors = new(15f, -10f, 4.4f);


        public override void PreScenarioBegin()
        {

        }
        public override void OnScenarioBegin()
        {
            VanillaQuestsHelper.StartHouseTimer();

            var myChar = SL.GetSLPack("iggythemad AlternateStart").CharacterTemplates[enemyID];
            myChar.Spawn(prisonJump, Vector3.back, UID.Generate());
        }

        public override void OnStartSpawn(Character character)
        {    
             character.Stats.IncreaseBurntHealth(200, 1);
             /*character.Inventory.ReceiveItemReward(3000133, 1, true); //beggarB head
             character.Inventory.ReceiveItemReward(3000130, 1, true); //beggarB chest
             character.Inventory.ReceiveItemReward(3000136, 1, true); //beggarB legs*/
             character.Inventory.RemoveMoney(27, true);
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
