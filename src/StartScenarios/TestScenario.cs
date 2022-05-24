using AlternateStart.Characters;
using HarmonyLib;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using SideLoader;
using SideLoader.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class TestScenario : Scenario
    {
        internal static TestScenario Instance { get; private set; }

        public override Scenarios Type => Scenarios.Test;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioAreas Area => ScenarioAreas.Test;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.CierzoVillage;
        public override Vector3 SpawnPosition => default;

        public override bool HasQuest => true;
        public override string QuestName => "Lord of the Berries";

        // Custom Quest Log Signature UIDs
        internal const string TEST_LOG_OBJECTIVE_A = "iggythemad.testquest.objective.a";
        internal const string TEST_LOG_OBJECTIVE_B = "iggythemad.testquest.objective.b";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                TEST_LOG_OBJECTIVE_A,
                "Gather 4 Gaberries."
            },
            {
                TEST_LOG_OBJECTIVE_B,
                "Be happy :)"
            },
        };

        // Custom Quest Events
        internal static QuestEventSignature QE_BerriesPicked;

        // Dialogue character test
        internal static DialogueCharacter testCharacter;

        public TestScenario()
        {
            Instance = this;
        }

        public override void Init()
        {
            base.Init();

            // Stackable quest event for picking berries.
            QE_BerriesPicked = CustomQuests.CreateQuestEvent("iggythemad.testquest.berriesPicked", false, true);

            SetupTestCharacter();
        }

        public override void OnScenarioChosen()
        {
        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();
        }

        public override void OnStartSpawn(Character character)
        {
            character.Inventory.ReceiveItemReward(2000010, 1, false);
        }

        // ~~~~~~~~~~ Quest processing ~~~~~~~~~~

        // Our main method for updating the quest progress
        public override void UpdateQuestProgress(Quest quest)
        {
            // Do nothing if we are not the host.
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            QuestProgress progress = quest.GetComponent<QuestProgress>();

            int berryStack = QuestEventManager.Instance.GetEventCurrentStack(QE_BerriesPicked.EventUID);

            // Update the first log. Set it to completed if berryStack is 4 or higher.
            var firstLog = progress.GetLogSignature(TEST_LOG_OBJECTIVE_A);
            progress.UpdateLogEntry(firstLog, berryStack >= 4);

            if(berryStack > 0)
            {
                ShowUIMessage("Gather Gaberries: " + berryStack + "/4");
            }

            // If quest completed
            if (berryStack >= 4)
            {
                // Don't update this log until 4 or more berries picked, otherwise it appears in the journal.
                var secondLog = progress.GetLogSignature(TEST_LOG_OBJECTIVE_B);
                progress.UpdateLogEntry(secondLog, true);

                progress.DisableQuest(QuestProgress.ProgressState.Successful);
            }
        }

        // Patch on gatherable looting to give the player the quest and update state

        [HarmonyPatch(typeof(Gatherable), nameof(Gatherable.OnGatherInteraction))]
        public class Gatherable_OnGatherInteraction
        {
            const int GABERRIES_ITEMID = 4000010;

            [HarmonyPrefix]
            public static void Postfix(Gatherable __instance)
            {
                if (!Instance.IsActiveScenario)
                    return;

                // Do nothing if we are not the host.
                if (PhotonNetwork.isNonMasterClientInRoom)
                    return;

                if (__instance.m_drops == null)
                    return;

                // Search all the drops for Gaberries
                bool isGaberries = false;
                // iterate over the "Dropable" components
                foreach (var dropper in __instance.m_drops)
                {
                    if (dropper.m_mainDropTables == null)
                        continue;
                    // Iterate over the Dropable's DropTables
                    foreach (var table in dropper.m_mainDropTables)
                    {
                        if (table.m_itemDrops == null)
                            continue;
                        // Check if any of the drops are Gaberries
                        if (table.m_itemDrops.Any(it => it.DroppedItem?.ItemID == GABERRIES_ITEMID))
                        {
                            // Found the Gaberries, break from iterating the DropTables
                            isGaberries = true;
                            break;
                        }
                    }
                    // If we found Gaberries, break from iterating the Dropables
                    if (isGaberries)
                        break;
                }

                if (!isGaberries)
                    return;

                // We are gathering a Gaberries bush.

                // Add a quest event stack
                QuestEventManager.Instance.AddEvent(QE_BerriesPicked, 1);
                

                // Update the quest progress (and give quest if they dont have it)
                var quest = Instance.GetOrGiveQuestToHost();
                Instance.UpdateQuestProgress(quest);
            }
        }

        // ~~~~~~~~~~ Setup custom character dialogue ~~~~~~~~~~~~~~~~~

        private void SetupTestCharacter()
        {
            testCharacter = new()
            {
                UID = "testscenario.character",
                Name = "Test Character",
                SpawnSceneBuildName = "CierzoNewTerrain",
                SpawnPosition = new(1241.508f, 26.6315f, 1683.957f),
                SpawnRotation = new(0, 241.8525f, 0),
                HelmetID = 3100141,
                ChestID = 3100140,
                BootsID = 3100142,
                WeaponID = 2020335,
                StartingPose = Character.SpellCastType.IdleAlternate,
            };

            // Create and apply the template
            var template = testCharacter.CreateAndApplyTemplate();

            // Add a listener to set up our dialogue
            testCharacter.OnSetupDialogueGraph += TestCharacter_OnSetupDialogueGraph;

            // Add this func to determine if our character should actually spawn
            template.ShouldSpawn = () => this.IsActiveScenario;
        }

        private void TestCharacter_OnSetupDialogueGraph(DialogueTree graph, Character character)
        {
            var ourActor = graph.actorParameters[0];

            // Add our root statement
            var rootStatement = graph.AddNode<StatementNodeExt>();
            rootStatement.statement = new("What's up doc?");
            rootStatement.SetActorName(ourActor.name);
            
            // Add a multiple choice
            var multiChoice1 = graph.AddNode<MultipleChoiceNodeExt>();
            multiChoice1.availableChoices.Add(new(statement: new("Who's the man?")));
            multiChoice1.availableChoices.Add(new(statement: new("What is the meaning of life?")));
            multiChoice1.availableChoices.Add(new(statement: new("What's up with that guy IggyTheMad?")));

            // Add our answers
            var answer1 = graph.AddNode<StatementNodeExt>();
            answer1.statement = new("You the man!");
            answer1.SetActorName(ourActor.name);

            var answer2 = graph.AddNode<StatementNodeExt>();
            answer2.statement = new("37.");
            answer2.SetActorName(ourActor.name);
            
            var answer3 = graph.AddNode<StatementNodeExt>();
            answer3.statement = new("I hear he's pretty cool.");
            answer3.SetActorName(ourActor.name);
            
            // ===== finalize nodes =====
            graph.allNodes.Clear();
            // add the nodes we want to use
            graph.allNodes.Add(rootStatement);
            graph.primeNode = rootStatement;
            graph.allNodes.Add(multiChoice1);
            graph.allNodes.Add(answer1);
            graph.allNodes.Add(answer2);
            graph.allNodes.Add(answer3);
            // setup our connections
            graph.ConnectNodes(rootStatement, multiChoice1);    // prime node triggers the multiple choice
            graph.ConnectNodes(multiChoice1, answer1, 0);       // choice1: answer1
            graph.ConnectNodes(answer1, rootStatement);         // - choice1 goes back to root node
            graph.ConnectNodes(multiChoice1, answer2, 1);       // choice2: answer2
            graph.ConnectNodes(answer2, rootStatement);         // - choice2 goes back to root node
            graph.ConnectNodes(multiChoice1, answer3, 2);       // choice3: answer3
            graph.ConnectNodes(answer3, rootStatement);         // - choice3 goes back to root node
        }
    }
}
