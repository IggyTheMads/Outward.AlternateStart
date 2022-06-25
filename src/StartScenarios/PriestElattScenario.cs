using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace AlternateStart.StartScenarios
{
    public class PriestElattScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_PriestElatt;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioPassives Area => ScenarioPassives.PriestElatt;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.Monsoon;
        public override Vector3 SpawnPosition => new(-175.2f, -1514.6f, 751.9f);
        public override Vector3 SpawnRotation => new(0, 359f, 0);

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Stats.FullStamina();
            character.Inventory.ReceiveSkillReward(8100250); //chakram arc
            character.Inventory.ReceiveItemReward(9000010, 190, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000071, 1, true); //apprentice helm
            character.Inventory.ReceiveItemReward(3000070, 1, true); //apprentice chest
            character.Inventory.ReceiveItemReward(3000174, 1, true); //sandals
            character.Inventory.ReceiveItemReward(2020010, 1, false); //mace
            character.Inventory.ReceiveItemReward(5110030, 1, true); //chakram
        }

        public override void OnStartSpawn()
        {
        }

        public override void OnStartSpawn(Character character)
        {
        }

        public override void UpdateQuestProgress(Quest quest)
        {

        }

        #region PassiveEffects

        float holyNerf = 0.25f;

        internal static PriestElattScenario Instance { get; private set; }
        public PriestElattScenario()
        {
            Instance = this;
        }

        //Change Damage Type
        [HarmonyPatch(typeof(Character), "ReceiveHit", new Type[] { typeof(UnityEngine.Object), typeof(DamageList), typeof(Vector3), typeof(Vector3), typeof(float), typeof(float), typeof(Character), typeof(float), typeof(bool) })]
        public class Character_ReceiveHit
        {
            [HarmonyPrefix]
            public static void Prefix(Character __instance, UnityEngine.Object _damageSource, ref DamageList _damage, Vector3 _hitDir, Vector3 _hitPoint, float _angle, float _angleDir, Character _dealerChar, float _knockBack, bool _hitInventory)
            {
                if (__instance == null || _dealerChar == null) { return; }

                if (_dealerChar.IsLocalPlayer)
                {
                    if (_dealerChar.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.PriestElatt))
                    {
                        int intType;

                        var dmgType = DamageType.Types.Electric;
                        intType = 3;
                        float totaldmg = _damage.TotalDamage;
                        float ampDmg = totaldmg * _dealerChar.Stats.m_totalDamageAttack[intType];
                        ampDmg -= ampDmg * Instance.holyNerf;
                        //Debug.Log("pre: " + totaldmg + " ------ post: " + ampDmg);
                        _damage.Clear();
                        _damage.Add(new DamageType(dmgType, ampDmg));
                    }
                }
            }
        }

        #endregion

    }
}
