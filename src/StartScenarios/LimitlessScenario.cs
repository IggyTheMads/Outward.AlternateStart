using HarmonyLib;
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
    public class LimitlessScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_Unbound;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.Unbound;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Emercar;
        public override Vector3 SpawnPosition => new(1053.8f, -32f, 1155.6f);
        public override Vector3 SpawnRotation => new(0, 230.2f, 0);
        public override void Gear(Character character)
        {
            character.Inventory.AddMoney(37);
            character.Inventory.ReceiveItemReward(4100550, 3, false); //festive chest
            character.Inventory.ReceiveItemReward(3000091, 1, true); //festive chest
            character.Inventory.ReceiveItemReward(3000282, 1, true); //leather legs
            character.Inventory.ReceiveItemReward(2160020, 1, false); //cloth fist
            character.Inventory.ReceiveItemReward(5110030, 1, false); //chakram
        }
        public override bool HasQuest => true;
        public override string QuestName => "Unbound. Unlimited.";

        const string LogSignature_A = "unbound.objective.a";
        const string LogSignature_B = "unbound.objective.b";
        const string LogSignature_C = "unbound.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "You've unlocked your true potential. You are unbound. Find any trainer to learn from."
            },
            {
                LogSignature_B,
                "Purchase your first skill. No restrictions. Choose wisely."
            },
            {
                LogSignature_C,
                "You are ready to face the world. You are ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedUnboundStart;

        public override void Init()
        {
            base.Init();

            QE_FixedUnboundStart = CustomQuests.CreateQuestEvent("iggythemad.unbound.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
            //TinyHelper.TinyHelper.OnDescriptionModified += LimitlessDescriptions;
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

        public int questTrigger = 0;
        public override void UpdateQuestProgress(Quest quest)
        {
            // Do nothing if we are not the host.
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            Character host = CharacterManager.Instance.GetWorldHostCharacter();
            // Each scene load we add 1 to this quest event stack, until it reaches 3.
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedUnboundStart.EventUID);
            QuestProgress progress = quest.m_questProgress;

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedUnboundStart, 1);
                stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedUnboundStart.EventUID);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("I feel... free. Unbound. I need power!");
            }
            else if (stack < 2 && AreaManager.Instance.GetIsCurrentAreaTownOrCity() == true)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedUnboundStart, 1);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
    
                ShowUIMessage("So many choices... I gotta choose wisely.");

            }
            else if(stack < 3 && questTrigger == 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedUnboundStart, 2);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
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

        public override void OnScenarioChosen()
        {
            //VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnScenarioChosen(Character character)
        {

        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();
        }

        public override void OnStartSpawn(Character character)
        {
        }

        static int maxLimitless = 15;
        internal static LimitlessScenario Instance { get; private set; }
        public LimitlessScenario()
        {
            Instance = this;

            //TrainerManager.bergSpellblade.ShouldSpawn = () => this.IsActiveScenario;
        }

        /////////////////TESTING
        [HarmonyPatch(typeof(BaseSkillSlot), "CheckCharacterRequirements")]
        class BaseSkillSlots_CheckCharacterRequirements
        {
            public static bool Prefix(ref bool __result, BaseSkillSlot __instance, Character _character, bool _notify = false)
            {
                if (_character.IsLocalPlayer && _character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Unbound)
                    && _character.PlayerStats.m_usedBreakthroughCount < maxLimitless)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(SkillSlot), "UnlockSkill", new Type[] { typeof(Character) })]
        public class SkillSlot_UnlockSkill
        {
            [HarmonyPostfix]
            public static void Postfix(SkillSlot __instance, Character _character)
            {
                if(SceneManagerHelper.ActiveSceneName == "DreamWorld") { return; }
                if(__instance.IsBreakthrough) { return; }
                if(_character.IsLocalPlayer && _character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Unbound))
                {
                    //add free exceptions TODO

                    if(__instance.RequiredMoney <= 25)
                    { return; }
                    else if(_character.PlayerStats.m_usedBreakthroughCount < maxLimitless)
                    {
                        _character.PlayerStats.UseBreakthrough();

                        Quest quest = _character.Inventory.QuestKnowledge.GetItemFromItemID((int)Instance.Type) as Quest;
                        Instance.UpdateQuestProgress(quest);
                    }
                    else
                    {
                        _character.CharacterUI.ShowInfoNotification("Ran out of skill points...");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerCharacterStats), "get_RemainingBreakthrough")]
        class PlayerCharacterStats_RemainingBreakthrough
        {
            public static bool Prefix(ref int __result, PlayerCharacterStats __instance)
            {
                if (__instance.m_character.IsLocalPlayer && __instance.m_character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Unbound))
                {
                    __result = Mathf.Clamp(maxLimitless - __instance.m_usedBreakthroughCount, 0, 20);
                    return false;
                }
                return true;
            }
        }

    }
}
