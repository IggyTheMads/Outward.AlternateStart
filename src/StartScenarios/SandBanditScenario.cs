using SideLoader;
using SideLoader.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace AlternateStart.StartScenarios
{
    public class SandBanditScenario : Scenario
    {
        public override Scenarios Type => Scenarios.SandBandit;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioAreas Area => ScenarioAreas.Abrassar;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.AbrassarDungeon6;
        public override Vector3 SpawnPosition => new(-53.3f, 0.5f, 55.1f);
        public override Vector3 SpawnRotation => new(0, 227.3f, 0);

        public override bool HasQuest => true;
        public override string QuestName => "Sand Corsair Exile";

        const float GRACE_PERIOD_INGAMETIME = 0.3f;

        const string LogSignature_A = "sandbandits.objective.a";
        const string LogSignature_B = "sandbandits.objective.b";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "You have been exiled from the Sand Corsairs, leave before they turn on you!"
            },
            {
                LogSignature_B,
                "Your grace period is over, Old Levant is no longer your ally."
            }
        };

        static QuestEventSignature QE_StartTimer;

        private Coroutine delayedQuestUpdate;

        internal static SandBanditScenario Instance { get; private set; }
        public SandBanditScenario()
        {
            Instance = this;
        }

        public override void Init()
        {
            base.Init();

            QE_StartTimer = CustomQuests.CreateQuestEvent("iggythemad.sandbandits.starttimer", true, false, true, Plugin.QUEST_EVENT_FAMILY_NAME);

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
        }

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false);
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Inventory.ReceiveItemReward(9000010, 26, false); //Starter Gold
            character.Inventory.ReceiveItemReward(5100010, 1, true); //lamp
            character.Inventory.ReceiveItemReward(3000087, 1, true); //beggar helm
            character.Inventory.ReceiveItemReward(3000201, 1, true); //desert armor
            character.Inventory.ReceiveItemReward(3000205, 1, true); //desert legs
            character.Inventory.ReceiveItemReward(2000110, 1, true); //curved sword
        }

        public override void OnStartSpawn()
        {
            ChangeCharactersFactions(Character.Factions.Bandits, this.QuestLogSignatures[LogSignature_A]);

            QuestEventManager.Instance.AddEvent(QE_StartTimer);

            GetOrGiveQuestToHost();
            StartDelayedQuestUpdate();
        }

        public override void OnStartSpawn(Character character)
        {
        }

        private void ChangeCharactersFactions(Character.Factions faction, string notifText)
        {
            foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                var character = CharacterManager.Instance.GetCharacter(uid);

                if (character.Faction == faction)
                    continue;

                character.ChangeFaction(faction);

                if (!string.IsNullOrEmpty(notifText))
                    ShowUIMessage(notifText);
            }
        }

        private void SL_OnGameplayResumedAfterLoading()
        {
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            var host = CharacterManager.Instance.GetWorldHostCharacter();
            if (host.Inventory.QuestKnowledge.IsItemLearned((int)this.Type))
            {
                var quest = host.Inventory.QuestKnowledge.GetItemFromItemID((int)this.Type) as Quest;
                UpdateQuestProgress(quest);
            }
        }

        void StartDelayedQuestUpdate()
        {
            if (delayedQuestUpdate != null)
                Plugin.Instance.StopCoroutine(delayedQuestUpdate);

            delayedQuestUpdate = Plugin.Instance.StartCoroutine(UpdateQuestAfterDelay());
        }

        IEnumerator UpdateQuestAfterDelay()
        {
            var timer = QuestEventManager.Instance.GetEventActiveTimeDelta(QE_StartTimer.EventUID);

            while (timer < GRACE_PERIOD_INGAMETIME)
            {
                yield return new WaitForSeconds(1f);

                if (QuestEventManager.Instance == null || !CharacterManager.Instance?.GetWorldHostCharacter())
                    yield break;

                timer = QuestEventManager.Instance.GetEventActiveTimeDelta(QE_StartTimer.EventUID);
            }

            var host = CharacterManager.Instance?.GetWorldHostCharacter();

            if (!host)
                yield break;

            if (host.Inventory.QuestKnowledge.IsItemLearned((int)this.Type))
            {
                var quest = host.Inventory.QuestKnowledge.GetItemFromItemID((int)this.Type) as Quest;
                UpdateQuestProgress(quest);
            }

            delayedQuestUpdate = null;
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            var timer = QuestEventManager.Instance.GetEventActiveTimeDelta(QE_StartTimer.EventUID);

            QuestProgress progress = quest.m_questProgress;

            progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), timer >= GRACE_PERIOD_INGAMETIME);

            if (timer >= GRACE_PERIOD_INGAMETIME)
            {
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                ChangeCharactersFactions(Character.Factions.Player, this.QuestLogSignatures[LogSignature_B]);

                progress.DisableQuest(QuestProgress.ProgressState.Successful);
            }
            else
            {
                ChangeCharactersFactions(Character.Factions.Bandits, string.Empty);
                StartDelayedQuestUpdate();
            }
        }

        // [HarmonyPatch(typeof(CharacterManager), "RequestAreaSwitch")]
        // public class CharacterManager_RequestAreaSwitch
        // {
        //     [HarmonyPrefix]
        //     public static bool Prefix(CharacterManager __instance, Character _character, Area _areaToSwitchTo, int _longTravelTime, int _spawnPoint, float _offset, string _overrideLocKey)
        //     {
        //         if (!Instance.IsActiveScenario)
        //             return true;
// 
        //         // Do nothing if we are not the host.
        //         if (PhotonNetwork.isNonMasterClientInRoom)
        //             return true;
// 
        //         if (_areaToSwitchTo.SceneName == "Levant") //IGGY: Cant make quest completion requierement work, I get null reference exception
        //         {
        //             //var host = CharacterManager.Instance.GetWorldHostCharacter();
        //             //var quest = host.Inventory.SkillKnowledge.GetItemFromItemID(Instance.SL_Quest_ItemID) as Quest;
        //             //QuestProgress progress = quest.GetComponent<QuestProgress>();
        //             //if (progress.m_progressState != QuestProgress.ProgressState.Successful)
        //             //{
        //                 _character.CharacterUI.ShowInfoNotification("You are not welcome here");
        //                 return false;
        //             //}
        //             //return true;
        //         }
        //         else { return true; }
        //     }
        // }
    }
}
