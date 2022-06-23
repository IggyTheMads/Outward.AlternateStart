using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using SideLoader;
using HarmonyLib;

namespace AlternateStart.StartScenarios
{
    public class VendavelSlaveScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_VendavelSlave;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioPassives Area => ScenarioPassives.VendavelSlave;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.ChersoDungeon1;
        public override Vector3 SpawnPosition => new(-4.9f, -10f, 26.9f);
        public override Vector3 SpawnRotation => new(0, 356.3f, 0);

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };


        //variables
        public string enemyID = "com.iggy.vendavelbandit";
        public Vector3 prisonJump = new(43f, -9.7f, 34.7f);
        public Vector3 prisonDoors = new(15f, -10f, 4.4f);

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.StartHouseTimer();
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Stats.IncreaseBurntHealth(200, 1);
            //character.Inventory.ReceiveItemReward(3000133, 1, true); //beggarB head
            //character.Inventory.ReceiveItemReward(3000130, 1, true); //beggarB chest
            //character.Inventory.ReceiveItemReward(3000136, 1, true); //beggarB legs
        }

        public override void OnStartSpawn()
        {

            VanillaQuestsHelper.StartHouseTimer();

            //SL_Character myChar = SL.GetSLPack("iggythemad AlternateStart").CharacterTemplates[enemyID];
            //myChar.Spawn(prisonJump, Vector3.back, UID.Generate());

        }

        public override void OnStartSpawn(Character character)
        {
            character.Stats.IncreaseBurntHealth(200, 1);
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            ////////////////////////
            ///
            /// ADD start guards hostile event if killing the armored guy
            /// ADD Dying OR leaving the fortress through the hole, removes the armor obtained
            /// 
            ////////////////////////
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
