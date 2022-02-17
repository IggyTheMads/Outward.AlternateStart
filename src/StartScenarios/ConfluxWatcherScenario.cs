using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class ConfluxWatcherScenario : Scenario
    {
        public override Scenarios Type => Scenarios.ConfluxWatcher;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioTheme Theme => ScenarioTheme.Magic;
        public override ScenarioAreas Area => ScenarioAreas.Chersonese;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon4;
        public override Vector3 SpawnPosition => new(-435.2f, -19.1f, -96.5f);
        public override Vector3 SpawnRotation => new(0, 172.4f, 0);

        public override string SL_Quest_FileName => null;
        public override int SL_Quest_ItemID => -1;

        public override void OnScenarioBegin()
        {

        }

        public override void OnStartSpawn(Character character)
        {    
             character.Inventory.ReceiveSkillReward(8100220); //shim
             character.Inventory.ReceiveSkillReward(8100230); //egoth
             character.Inventory.ReceiveItemReward(3000134, 1, true); //beggarB head
             character.Inventory.ReceiveItemReward(3000131, 1, true); //beggarB chest
             character.Inventory.ReceiveItemReward(3000136, 1, true); //beggarB legs
             character.Inventory.ReceiveItemReward(2010050, 1, false); //hatchet weapon
             character.Inventory.ReceiveItemReward(5100500, 1, true); //lexicon
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }
    }
}
