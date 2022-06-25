using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;

namespace AlternateStart.StartScenarios
{
    public class ClaustroScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_Claustro;
        public override ScenarioDifficulty Difficulty => ScenarioDifficulty.Easy;
        public override ScenarioPassives Area => ScenarioPassives.Claustrophobic;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.EmercarDungeonsSmall;
        public override Vector3 SpawnPosition => new(1499.7f, -8.9f, 54.7f);
        public override Vector3 SpawnRotation => new(0, 99.8f, 0);

        public override bool HasQuest => false;
        public override string QuestName => "";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {

        };

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false);
        }

        public override void OnScenarioChosen(Character character)
        {
            character.Stats.FullStamina();
            //character.Inventory.ReceiveItemReward(9000010, 1, false); //no gold
            character.Inventory.ReceiveItemReward(3000081, 1, true); //chest worker
            character.Inventory.ReceiveItemReward(3000083, 1, true); //legs worker
            character.Inventory.ReceiveItemReward(3000086, 1, true); //helm worker
        }

        public override void OnStartSpawn()
        {
            CharacterManager.Instance.GetWorldHostCharacter().Inventory.QuestKnowledge.ReceiveQuest(VanillaQuestsHelper.enrollmentQ);
        }

        public override void OnStartSpawn(Character character)
        {
        }

        public override void UpdateQuestProgress(Quest quest)
        {

        }

        string claustroEffectID = "claustroEffect";
        internal static ClaustroScenario Instance { get; private set; }
        public ClaustroScenario()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(CharacterInventory), "EquipItem", new Type[] { typeof(Equipment), typeof(bool) })]
        public class CharacterInventory_EquipItem
        {
            [HarmonyPrefix]
            public static void Postfix(CharacterInventory __instance, Equipment _itemToEquip, bool _playAnim, ref Character ___m_character)
            {
                Character player = ___m_character;
                if (___m_character == null || !___m_character.IsLocalPlayer) { return; }

                if (player.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Claustrophobic))
                {
                    Plugin.Instance.StartCoroutine(Instance.checkHelmet(player));
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
                if (___m_character == null || !___m_character.IsLocalPlayer) { return; }

                if (player.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Claustrophobic))
                {
                    Plugin.Instance.StartCoroutine(Instance.checkHelmet(player));
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

                if (player.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Claustrophobic))
                {
                    Plugin.Instance.StartCoroutine(Instance.checkHelmet(player));
                }
            }
        }

        IEnumerator checkHelmet(Character player)
        {
            player.StatusEffectMngr.RemoveStatusWithIdentifierName(claustroEffectID);
            yield return new WaitForSeconds(0.5f);
            //if(player.Inventory.Equipment.IsEquipmentSlotEmpty(EquipmentSlot.EquipmentSlotIDs.Helmet))
            player.StatusEffectMngr.AddStatusEffect(claustroEffectID);
        }
    }
}
