﻿using SideLoader;
using SideLoader.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class VanillaLevant : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_VanillaLevant;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioPassives Area => ScenarioPassives.VanillaLevant;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Abrassar; //NEED GEAR UPDATE
        public override Vector3 SpawnPosition => new(225.8f, -148.5f, -499.4f);
        public override Vector3 SpawnRotation => new(0, 73f, 0);

        public override bool HasQuest => true;
        public override string QuestName => "A New Beginning";

        const string LogSignature_A = "vanilla.objective.a";
        const string LogSignature_B = "vanilla.objective.b";
        const string LogSignature_C = "vanilla.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Visit any city."
            },
            {
                LogSignature_B,
                "Find a familiar face."
            },
            {
                LogSignature_C,
                "You are ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedVanillaStart;

        public override void Init()
        {
            base.Init();

            QE_FixedVanillaStart = CustomQuests.CreateQuestEvent("iggythemad.vanillal.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

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
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedVanillaStart.EventUID);
            QuestProgress progress = quest.m_questProgress;

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedVanillaStart, 1);
                stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedVanillaStart.EventUID);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("I should visit the nearest city...");
            }
            else if (stack < 2 && AreaManager.Instance.GetIsCurrentAreaTownOrCity() == true)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedVanillaStart, 2);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
                ShowUIMessage("I should join a faction...");
            }
        }

        public override void OnScenarioChosen()
        {
            //VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Inventory.AddMoney(27);
            character.Inventory.ReceiveItemReward(3000181, 1, true); //dancer chest
            character.Inventory.ReceiveItemReward(3000184, 1, true); //dancer legs
            character.Inventory.ReceiveItemReward(2000110, 1, true); //desertsword weapon
            character.Inventory.ReceiveItemReward(4200040, 1, false); //water
        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();
        }

        public override void OnStartSpawn(Character character)
        {
        }
    }
}
