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
    public class EnmerkarHunterScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_EmercarHunter;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Hard;
        public override ScenarioPassives Area => ScenarioPassives.EnmerkarHunter;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.EmercarDungeonsSmall;
        public override Vector3 SpawnPosition => new(600.6f, 0.8f, 8.1f);
        public override Vector3 SpawnRotation => new(0, 29.3f, 0);

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
            character.Inventory.ReceiveSkillReward(8205160); //slow metabolism
            character.Inventory.ReceiveItemReward(9000010, 13, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000020, 1, true); //adventurer armor
            character.Inventory.ReceiveItemReward(3000022, 1, true); //adventurer boots
            character.Inventory.ReceiveItemReward(2200000, 1, false); //bow
            character.Inventory.ReceiveItemReward(5200001, 30, true); //arrows
            character.Inventory.ReceiveItemReward(4000060, 4, false); //meat
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

        internal static EnmerkarHunterScenario Instance { get; private set; }
        public EnmerkarHunterScenario()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(LocalCharacterControl), "DetectMovementInputs")]
        public class LocalCharacterControl_DetectMovementInputs
        {
            [HarmonyPostfix]
            public static void Postfix(LocalCharacterControl __instance, ref Character ___m_character)
            {
                if (__instance == null) { return; }
                if (!__instance.Character.IsLocalPlayer) { return; }

                if (__instance.Character.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.EnmerkarHunter))
                {
                    if (__instance.Character.CurrentlyChargingAttack)
                    {
                        __instance.m_moveInput *= new Vector2(0.3f, 0.3f);
                        __instance.m_modifMoveInput *= new Vector2(0.6f, 0.6f);
                        Plugin.Instance.StartCoroutine(Instance.checkBowDraw(__instance.Character));
                        return;
                    }
                }
                return;
            }
        }

        IEnumerator checkBowDraw(Character player)
        {
            player.Animator.speed = 2.2f;
            while(player.CurrentlyChargingAttack)
            {
                yield return new WaitForSeconds(0.1f);
            }
            player.Animator.speed = 1f;

        }

        [HarmonyPatch(typeof(CharacterInventory), "EquipItem", new Type[] { typeof(Equipment), typeof(bool) })]
        public class CharacterInventory_EquipItem
        {
            [HarmonyPrefix]
            public static void Postfix(CharacterInventory __instance, Equipment _itemToEquip, bool _playAnim, ref Character ___m_character)
            {
                Character player = ___m_character;
                if (player == null || !player.IsLocalPlayer) { return; }

                if (player.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.EnmerkarHunter))
                {
                    Plugin.Instance.StartCoroutine(Instance.checkBow(player));
                }
            }
        }

        [HarmonyPatch(typeof(CharacterInventory), "UnequipItem", new Type[] { typeof(Equipment), typeof(bool) })]
        public class CharacterInventory_UnequipItem
        {
            [HarmonyPrefix]
            public static void Postfix(CharacterInventory __instance, Equipment _itemToUnequip, bool _playAnim, ref Character ___m_character)
            {
                Character player = ___m_character;
                if (player == null || !player.IsLocalPlayer) { return; }

                if (player.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.EnmerkarHunter))
                {
                    Plugin.Instance.StartCoroutine(Instance.checkBow(player));
                }
            }
        }

        [HarmonyPatch(typeof(CharacterKnowledge), "AddItem")]
        public class CharacterKnowledge_AddItem
        {
            [HarmonyPrefix]
            public static void Prefix(CharacterKnowledge __instance, Item _item, ref Character ___m_character)
            {
                var player = ___m_character;
                if (player == null) { return; }
                if (!player.IsLocalPlayer) { return; }

                if (player.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.EnmerkarHunter))
                {
                    Plugin.Instance.StartCoroutine(Instance.checkBow(player));
                }
            }
        }

        string bowmanEffect = "bowmanEffect";

        IEnumerator checkBow(Character player)
        {
            player.StatusEffectMngr.RemoveStatusWithIdentifierName(bowmanEffect);
            yield return new WaitForSeconds(0.5f);
            if(player.CurrentWeapon.Type != Weapon.WeaponType.Bow)
            {
                player.StatusEffectMngr.AddStatusEffect(bowmanEffect);
            }
        }
    }
}
