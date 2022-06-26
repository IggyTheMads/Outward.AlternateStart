using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using SideLoader.Managers;
using SideLoader;

namespace AlternateStart.StartScenarios
{
    public class WolfgangScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_WolfgangMercenary;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioPassives Area => ScenarioPassives.WolfgangMercenary;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.AntiqueField;
        public override Vector3 SpawnPosition => new(125.8f, 30.8f, 742.8f);
        public override Vector3 SpawnRotation => new(0, 350.5f, 0);

        public override bool HasQuest => true;
        public override string QuestName => "New Contract";

        const string LogSignature_A = "wolfgang.objective.a";
        const string LogSignature_B = "wolfgang.objective.b";
        const string LogSignature_C = "wolfgang.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Visit any city to find a new purpose."
            },
            {
                LogSignature_B,
                "Learn about the factions"
            },
            {
                LogSignature_C,
                "You are ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedWolfgangStart;

        public override void Init()
        {
            base.Init();

            QE_FixedWolfgangStart = CustomQuests.CreateQuestEvent("iggythemad.wolfgang.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

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
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedWolfgangStart.EventUID);
            QuestProgress progress = quest.m_questProgress;

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedWolfgangStart, 1);
                stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedWolfgangStart.EventUID);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("No mercenary work... I need a new purpose.");
            }
            else if (stack < 2 && AreaManager.Instance.GetIsCurrentAreaTownOrCity() == true)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedWolfgangStart, 2);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
                ShowUIMessage("I could join a faction...");
            }
        }

        public override void OnScenarioChosen()
        {
            //VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Inventory.ReceiveItemReward(9000010, 11, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3100260, 1, true); //virgin chest
            character.Inventory.ReceiveItemReward(3100262, 1, true); //virgin legs
            character.Inventory.ReceiveItemReward(2140010, 1, true); //halbert cleaver
            character.Inventory.ReceiveItemReward(4300010, 2, false); //potion
        }

        public override void OnStartSpawn(Character character)
        {
        }


        #region PassiveEffects

        //float wolfgangMagicNerf = 0.4f;
        float wolfgangBonusPerStam = 0.5f;

        internal static WolfgangScenario Instance { get; private set; }
        public WolfgangScenario()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(Character), "ReceiveHit", new Type[] { typeof(UnityEngine.Object), typeof(DamageList), typeof(Vector3), typeof(Vector3), typeof(float), typeof(float), typeof(Character), typeof(float), typeof(bool) })]
        public class Character_ReceiveHit
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance, UnityEngine.Object _damageSource, ref DamageList _damage, Vector3 _hitDir, Vector3 _hitPoint, float _angle, float _angleDir, Character _dealerChar, ref float _knockBack, bool _hitInventory)
            {
                if (__instance == null || _dealerChar == null) { return; }

                if (_dealerChar.IsLocalPlayer)
                {
                    if (_dealerChar.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.WolfgangMercenary))
                    {
                        if (_damage[0] != null)
                        {
                            float baseDmg = _damage[0].Damage;
                            float multiplier = (_dealerChar.Stats.CurrentStamina * Instance.wolfgangBonusPerStam) / 100f;
                            _damage[0].Damage += baseDmg * multiplier;
                        }
                    }
                }
                /*else if (__instance.IsLocalPlayer)
                {
                    if (__instance.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.WolfgangMercenary))
                    {
                        for(int i = 1; i < _damage.Count; i++)
                        {
                            if(_damage[i] != null && _damage[i].Damage > 0)
                            {
                                float baseDmg = _damage[i].Damage;
                                _damage[i].Damage -= baseDmg * Instance.wolfgangMagicNerf;
                                Debug.Log(i + " :Receive dmg: " + _damage[0].Damage);
                            }
                        }
                    }
                }*/
            }
        }
        #endregion
    }
}
