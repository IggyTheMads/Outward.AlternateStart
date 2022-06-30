using SideLoader;
using SideLoader.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.Collections;

namespace AlternateStart.StartScenarios
{
    public class GiantRisenScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_GiantRisen;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.GiantRisen;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.HallowedDungeon2;
        public override Vector3 SpawnPosition => default;
        public override void Gear(Character character)
        {
            //character.Inventory.ReceiveSkillReward(8205040); //fitness passive
            //character.Inventory.ReceiveSkillReward(8205030); //steady arm passive
            character.Inventory.ReceiveItemReward(3000221, 1, true); //ash head
            character.Inventory.ReceiveItemReward(3000220, 1, true); //ash chest
            character.Inventory.ReceiveItemReward(3000222, 1, true); //ash legs
            character.Inventory.ReceiveItemReward(2110000, 1, true); //brutal greataxe
        }
        public override bool HasQuest => true;
        public override string QuestName => "Giant Mistake";

        const string LogSignature_A = "giant.objective.a";
        const string LogSignature_B = "giant.objective.b";
        const string LogSignature_C = "giant.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "The giants have disowned you! Run for your life!"
            },
            {
                LogSignature_B,
                "Find a new home..."
            },
            {
                LogSignature_C,
                "You are ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedGiantRisenStart;

        public override void Init()
        {
            base.Init();

            QE_FixedGiantRisenStart = CustomQuests.CreateQuestEvent("iggythemad.giantrisen.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
        }

        public override void OnScenarioChosen()
        {

        }

        public override void OnScenarioChosen(Character character)
        {

        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();

            VanillaQuestsHelper.AddQuestEvent(VanillaQuestsHelper.ashFight);

            // Add 1 to our tracker event stack. Next scene load we will reset the quest events.
            //QuestEventManager.Instance.AddEvent(QE_FixedGiantRisenStart);
        }

        public override void OnStartSpawn(Character character)
        {
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

        // We call this on scene loads.
        public override void UpdateQuestProgress(Quest quest)
        {
            // Do nothing if we are not the host.
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            int maxStacks = 3;
            //Character host = CharacterManager.Instance.GetWorldHostCharacter();
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedGiantRisenStart.EventUID);
            QuestProgress progress = quest.m_questProgress;
            if (stack >= maxStacks)
            {
                progress.DisableQuest(QuestProgress.ProgressState.Successful);
                return;
            }

            if (stack < 1)
            {
                // Update the first log no matter what. It's completed if our stack is 2 or higher.
                QuestEventManager.Instance.AddEvent(QE_FixedGiantRisenStart, 1);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("I've been exiled! I should flee!");
            }

            // If we reached 2, remove the giant quest events and add the second log.
            else if (stack < 2)
            {
                // Second log
                QuestEventManager.Instance.AddEvent(QE_FixedGiantRisenStart, 1);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), false);
                ShowUIMessage("I need to find a new home...");

            }

            else if (stack < 3 && AreaManager.Instance.GetIsCurrentAreaTownOrCity() == true)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedGiantRisenStart, 1);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.RemoveEvent(VanillaQuestsHelper.ashWarp);
                VanillaQuestsHelper.RemoveEvent(VanillaQuestsHelper.ashFight);
                VanillaQuestsHelper.RemoveEvent(VanillaQuestsHelper.ashAllyFail);
                VanillaQuestsHelper.RemoveEvent(VanillaQuestsHelper.ashCompleteFail);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
                ShowUIMessage("Maybe I should join a new faction...");
            }
            ReloadLogs(stack, progress);
        }

        public void ReloadLogs(int stack, QuestProgress progress)
        {
            int stackCounter = 0;
            foreach (KeyValuePair<string, string> entry in QuestLogSignatures)
            {
                if (stackCounter < stack && entry.Key != null)
                {
                    // do something with entry.Value or entry.Key
                    Debug.Log("log: " + entry.Key);
                    progress.UpdateLogEntry(progress.GetLogSignature(entry.Key), false);
                    stackCounter += 1;
                }
            }
        }

        #region PassiveEffects

        internal static GiantRisenScenario Instance { get; private set; }
        public GiantRisenScenario()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(Character), "DodgeInput", new Type[] { typeof(Vector3) })]
        public class Character_DodgeInput
        {
            [HarmonyPostfix]
            public static void Postfix(Character __instance, Vector3 _direction)
            {
                if (__instance.IsLocalPlayer && __instance.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.GiantRisen) && !__instance.DodgeRestricted)
                {
                    Plugin.Instance.StartCoroutine(Instance.DodgeSlower(__instance));
                }
            }
        }

        public IEnumerator DodgeSlower(Character _character)
        {

            //yield return new WaitForSeconds(0.1f);
            if (_character.Dodging == true)
            {
                _character.Animator.speed = 0.6f;
                while (_character.Dodging == true)
                {
                    yield return new WaitForSeconds(0.2f);
                }
                yield return new WaitForSeconds(0.2f);
                _character.Animator.speed = 1f;
            }
        }
        #endregion
    }
}
