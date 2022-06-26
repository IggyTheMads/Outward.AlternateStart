﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using SideLoader;
using SideLoader.Managers;

namespace AlternateStart.StartScenarios
{
    public class CorruptedScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_CorruptedSoul;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioPassives Area => ScenarioPassives.CorruptedSoul;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon6; 
        public override Vector3 SpawnPosition => new(-37.3f, -7f, -126.4f);
        public override Vector3 SpawnRotation => new(0, 222.9f, 0);

        public override bool HasQuest => true;
        public override string QuestName => "Corruption Reborn";

        const string LogSignature_A = "corrupted.objective.a";
        const string LogSignature_B = "corrupted.objective.b";
        const string LogSignature_C = "corrupted.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Whatever happend to you. Find a way out. "
            },
            {
                LogSignature_B,
                "Head to a nearby settlement"
            },
            {
                LogSignature_C,
                "You are ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedCorruptedStart;

        public override void Init()
        {
            base.Init();

            QE_FixedCorruptedStart = CustomQuests.CreateQuestEvent("iggythemad.corrupted.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
        }

        private void SL_OnGameplayResumedAfterLoading()
        {
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            foreach (Character player in CharacterManager.Instance.Characters.Values)
            {
                if (player.IsLocalPlayer && player.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.CorruptedSoul))
                {
                    if (player.PlayerStats.Corruption < 900)
                    {
                        player.PlayerStats.AffectCorruptionLevel(900, false);
                    }
                }
            }

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
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedCorruptedStart.EventUID);
            QuestProgress progress = quest.m_questProgress;

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedCorruptedStart, 1);
                stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedCorruptedStart.EventUID);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("What... Where am I?");
            }
            else if (stack < 2)
            {
                // Second log
                QuestEventManager.Instance.AddEvent(QE_FixedCorruptedStart, 1);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), false);
                ShowUIMessage("I should find others... Living, preferably.");

            }
            else if (AreaManager.Instance.GetIsCurrentAreaTownOrCity() == true)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedCorruptedStart, 1);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
                ShowUIMessage("I'm somehow back to life. I should do something with it.");

            }
        }

        public override void OnScenarioChosen()
        {
            //VanillaQuestsHelper.StartHouseTimer();
        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();
        }

        public override void OnScenarioChosen(Character character)
        {
            //character.Inventory.ReceiveItemReward(9000010, 53, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000044, 1, true); //jade acolyte robes
            character.Inventory.ReceiveItemReward(3000046, 1, true); //jade acolyte boots
            character.Inventory.ReceiveItemReward(2150001, 1, false); //mage stick
            character.Inventory.ReceiveItemReward(5100060, 1, true); //torch

            //character.Inventory.ReceiveItemReward(4400010, 3, false); //bandage
        }

        public override void OnStartSpawn(Character character)
        {
        }

        #region PassiveEffects

        internal static CorruptedScenario Instance { get; private set; }
        public CorruptedScenario()
        {
            Instance = this;
        }

        //NEEDS TO BE OPTIMIZED
        [HarmonyPatch(typeof(CharacterStats), "UseStamina", new Type[] { typeof(float), typeof(float) })]
        public class Character_UseStamina
        {
            [HarmonyPrefix]
            public static void Prefix(CharacterStats __instance, float _staminaConsumed, float _burnRatioModifier, ref Character ___m_character)
            {
                if (__instance == null) { return; }
                if (!__instance.m_character.IsLocalPlayer) { return; }

                if (__instance.m_character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.CorruptedSoul))
                {
                    if (__instance.m_character.PlayerStats.Corruption < 900)
                    {
                        __instance.m_character.PlayerStats.AffectCorruptionLevel(900, false);
                    }
                    /*if(__instance.m_character.StatusEffectMngr.HasStatusEffect("Bleeding"))
                    {

                    }*/
                }
            }
        }
        #endregion
    }
}
