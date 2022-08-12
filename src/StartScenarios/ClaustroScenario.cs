using System;
using System.Collections;
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
    public class ClaustroScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_Claustro;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.Claustrophobic;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.EmercarDungeonsSmall;
        public override Vector3 SpawnPosition => new(1499.7f, -8.9f, 54.7f);
        public override Vector3 SpawnRotation => new(0, 99.8f, 0);
        public override void Gear(Character character)
        {
            character.Stats.FullStamina();
            //character.Inventory.ReceiveItemReward(9000010, 1, false); //no gold
            character.Inventory.ReceiveItemReward(3000081, 1, true); //chest worker
            character.Inventory.ReceiveItemReward(3000083, 1, true); //legs worker
            character.Inventory.ReceiveItemReward(3000086, 1, true); //helm worker
        }
        public override bool HasQuest => true;
        public override string QuestName => "Kidnap Miracle";

        const string LogSignature_A = "claustro.objective.a";
        const string LogSignature_B = "claustro.objective.b";
        const string LogSignature_C = "claustro.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Escape before your kidnappers return."
            },
            {
                LogSignature_B,
                "Reach somewhere safe. A city perhaps."
            },
            {
                LogSignature_C,
                "You are ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedClaustroStart;

        public override void Init()
        {
            base.Init();

            QE_FixedClaustroStart = CustomQuests.CreateQuestEvent("iggythemad.claustro.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

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
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedClaustroStart.EventUID);
            QuestProgress progress = quest.m_questProgress;
            if (stack >= maxStacks)
            {
                progress.DisableQuest(QuestProgress.ProgressState.Successful);
                return;
            }

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedClaustroStart, 1);
                stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedClaustroStart.EventUID);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("Something happened... this is my chance to escape.");
            }
            else if (stack < 2)
            {
                // Second log
                QuestEventManager.Instance.AddEvent(QE_FixedClaustroStart, 1);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), false);
                ShowUIMessage("I should go somewhere safe...");

            }
            if (stack < 3 && AreaManager.Instance.GetIsCurrentAreaTownOrCity() == true)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedClaustroStart, 2);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
                ShowUIMessage("It would be safer to join a faction...");

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

        public override void OnScenarioChosen(Character character)
        {

        }

        public override void OnStartSpawn()
        {
            GetOrGiveQuestToHost();
        }

        public override void OnStartSpawn(Character character)
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

        [HarmonyPatch(typeof(CharacterInventory), "TakeItem", new Type[] { typeof(Item), typeof(bool) })]
        public class CharacterInventory_TakeItem
        {
            [HarmonyPrefix]
            public static void Prefix(CharacterInventory __instance, Item takenItem, ref bool _tryToEquip, ref Character ___m_character)
            {
                Character player = ___m_character;
                if (player == null || !player.IsLocalPlayer) { return; }
                Equipment takenEquipment;
                if(takenItem is Equipment) { takenEquipment = takenItem as Equipment; } else { takenEquipment = null; }

                if (player.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.Claustrophobic) && takenEquipment?.EquipSlot == ArmorEquipmentSlot.EquipmentSlotIDs.Helmet)
                {
                    _tryToEquip = false;
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
