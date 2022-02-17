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
    public class GiantRisenScenario : Scenario
    {
        public override Scenarios Type => Scenarios.GiantRisen;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioTheme Theme => ScenarioTheme.Tanky;
        public override ScenarioAreas Area => ScenarioAreas.HallowedMarsh;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.HallowedDungeon2;
        public override Vector3 SpawnPosition => default;

        public override string SL_Quest_FileName => "GiantsQuest";
        public override int SL_Quest_ItemID => -2302;

        private QuestEventSignature QE_FixedGiantRisenStart;

        const string LogSignature_A = "giantquest.objective.a";
        const string LogSignature_B = "giantquest.objective.b";

        public override void Init()
        {
            base.Init();

            QE_FixedGiantRisenStart = CustomQuests.CreateQuestEvent("iggythemad.giantrisen.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
        }

        private void SL_OnGameplayResumedAfterLoading()
        {
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            var host = CharacterManager.Instance.GetWorldHostCharacter();
            if (host.Inventory.SkillKnowledge.IsItemLearned(this.SL_Quest_ItemID))
            {
                var quest = host.Inventory.SkillKnowledge.GetItemFromItemID(this.SL_Quest_ItemID) as Quest;
                UpdateQuestProgress(quest);
            }
        }

        public override void OnScenarioBegin()
        {
            GetOrGiveQuestToHost();

            VanillaQuestsHelper.AddQuestEvent(VanillaQuestsHelper.ashFight);
            VanillaQuestsHelper.SkipHostToFactionChoice(false);

            // Add 1 to our tracker event stack. Next scene load we will reset the quest events.
            QuestEventManager.Instance.AddEvent(QE_FixedGiantRisenStart);
        }

        public override void OnStartSpawn(Character character)
        {    
            character.Inventory.ReceiveSkillReward(8205040); //fitness passive
            character.Inventory.ReceiveSkillReward(8205030); //steady arm passive
            character.Inventory.ReceiveItemReward(3000221, 1, true); //ash head
            character.Inventory.ReceiveItemReward(3000220, 1, true); //ash chest
            character.Inventory.ReceiveItemReward(3000222, 1, true); //ash legs
            character.Inventory.ReceiveItemReward(2110000, 1, true); //brutal greataxe
            character.Inventory.RemoveMoney(27, true);
        }

        // We call this on scene loads.
        public override void UpdateQuestProgress(Quest quest)
        {
            // Each scene load we add 1 to this quest event stack, until it reaches 2.
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedGiantRisenStart.EventUID);

            if (stack < 2)
                QuestEventManager.Instance.AddEvent(QE_FixedGiantRisenStart);

            // Update the first log no matter what. It's completed if our stack is 2 or higher.
            QuestProgress progress = quest.m_questProgress;
            progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), stack >= 2);

            // If we reached 2, remove the giant quest events and add the second log.
            if (QuestEventManager.Instance.GetEventCurrentStack(QE_FixedGiantRisenStart.EventUID) >= 2)
            {
                // Second log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.RemoveEvent(VanillaQuestsHelper.ashWarp);
                VanillaQuestsHelper.RemoveEvent(VanillaQuestsHelper.ashFight);
                VanillaQuestsHelper.RemoveEvent(VanillaQuestsHelper.ashAllyFail);
                VanillaQuestsHelper.RemoveEvent(VanillaQuestsHelper.ashCompleteFail);
            }
        }
    }
}
