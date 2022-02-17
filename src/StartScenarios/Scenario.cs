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
    public abstract class Scenario
    {
        // Our scenario classes will override these properties with the actual data
        public abstract Scenarios Type { get; }
        public abstract ScenarioDifficulty Difficulty { get; }
        public abstract ScenarioTheme Theme { get; }
        public abstract ScenarioAreas Area { get; }

        public abstract AreaManager.AreaEnum SpawnScene { get; }
        public abstract Vector3 SpawnPosition { get; }
        public virtual Vector3 SpawnRotation => Quaternion.identity.eulerAngles;

        public virtual string SL_Quest_FileName { get; }
        public virtual int SL_Quest_ItemID { get; } = -1;

        public QuestEventSignature QE_ScenarioQuestEvent { get; private set; }
        public string QE_Scenario_UID => $"iggythemad.scenarios.{this.GetType().Name}";

        public bool IsActiveScenario => QuestEventManager.Instance.HasQuestEvent(QE_ScenarioQuestEvent);
        
        // Abstract methods (must override)

        public abstract void UpdateQuestProgress(Quest quest);

        public abstract void OnScenarioBegin();

        public abstract void OnStartSpawn(Character character);

        // Virtual methods (optional override)

        public virtual void Init()
        {
            SL.OnPacksLoaded += OnPacksLoaded;

            QE_ScenarioQuestEvent = CustomQuests.CreateQuestEvent(QE_Scenario_UID, false, false, true, Plugin.QUEST_EVENT_FAMILY_NAME);
        }

        public virtual void OnPacksLoaded()
        {
            if (!string.IsNullOrEmpty(this.SL_Quest_FileName) && this.SL_Quest_ItemID != -1)
            {
                var quest = SL.GetSLPack(Plugin.SL_PACK_NAME).GetContentByFileName<SL_Item>(SL_Quest_FileName) as SL_Quest;
                quest.OnQuestLoaded += UpdateQuestProgress;
            }
        }

        // Concrete methods (cannot override)

        public IEnumerator StartScenario()
        {
            // Give the host the DESTINY CHOSEN quest event
            QuestEventManager.Instance.AddEvent(ScenarioManager.QE_DestinyChosen);

            QuestEventManager.Instance.AddEvent(QE_ScenarioQuestEvent);

            // Autoknock the players
            foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                var character = CharacterManager.Instance.GetCharacter(uid);
                character.AutoKnock(true, Vector3.back, character);
            }

            ScenarioManager.SetFullStop(false);

            yield return new WaitForSeconds(1f);

            // Teleport to start area

            NetworkLevelLoader.Instance.RequestSwitchArea(AreaManager.Instance.GetArea(this.SpawnScene).SceneName, 0, 1.5f);

            SL.OnGameplayResumedAfterLoading += OnGameplayResumedAfterScenarioStart;
        }

        private void OnGameplayResumedAfterScenarioStart()
        {
            // Teleport players to spawn position
            foreach (var uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                var character = CharacterManager.Instance.GetCharacter(uid);

                if (SpawnPosition != default)
                    character.Teleport(SpawnPosition, SpawnRotation);

                character.SpellCastProcess((int)Character.SpellCastType.GetUpBelly, (int)Character.SpellCastModifier.Immobilized, 0, 0);

                OnStartSpawn(character);
            }

            OnScenarioBegin();

            SL.OnGameplayResumedAfterLoading -= OnGameplayResumedAfterScenarioStart;
        }

        public Quest GetOrGiveQuestToHost()
        {
            Character character = CharacterManager.Instance.GetWorldHostCharacter();

            if (character.Inventory.QuestKnowledge.IsItemLearned(this.SL_Quest_ItemID))
                return character.Inventory.QuestKnowledge.GetItemFromItemID(this.SL_Quest_ItemID) as Quest;

            Quest quest = ItemManager.Instance.GenerateItemNetwork(this.SL_Quest_ItemID) as Quest;
            quest.transform.SetParent(character.Inventory.QuestKnowledge.transform);
            character.Inventory.QuestKnowledge.AddItem(quest);

            QuestProgress progress = quest.GetComponent<QuestProgress>();
            progress.m_progressState = QuestProgress.ProgressState.InProgress;

            UpdateQuestProgress(quest);

            return quest;
        }
    }
}
