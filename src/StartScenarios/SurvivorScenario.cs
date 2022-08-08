using System;
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
    public class SurvivorScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_Survivor;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.Survivor;
        public override void Gear(Character character)
        {
            character.Inventory.ReceiveItemReward(4100550, 4, false); //rations
            character.Inventory.ReceiveItemReward(3000240, 1, true); //fur chest
            character.Inventory.ReceiveItemReward(3000242, 1, true); //fur legs
            character.Inventory.ReceiveItemReward(2160020, 1, true); //cloth fists
            //character.Inventory.ReceiveItemReward(4400010, 3, false); //bandage
        }
        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.CierzoOutside;
        public override Vector3 SpawnPosition => new(133.7f, 33.4f, 1456.8f);
        public override Vector3 SpawnRotation => new(0, 68.4f, 0);

        public override bool HasQuest => true;
        public override string QuestName => "Lone Survivor";

        const string LogSignature_A = "survivor.objective.a";
        const string LogSignature_B = "survivor.objective.b";
        const string LogSignature_C = "survivor.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Travel to Berg and search for refugees."
            },
            {
                LogSignature_B,
                "Ask the kazites if they saw any other survivors."
            },
            {
                LogSignature_C,
                "You found Eto. You are now ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedSurvivorStart;

        public override void Init()
        {
            base.Init();

            QE_FixedSurvivorStart = CustomQuests.CreateQuestEvent("iggythemad.survivor.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

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
            Character host = CharacterManager.Instance.GetWorldHostCharacter();
            // Each scene load we add 1 to this quest event stack, until it reaches 3.
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedSurvivorStart.EventUID);
            QuestProgress progress = quest.m_questProgress;
            if (stack >= maxStacks)
            {
                progress.DisableQuest(QuestProgress.ProgressState.Successful);
                return;
            }
            //if(stack >= maxStacks) { return; }

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedSurvivorStart, 1);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("I barely escaped... I hope others made it...");
            }
            if (stack < 2 && SceneManagerHelper.ActiveSceneName == "Berg")
            {
                // Second log
                QuestEventManager.Instance.AddEvent(QE_FixedSurvivorStart, 1);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), false);
                ShowUIMessage("I should ask the kazites. They might've seen more survivors.");

            }
            if (stack < 3 && talkEto == true)
            {
                talkEto = false;
                QuestEventManager.Instance.AddEvent(QE_FixedSurvivorStart, 2);
                //QuestEventManager.Instance.SetQuestEventStack(QE_FixedSurvivorStart.EventUID, 3, true); //force stacks?

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
                VanillaQuestsHelper.DestroyCierzo(true, true);
                ShowUIMessage("I will get my vengeance... Eventually.");
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
            VanillaQuestsHelper.DestroyCierzo(true, false);
        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();
        }

        public override void OnScenarioChosen(Character character)
        {

        }

        public override void OnStartSpawn(Character character)
        {
            /*var pack = SL.GetSLPack("iggythemad AlternateStart");

            bergSpellblade = pack.CharacterTemplates["com.iggy.berg.spellblade.trainer"];
            bergSpellblade.OnSpawn += SpellbladeSetup;*/
        }

        Vector3 SpellbladeSpawn = new Vector3(1284.4f, -3.7f, 1622.2f);
        bool talkEto = false;

        [HarmonyPatch(typeof(InteractionTriggerBase), "TryActivateBasicAction", new Type[] { typeof(Character), typeof(int) })]
        public class InteractionTriggerBase_TryActivateBasicAction
        {
            [HarmonyPrefix]
            public static void Prefix(InteractionTriggerBase __instance, Character _character, int _toggleState)
            {
                if (!Instance.IsActiveScenario
                    || PhotonNetwork.isNonMasterClientInRoom
                    || !_character.IsLocalPlayer
                    || SceneManagerHelper.ActiveSceneName != "Berg")
                    return;

                if (__instance.CurrentTriggerManager as InteractionActivator == true)
                {
                    InteractionActivator activator = __instance.CurrentTriggerManager as InteractionActivator;
                    if (activator.BasicInteraction != null)
                    {
                        IInteraction interaction = activator.BasicInteraction;
                        if (interaction is InteractionTrainerDialogue)
                        {

                            if((Vector3.Distance(_character.CenterPosition, Instance.SpellbladeSpawn) < 2f))
                            {
                                Quest quest = _character.Inventory.QuestKnowledge.GetItemFromItemID((int)Instance.Type) as Quest;
                                int stack = QuestEventManager.Instance.GetEventCurrentStack(Instance.QE_FixedSurvivorStart.EventUID);
                                //Debug.Log("isTrainerDialogue");
                                if (stack < 3)
                                {
                                    Instance.talkEto = true;
                                    Instance.UpdateQuestProgress(quest);
                                }
                            }
                        }
                    }
                }
            }
        }

        #region PassiveEffects

        string rageID = "Rage";
        string adrenalineSpeedID = "AdrenalineSpeed";

        float injuredBonus = 0.3f;

        internal static SurvivorScenario Instance { get; private set; }
        //internal static SL_Character bergSpellblade;

        public SurvivorScenario()
        {
            Instance = this;

            //TrainerManager.bergSpellblade.ShouldSpawn = () => this.IsActiveScenario;
        }


        [HarmonyPatch(typeof(Character), "VitalityHit")]
        public class Character_VitalityHitPre
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance, Character _dealerChar, ref float _damage, Vector3 _hitVector)
            {
                if (_dealerChar != null && _dealerChar.IsLocalPlayer && _dealerChar.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Survivor))
                {
                    float percentHP = (1 / _dealerChar.Stats.MaxHealth) * _dealerChar.Stats.CurrentHealth;
                    if (percentHP <= 0.75f)
                    {
                        _damage += _damage * Instance.injuredBonus;
                    }
                    /*else
                    {
                        _damage -= _damage * 0.2f;
                    }*/
                }
            }
        }

        [HarmonyPatch(typeof(Character), "VitalityHit")]
        public class Character_VitalityHit
        {
            [HarmonyPostfix]
            public static void Postfix(Character __instance, Character _dealerChar, ref float _damage, Vector3 _hitVector)
            {
                if (__instance.IsLocalPlayer && __instance.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Survivor))
                {
                    float percentHP = (1 / __instance.Stats.MaxHealth) * __instance.Stats.CurrentHealth;
                    if (percentHP <= 0.5f)
                    {
                        if (!__instance.StatusEffectMngr.HasStatusEffect(Instance.rageID))
                        {
                            __instance.StatusEffectMngr.AddStatusEffect(Instance.rageID);
                        }
                        if (percentHP <= 0.3f)
                        {
                            __instance.StatusEffectMngr.AddStatusEffect(Instance.adrenalineSpeedID);
                        }
                    }
                    else
                    {
                        /*if (__instance.StatusEffectMngr.HasStatusEffect(Instance.rageID))
                        {
                            __instance.StatusEffectMngr.RemoveStatusWithIdentifierName(Instance.rageID);
                        }*/
                        if (percentHP > 0.3f)
                        {
                            if (__instance.StatusEffectMngr.HasStatusEffect(Instance.adrenalineSpeedID))
                            {
                                __instance.StatusEffectMngr.RemoveStatusWithIdentifierName(Instance.adrenalineSpeedID);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(CharacterStats), "AffectHealth")]
        public class CharacterStats_AffectHealth
        {
            [HarmonyPostfix]
            public static void Postfix(CharacterStats __instance, ref float _quantity)
            {
                if (!__instance.m_character.IsLocalPlayer) { return; }

                if (__instance.m_character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Survivor))
                {
                    float percentHP = (1 / __instance.MaxHealth) * __instance.CurrentHealth;
                    if (percentHP <= 0.5f)
                    {
                        if (!__instance.m_character.StatusEffectMngr.HasStatusEffect("Rage"))
                        {
                            __instance.m_character.StatusEffectMngr.AddStatusEffect("Rage");
                        }
                        if (percentHP <= 0.3f)
                        {
                            __instance.m_character.StatusEffectMngr.AddStatusEffect(Instance.adrenalineSpeedID);
                        }
                    }
                    else
                    {
                        if (__instance.m_character.StatusEffectMngr.HasStatusEffect("Rage"))
                        {
                            __instance.m_character.StatusEffectMngr.RemoveStatusWithIdentifierName("Rage");
                        }
                        if (percentHP > 0.3f)
                        {
                            if (__instance.m_character.StatusEffectMngr.HasStatusEffect(Instance.adrenalineSpeedID))
                            {
                                __instance.m_character.StatusEffectMngr.RemoveStatusWithIdentifierName(Instance.adrenalineSpeedID);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
