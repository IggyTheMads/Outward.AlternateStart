using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

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

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.StartHouseTimer();
        }

        public override void OnStartSpawn()
        {
        }

        public override void OnScenarioChosen(Character character)
        {
            //character.Inventory.ReceiveItemReward(9000010, 53, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000044, 1, true); //jade acolyte robes
            character.Inventory.ReceiveItemReward(3000046, 1, true); //jade acolyte boots
            character.Inventory.ReceiveItemReward(2150001, 1, true); //fang cub
            //character.Inventory.ReceiveItemReward(4400010, 3, false); //bandage
        }

        public override void OnStartSpawn(Character character)
        {
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            
        }


        #region PassiveEffects

        internal static CorruptedScenario Instance { get; private set; }
        public CorruptedScenario()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(CharacterStats), "UseStamina", new Type[] { typeof(float), typeof(float) })]
        public class Character_UseStamina
        {
            [HarmonyPrefix]
            public static void Prefix(CharacterStats __instance, float _staminaConsumed, float _burnRatioModifier, ref Character ___m_character)
            {
                if (__instance == null) { return; }
                if (!__instance.m_character.IsLocalPlayer) { return; }

                if (__instance.m_character.PlayerStats.Corruption < 900)
                {
                    __instance.m_character.PlayerStats.AffectCorruptionLevel(900, false);
                }
                /*if(__instance.m_character.StatusEffectMngr.HasStatusEffect("Bleeding"))
                {

                }*/
            }
        }
        #endregion
        }
}
