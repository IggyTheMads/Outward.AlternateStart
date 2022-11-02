using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace AlternateStart.StartScenarios
{
    public class WarVeteranScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_Veteran;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.Veteran;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Levant;
        public override Vector3 SpawnPosition => new(-361.7f, -1504.6f, 571.4f);
        public override Vector3 SpawnRotation => new(0, 274f, 0);
        public override void Gear(Character character)
        {
            character.Inventory.ReceiveItemReward(9000010, 53, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000010, 1, true); //padded chest
            character.Inventory.ReceiveItemReward(3000012, 1, true); //padded legs
            character.Inventory.ReceiveItemReward(2000061, 1, true); //gold machete
            character.Inventory.ReceiveItemReward(4400010, 3, false); //bandage
        }
        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };

        public override void OnScenarioChosen()
        {
            //VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnStartSpawn()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnScenarioChosen(Character character)
        {

        }

        public override void OnStartSpawn(Character character)
        {
        }

        public override void UpdateQuestProgress(Quest quest)
        {

        }


        #region PassiveEffects

        internal static WarVeteranScenario Instance { get; private set; }
        public WarVeteranScenario()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(Character), "StabilityHit")]
        public class Character_StabilityHit
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance, ref float _knockValue, float _angle, bool _block, Character _dealerChar)
            {
                if (__instance == null) { return; }
                if (!__instance.IsLocalPlayer) { return; }

                if (__instance.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Veteran))
                {
                    //Debug.Log("BLOCK");
                    //lower stamina, more knockback received
                    float blockExtra = 1f;
                    if (__instance.Blocking) { blockExtra = 2f; }
                    float multiplier = ((1 / __instance.Stats.MaxStamina) * __instance.Stats.CurrentStamina);
                    _knockValue = _knockValue + ((_knockValue * (1 - multiplier) * blockExtra));
                }
            }
        }

        //DISABLED SPEED NERF
        /*[HarmonyPatch(typeof(LocalCharacterControl), "DetectMovementInputs")]
        public class LocalCharacterControl_DetectMovementInputs
        {
            [HarmonyPostfix]
            public static void Postfix(LocalCharacterControl __instance, ref Character ___m_character)
            {
                if (__instance == null) { return; }
                if (!__instance.Character.IsLocalPlayer) { return; }

                if (__instance.Character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Veteran))
                {
                    float minMov = 0.6f;
                    float multiplier = 1 / __instance.Character.Stats.MaxStamina * __instance.Character.Stats.CurrentStamina;
                    float slowedX = Mathf.Abs(__instance.m_moveInput.x * (minMov + (1f - minMov) * multiplier));
                    float slowedY = Mathf.Abs(__instance.m_moveInput.y * (minMov + (1f - minMov) * multiplier)); // Input * (min + (max - min) * ratio)

                    __instance.m_moveInput = (__instance.m_moveInput * new Vector2(slowedX, slowedY)).normalized;
                    __instance.m_modifMoveInput = (__instance.m_modifMoveInput * new Vector2(slowedX, slowedY)).normalized;
                }
                return;
            }
        }*/
        #endregion
    }
}
