﻿using SideLoader;
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
    public class VendavelSlaveScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_VendavelSlave;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.VendavelSlave;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon1;
        public override Vector3 SpawnPosition => new(-4.9f, -10f, 26.9f);
        public override Vector3 SpawnRotation => new(0, 356.3f, 0);
        public override void Gear(Character character)
        {

        }
        public override bool HasQuest => true;
        public override string QuestName => "Sweet Freedom";

        const string LogSignature_A = "slave.objective.a";
        const string LogSignature_B = "slave.objective.b";
        const string LogSignature_C = "slave.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Find a way out. Freedom."
            },
            {
                LogSignature_B,
                "Seek help in Cierzo village."
            },
            {
                LogSignature_C,
                "You are safe in Cierzo."
            }
        };

        private QuestEventSignature QE_FixedSlaveStart;

        public override void Init()
        {
            base.Init();

            QE_FixedSlaveStart = CustomQuests.CreateQuestEvent("iggythemad.slave.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

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

            int maxStacks = 3;
            //Character host = CharacterManager.Instance.GetWorldHostCharacter();
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedSlaveStart.EventUID);
            QuestProgress progress = quest.m_questProgress;
            if (stack >= maxStacks)
            {
                progress.DisableQuest(QuestProgress.ProgressState.Successful);
                return;
            }

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedSlaveStart, 1);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("I can't take this anymore...");
            }
            else if (stack < 2 && SceneManagerHelper.ActiveSceneName == "ChersoneseNewTerrain")
            {
                // Second log
                QuestEventManager.Instance.AddEvent(QE_FixedSlaveStart, 1);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), false);
                ShowUIMessage("I should seek help in Cierzo...");
                VanillaQuestsHelper.DestroyCierzo(false, false);
                //VanillaQuestsHelper.AddQuestEvent(VanillaQuestsHelper.factionCommit);

            }
            else if (stack < 3 && SceneManagerHelper.ActiveSceneName == "CierzoNewTerrain")
            {
                QuestEventManager.Instance.AddEvent(QE_FixedSlaveStart, 2);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, false);
                ShowUIMessage("I should ask around...");

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

        //variables
        public string enemyID = "com.iggy.vendavelbandit";
        public Vector3 prisonJump = new(43f, -9.7f, 34.7f);
        public Vector3 prisonDoors = new(15f, -10f, 4.4f);

        public override void OnScenarioChosen()
        {
            //VanillaQuestsHelper.SkipHostToFactionChoice(false);
            
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Stats.IncreaseBurntHealth(200, 1);
        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();

            //SL_Character myChar = SL.GetSLPack("iggythemad AlternateStart").CharacterTemplates[enemyID];
            //myChar.Spawn(prisonJump, Vector3.back, UID.Generate());
        }

        public override void OnStartSpawn(Character character)
        {
            character.Stats.IncreaseBurntHealth(200, 1);
        }

        internal static VendavelSlaveScenario Instance { get; private set; }
        public VendavelSlaveScenario()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(Character), "ReceiveHit", new Type[] { typeof(UnityEngine.Object), typeof(DamageList), typeof(Vector3), typeof(Vector3), typeof(float), typeof(float), typeof(Character), typeof(float), typeof(bool) })]
        public class Character_ReceiveHit
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance, UnityEngine.Object _damageSource, DamageList _damage, Vector3 _hitDir, Vector3 _hitPoint, float _angle, float _angleDir, Character _dealerChar, ref float _knockBack, bool _hitInventory)
            {
                if (__instance == null) { return; }
                if (!__instance.IsLocalPlayer || _dealerChar == null) { return; }

                if (__instance.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.VendavelSlave))
                {
                    if (_damage[0].Damage > 1)
                    {
                        if (__instance.StatusEffectMngr.HasStatusEffect("Bleeding"))
                        {
                            __instance.StatusEffectMngr.AddStatusEffectBuildUp("Bleeding +", _damage[0].Damage, _dealerChar);
                        }
                        else
                        {
                            __instance.StatusEffectMngr.AddStatusEffectBuildUp("Bleeding", _damage[0].Damage * 2f, _dealerChar);
                        }
                    }
                }
            }
        }
    }
}
