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
    public class ConfluxWatcherScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_ConfluxWatcher;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.ConfluxWatcher;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon4;
        public override Vector3 SpawnPosition => new(-435.2f, -19.1f, -96.5f);
        public override Vector3 SpawnRotation => new(0, 172.4f, 0);
        public override void Gear(Character character)
        {
            character.Inventory.ReceiveSkillReward(8100220); //shim
            character.Inventory.ReceiveSkillReward(8100230); //egoth
            character.Stats.SetManaPoint(3);

            character.Inventory.ReceiveItemReward(3000134, 1, true); //beggarB head
            character.Inventory.ReceiveItemReward(3000131, 1, true); //beggarB chest
            character.Inventory.ReceiveItemReward(3000136, 1, true); //beggarB legs
            //character.Inventory.ReceiveItemReward(2010050, 1, false); //hatchet weapon
            character.Inventory.ReceiveItemReward(5100500, 1, true); //lexicon
        }
        public override bool HasQuest => true;
        public override string QuestName => "Thy watch has ended";

        const string LogSignature_A = "watcher.objective.a";
        const string LogSignature_B = "watcher.objective.b";
        const string LogSignature_C = "watcher.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Get out into the world."
            },
            {
                LogSignature_B,
                "Visit the nearby village of Cierzo."
            },
            {
                LogSignature_C,
                "You are again part of the world."
            }
        };

        private QuestEventSignature QE_FixedWatcherStart;

        public override void Init()
        {
            base.Init();

            QE_FixedWatcherStart = CustomQuests.CreateQuestEvent("iggythemad.watcher.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
        }

        private void SL_OnGameplayResumedAfterLoading()
        {
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            Character host = CharacterManager.Instance.GetWorldHostCharacter();
            if (host.Inventory.QuestKnowledge.IsItemLearned((int)this.Type))
            {
                Quest quest = host.Inventory.QuestKnowledge.GetItemFromItemID((int)this.Type) as Quest;
                UpdateQuestProgress(quest);
            }
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            // Do nothing if we are not the host.
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            Character host = CharacterManager.Instance.GetWorldHostCharacter();
            // Each scene load we add 1 to this quest event stack, until it reaches 3.
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedWatcherStart.EventUID);
            QuestProgress progress = quest.m_questProgress;

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedWatcherStart, 1);
                stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedWatcherStart.EventUID);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("My watch is over. The world might need me.");
            }
            else if (stack == 1 && SceneManagerHelper.ActiveSceneName == "ChersoneseNewTerrain")
            {
                // Second log
                QuestEventManager.Instance.AddEvent(QE_FixedWatcherStart, 1);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), false);
                ShowUIMessage("There used to be a village nearby...Cierzo, is it?");
                //VanillaQuestsHelper.DestroyCierzo(false, false);
            }
            else if (/*stack == 2 &&*/ SceneManagerHelper.ActiveSceneName == "CierzoNewTerrain")
            {
                QuestEventManager.Instance.AddEvent(QE_FixedWatcherStart, 1);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, false);
                ShowUIMessage("I should ask around...");

            }
        }

        public override void OnScenarioChosen()
        {
            //VanillaQuestsHelper.SkipHostToFactionChoice(false, false);
        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();
        }

        public override void OnStartSpawn(Character character)
        {

        }

        public override void OnScenarioChosen(Character character)
        {

        }

    }
}
