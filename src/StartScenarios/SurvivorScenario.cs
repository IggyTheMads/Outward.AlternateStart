using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace AlternateStart.StartScenarios
{
    public class SurvivorScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_Survivor;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioPassives Area => ScenarioPassives.Survivor;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.CierzoOutside;
        public override Vector3 SpawnPosition => new(133.7f, 33.4f, 1456.8f);
        public override Vector3 SpawnRotation => new(0, 68.4f, 0);

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false);
            VanillaQuestsHelper.DestroyCierzo(true);
        }

        public override void OnStartSpawn()
        {
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Inventory.ReceiveItemReward(4100550, 4, false); //rations
            character.Inventory.ReceiveItemReward(3000240, 1, true); //fur chest
            character.Inventory.ReceiveItemReward(3000242, 1, true); //fur legs
            character.Inventory.ReceiveItemReward(2160020, 1, true); //cloth fists
            //character.Inventory.ReceiveItemReward(4400010, 3, false); //bandage
        }

        public override void OnStartSpawn(Character character)
        {
        }

        public override void UpdateQuestProgress(Quest quest)
        {

        }


        #region PassiveEffects

        string rageID = "Rage";
        string adrenalineSpeedID = "AdrenalineSpeed";

        float injuredBonus = 0.3f;

        internal static SurvivorScenario Instance { get; private set; }
        public SurvivorScenario()
        {
            Instance = this;
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
                        if (__instance.StatusEffectMngr.HasStatusEffect(Instance.rageID))
                        {
                            __instance.StatusEffectMngr.RemoveStatusWithIdentifierName(Instance.rageID);
                        }
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
