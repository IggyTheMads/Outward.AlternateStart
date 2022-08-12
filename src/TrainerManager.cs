using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;
using SideLoader;

namespace AlternateStart
{
    public static class TrainerManager
    {
        // This class is just used for setting up the Trainer NPC. The Skill Tree is set up by the XML file.

        internal static SL_Character bergSpellblade;
        static SLPack pack;

        public static void Init()
        {
            pack = SL.GetSLPack("iggythemad AlternateStart");

            bergSpellblade = pack.CharacterTemplates["com.iggy.berg.spellblade.trainer"];
            bergSpellblade.OnSpawn += SpellbladeSetup;

            SL.OnGameplayResumedAfterLoading += spawnSpellblade;
        }

        private static void spawnSpellblade()
        {
            string scene = SceneManagerHelper.ActiveSceneName;
            if (scene == "Berg")
            {
                Character host = CharacterManager.Instance.GetWorldHostCharacter();
                if(!host.IsLocalPlayer) { return; }
                if(QuestEventManager.Instance.HasQuestEvent(QuestEventDictionary.GetQuestEvent(VanillaQuestsHelper.cierzoDestroy)))
                {
                    string trainerID;
                    Vector3 trainerPos;
                    Vector3 trainerRot;
                    trainerPos = new Vector3(1284.4f, -3.7f, 1622.2f);
                    trainerRot = new Vector3(0, 203f, 0);
                    trainerID = "com.iggy.berg.spellblade.trainer";
                    SpawnCharacter(trainerID, trainerPos, trainerRot);
                }
            }
        }

        public static Character SpawnCharacter(string trainerID, Vector3 trainerPos, Vector3 trainerRot)
        {
            var myChar = pack.CharacterTemplates[trainerID];
            Character spawnee = myChar.Spawn(trainerPos, trainerRot, UID.Generate());
            return spawnee;
        }

        public static void SpellbladeSetup(Character trainer, string _)
        {
            //Character host = CharacterManager.Instance.GetWorldHostCharacter();
            //if (host.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Survivor))
            //{
                GenericTrainerSetup(trainer,
                bergSpellblade,
                "com.iggy.bergspellblade",
                "You are alive! It's good to see a familiar face.",
                "What can you teach me?",
                "What happened to Cierzo?",
                "What should I do now?",
                "The Vendavel Scum. We were not prepared for an attack. I tried to fight back, but it was too late.",
                "Move on. Join a faction. Rissa is here, but you are free to do as you like.");
            //}
        }

        public static void GenericTrainerSetup(Character trainer, SL_Character currentCharacter, string treeUID, string introDialogue, string ask1, string ask2, string ask3, string reply2, string reply3)
        {
            // remove unwanted components
            GameObject.DestroyImmediate(trainer.GetComponent<CharacterStats>());
            GameObject.DestroyImmediate(trainer.GetComponent<StartingEquipment>());

            // add NPCLookFollow component
            //trainer.gameObject.AddComponent<NPCLookFollow>();

            // set Dialogue Actor name
            var actor = trainer.GetComponentInChildren<DialogueActor>();
            actor.SetName(currentCharacter.Name);

            // get "Trainer" component, and set the SkillTreeUID to our custom tree UID
            var trainerComp = trainer.GetComponentInChildren<Trainer>();
            trainerComp.m_skillTreeUID = new UID(treeUID);

            // setup dialogue tree
            var graphController = trainer.GetComponentInChildren<DialogueTreeController>();
            var graph = graphController.graph;

            // the template comes with an empty ActorParameter, we can use that for our NPC actor.
            var actors = (graph as DialogueTree)._actorParameters;
            actors[0].actor = actor;
            actors[0].name = actor.name;

            // setup the actual dialogue now
            var rootStatement = graph.AddNode<StatementNodeExt>();
            rootStatement.statement = new Statement(introDialogue);
            rootStatement.SetActorName(actor.name);

            var multiChoice1 = graph.AddNode<MultipleChoiceNodeExt>();
            multiChoice1.availableChoices.Add(new MultipleChoiceNodeExt.Choice { statement = new Statement { text = ask1 } });
            multiChoice1.availableChoices.Add(new MultipleChoiceNodeExt.Choice { statement = new Statement { text = ask2 } });
            multiChoice1.availableChoices.Add(new MultipleChoiceNodeExt.Choice { statement = new Statement { text = ask3 } });

            // the template already has an action node for opening the Train menu. 
            // Let's grab that and change the trainer to our custom Trainer component (setup above).
            var openTrainer = graph.allNodes[1] as ActionNode;
            (openTrainer.action as TrainDialogueAction).Trainer = new BBParameter<Trainer>(trainerComp);

            // create some custom dialogue
            var answer2 = graph.AddNode<StatementNodeExt>();
            answer2.statement = new Statement(reply2);
            answer2.SetActorName(actor.name);

            var answer3 = graph.AddNode<StatementNodeExt>();
            answer3.statement = new Statement(reply3);
            answer3.SetActorName(actor.name);

            // ===== finalize nodes =====
            graph.allNodes.Clear();
            // add the nodes we want to use
            graph.allNodes.Add(rootStatement);
            graph.allNodes.Add(multiChoice1);
            graph.allNodes.Add(openTrainer);
            graph.allNodes.Add(answer2);
            graph.allNodes.Add(answer3);
            graph.primeNode = rootStatement;
            graph.ConnectNodes(rootStatement, multiChoice1);    // prime node triggers the multiple choice
            graph.ConnectNodes(multiChoice1, openTrainer, 0);   // choice1: open trainer
            graph.ConnectNodes(multiChoice1, answer2, 1);       // choice2: answer1
            graph.ConnectNodes(answer2, rootStatement);         // - choice2 goes back to root node
            graph.ConnectNodes(multiChoice1, answer3, 2);       // choice3: answer2
            graph.ConnectNodes(answer3, rootStatement);         // - choice3 goes back to root node

            // set the trainer active
            trainer.gameObject.SetActive(true);
        }
    }
}