//using HarmonyLib;
//using SideLoader;
//using SideLoader.Managers;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;

//namespace AlternateStart.StartScenarios
//{
//    public class TestScenario : Scenario
//    {
//        internal static TestScenario Instance { get; private set; }

//        public override Scenarios Type => Scenarios.Test;
//        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
//        public override ScenarioAreas Area => ScenarioAreas.Test;

//        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.CierzoVillage;
//        public override Vector3 SpawnPosition => default;

//        public override bool HasQuest => true;
//        public override string QuestName => "Lord of the Berries";

//        // Custom Quest Log Signature UIDs
//        internal const string TEST_LOG_OBJECTIVE_A = "iggythemad.testquest.objective.a";
//        internal const string TEST_LOG_OBJECTIVE_B = "iggythemad.testquest.objective.b";
//        public override Dictionary<string, string> QuestLogSignatures => new()
//        {
//            {
//                TEST_LOG_OBJECTIVE_A,
//                "Gather 4 Gaberries."
//            },
//            {
//                TEST_LOG_OBJECTIVE_B,
//                "Be happy :)"
//            },
//        };

//        // Custom Quest Events
//        internal static QuestEventSignature QE_BerriesPicked;

//        public TestScenario()
//        {
//            Instance = this;
//        }

//        public override void Init()
//        {
//            base.Init();

//            // Stackable quest event for picking berries.
//            QE_BerriesPicked = CustomQuests.CreateQuestEvent("iggythemad.testquest.berriesPicked", false, true);
//        }

//        public override void PreScenarioBegin()
//        {

//        }
//        public override void OnScenarioBegin()
//        {
//            GetOrGiveQuestToHost();
//        }

//        public override void OnStartSpawn(Character character)
//        {
//            character.Inventory.ReceiveItemReward(2000010, 1, false);
//        }

//        // ~~~~~~~~~~ Quest processing ~~~~~~~~~~

//        // Our main method for updating the quest progress
//        public override void UpdateQuestProgress(Quest quest)
//        {
//            // Do nothing if we are not the host.
//            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
//                return;

//            QuestProgress progress = quest.GetComponent<QuestProgress>();

//            int berryStack = QuestEventManager.Instance.GetEventCurrentStack(QE_BerriesPicked.EventUID);

//            // Update the first log. Set it to completed if berryStack is 4 or higher.
//            var firstLog = progress.GetLogSignature("teststart.objective.a");
//            progress.UpdateLogEntry(firstLog, berryStack >= 4);

//            // If quest completed
//            if (berryStack >= 4)
//            {
//                // Don't update this log until 4 or more berries picked, otherwise it appears in the journal.
//                var secondLog = progress.GetLogSignature("teststart.objective.b");
//                progress.UpdateLogEntry(secondLog, true);

//                progress.DisableQuest(QuestProgress.ProgressState.Successful);
//            }
//        }

//        // Patch on gatherable looting to give the player the quest and update state

//        [HarmonyPatch(typeof(Gatherable), nameof(Gatherable.OnGatherInteraction))]
//        public class Gatherable_OnGatherInteraction
//        {
//            const int GABERRIES_ITEMID = 4000010;

//            [HarmonyPrefix]
//            public static void Postfix(Gatherable __instance)
//            {
//                if (!Instance.IsActiveScenario)
//                    return;

//                // Do nothing if we are not the host.
//                if (PhotonNetwork.isNonMasterClientInRoom)
//                    return;

//                if (__instance.m_drops == null)
//                    return;

//                // Search all the drops for Gaberries
//                bool isGaberries = false;
//                // iterate over the "Dropable" components
//                foreach (var dropper in __instance.m_drops)
//                {
//                    if (dropper.m_mainDropTables == null)
//                        continue;
//                    // Iterate over the Dropable's DropTables
//                    foreach (var table in dropper.m_mainDropTables)
//                    {
//                        if (table.m_itemDrops == null)
//                            continue;
//                        // Check if any of the drops are Gaberries
//                        if (table.m_itemDrops.Any(it => it.DroppedItem?.ItemID == GABERRIES_ITEMID))
//                        {
//                            // Found the Gaberries, break from iterating the DropTables
//                            isGaberries = true;
//                            break;
//                        }
//                    }
//                    // If we found Gaberries, break from iterating the Dropables
//                    if (isGaberries)
//                        break;
//                }

//                if (!isGaberries)
//                    return;

//                // We are gathering a Gaberries bush.

//                // Add a quest event stack
//                QuestEventManager.Instance.AddEvent(QE_BerriesPicked, 1);

//                // Update the quest progress (and give quest if they dont have it)
//                var quest = Instance.GetOrGiveQuestToHost();
//                Instance.UpdateQuestProgress(quest);
//            }
//        }
//    }
//}
