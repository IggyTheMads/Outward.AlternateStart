using HarmonyLib;
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
    public class ChosenOneScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_ChosenOne;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.ChosenOne;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.CierzoOutside; //NEED GEAR UPDATE
        public override Vector3 SpawnPosition => new(865.8f, 212.7f, 880.1f);
        public override Vector3 SpawnRotation => new(0, 225.7f, 0);
        public override void Gear(Character character)
        {
            character.Inventory.AddMoney(27);
            character.Inventory.ReceiveItemReward(4100550, 5, false); //rations
            character.Inventory.ReceiveItemReward(3100006, 1, true); //halfp chest
            character.Inventory.ReceiveItemReward(3100008, 1, true); //halfp legs
            character.Inventory.ReceiveItemReward(2000010, 1, false); //iron sword
            character.Inventory.ReceiveItemReward(5100500, 1, true); //lexicon
        }
        public override bool HasQuest => true;
        public override string QuestName => "Angelic Descend";

        const string LogSignature_A = "chosen.objective.a";
        const string LogSignature_B = "chosen.objective.b";
        const string LogSignature_C = "chosen.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Visit any city. Learn about the factions and their conflicts. Save Aurai."
            },
            {
                LogSignature_B,
                "You've learned about the factions."
            },
            {
                LogSignature_C,
                "You are ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedLimitlessStart;

        public override void Init()
        {
            base.Init();

            QE_FixedLimitlessStart = CustomQuests.CreateQuestEvent("iggythemad.chosenone.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

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
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedLimitlessStart.EventUID);
            QuestProgress progress = quest.m_questProgress;

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedLimitlessStart, 1);
                stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedLimitlessStart.EventUID);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("The world needs me. I should find about the troubles of Aurai...");
            }
            else if (stack < 2 && AreaManager.Instance.GetIsCurrentAreaTownOrCity() == true)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedLimitlessStart, 2);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
                ShowUIMessage("Time to learn about the people of Aurai. I will join them!");

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
            character.SpellCastAnim(Character.SpellCastType.Sit, Character.SpellCastModifier.Immobilized, 0);
        }

        //TESTING
        /*

        static string ChosenOneNerf = "ChosenOneNerf";
        static int maxBreakthroughs = 4;

        /////////////////TESTING
        [HarmonyPatch(typeof(PlayerCharacterStats), "get_RemainingBreakthrough")]
        class PlayerCharacterStats_RemainingBreakthrough
        {
            public static bool Prefix(ref int __result, PlayerCharacterStats __instance)
            {
                if (__instance.m_character.IsLocalPlayer && __instance.m_character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.ChosenOne))
                {
                    __result = Mathf.Clamp(maxBreakthroughs - __instance.m_usedBreakthroughCount, 0, 20);
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
                if(!_character.IsLocalPlayer || !_character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.ChosenOne)) { return; }
                if (__instance.IsBreakthrough)
                {
                    _character.StatusEffectMngr.AddStatusEffect(ChosenOneNerf);
                }
            }
        }*/
    }
}
