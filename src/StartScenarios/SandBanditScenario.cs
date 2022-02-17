using SideLoader;
using SideLoader.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart.StartScenarios
{
    public class SandBanditScenario : Scenario
    {
        public override Scenarios Type => Scenarios.SandBandit;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioTheme Theme => ScenarioTheme.Stamina;
        public override ScenarioAreas Area => ScenarioAreas.Abrassar;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.AbrassarDungeon6;
        public override Vector3 SpawnPosition => new(-53.3f, 0.5f, 55.1f);
        public override Vector3 SpawnRotation => new(0, 227.3f, 0);

        public override string SL_Quest_FileName => "SandBanditsQuest";
        public override int SL_Quest_ItemID => -2303;

        const string LogSignature_A = "sandbandits.objective.a";
        const string LogSignature_B = "sandbandits.objective.b";

        static QuestEventSignature QE_StartTimer;
        const string QE_StartTimer_UID = "iggythemad.sandbandits.starttimer";

        public override void Init()
        {
            base.Init();

            QE_StartTimer = CustomQuests.CreateQuestEvent(QE_StartTimer_UID, true, false, true, Plugin.QUEST_EVENT_FAMILY_NAME);

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

        public override void PreScenarioBegin()
        {
            GetOrGiveQuestToHost();

            VanillaQuestsHelper.SkipHostToFactionChoice(false);
        }

        public override void OnScenarioBegin()
        {
            ChangeCharactersFactions(Character.Factions.Bandits, "The Sand Corsairs sense that your allegiance is slipping...");
        }

        public override void OnStartSpawn(Character character)
        {
            character.Inventory.ReceiveItemReward(9000010, 26, false); //Starter Gold
            character.Inventory.ReceiveItemReward(5100010, 1, true); //lamp
            character.Inventory.ReceiveItemReward(3000087, 1, true); //beggar helm
            character.Inventory.ReceiveItemReward(3000201, 1, true); //desert armor
            character.Inventory.ReceiveItemReward(3000205, 1, true); //desert legs
            character.Inventory.ReceiveItemReward(2000110, 1, true); //curved sword

            //Plugin.Instance.StartCoroutine(UpdateQuestAfterDelay());
        }

        static void ChangeCharactersFactions(Character.Factions faction, string notifText)
        {
            foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                var character = CharacterManager.Instance.GetCharacter(uid);
                character.CharacterUI.ShowInfoNotification(notifText);
                character.ChangeFaction(faction);
            }
        }

        IEnumerator UpdateQuestAfterDelay()
        {
            yield return new WaitForSeconds(20f);

            if (CharacterManager.Instance == null)
                yield break;

            var host = CharacterManager.Instance.GetWorldHostCharacter();

            if (!host)
                yield break;

            if (host.Inventory.SkillKnowledge.IsItemLearned(this.SL_Quest_ItemID))
            {
                var quest = host.Inventory.SkillKnowledge.GetItemFromItemID(this.SL_Quest_ItemID) as Quest;
                UpdateQuestProgress(quest);
            }
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            var host = CharacterManager.Instance.GetWorldHostCharacter();
            if (host.Faction == Character.Factions.Bandits && !QuestEventManager.Instance.HasQuestEvent(QE_StartTimer) && SceneManagerHelper.ActiveSceneName == "Abrassar")
            {
                QuestEventManager.Instance.AddEvent(QE_StartTimer_UID);
            }
            
            var timer = QuestEventManager.Instance.GetEventActiveTimeDelta(QE_StartTimer_UID);

            QuestProgress progress = quest.m_questProgress;

            progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), timer >= 0.1f);

            if (timer >= 0.1f)
            {
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                ChangeCharactersFactions(Character.Factions.Player, "You've deserted the Sand Corsairs!");

                // Remove the timer
                QuestEventManager.Instance.RemoveEvent(QE_StartTimer_UID);

                progress.DisableQuest(QuestProgress.ProgressState.Successful);
            }
            else // Wait another 20 seconds and update it again until its completed. //IGGY: Not working. Only deserts on load screens
                Plugin.Instance.StartCoroutine(UpdateQuestAfterDelay());
        }
    }
}
