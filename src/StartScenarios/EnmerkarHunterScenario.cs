using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using System.Collections;
using SideLoader.Managers;
using SideLoader;

namespace AlternateStart.StartScenarios
{
    public class EnmerkarHunterScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_EmercarHunter;
        public override ScenarioType Difficulty => ScenarioType.Normal;
        public override ScenarioPassives Passive => ScenarioPassives.EnmerkarHunter;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.EmercarDungeonsSmall;
        public override Vector3 SpawnPosition => new(600.6f, 0.8f, 8.1f);
        public override Vector3 SpawnRotation => new(0, 29.3f, 0);
        public override void Gear(Character character)
        {
            character.Inventory.ReceiveItemReward(9000010, 13, false); //Starter Gold
            character.Inventory.ReceiveItemReward(3000020, 1, true); //adventurer armor
            character.Inventory.ReceiveItemReward(3000022, 1, true); //adventurer boots
            character.Inventory.ReceiveItemReward(2200000, 1, false); //bow
            character.Inventory.ReceiveItemReward(5200001, 30, true); //arrows
            character.Inventory.ReceiveItemReward(4000060, 4, false); //meat
        }
        public override bool HasQuest => true;
        public override string QuestName => "A Good Hunt";

        const string LogSignature_A = "hunter.objective.a";
        const string LogSignature_B = "hunter.objective.b";
        const string LogSignature_C = "hunter.objective.c";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "Leave your hut."
            },
            {
                LogSignature_B,
                "Go to any city to sell your goods."
            },
            {
                LogSignature_C,
                "You are ready to join a faction."
            }
        };

        private QuestEventSignature QE_FixedHunterStart;

        public override void Init()
        {
            base.Init();

            QE_FixedHunterStart = CustomQuests.CreateQuestEvent("iggythemad.hunter.fixedstart", false, true, true, Plugin.QUEST_EVENT_FAMILY_NAME);

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
            int stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedHunterStart.EventUID);
            QuestProgress progress = quest.m_questProgress;
            if (stack >= maxStacks)
            {
                progress.DisableQuest(QuestProgress.ProgressState.Successful);
                return;
            }

            //ShowUIMessage("Stacks -> " + stack);
            if (stack < 1)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedHunterStart, 1);
                stack = QuestEventManager.Instance.GetEventCurrentStack(QE_FixedHunterStart.EventUID);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), true);
                ShowUIMessage("Today's was a good hunt...");
            }
            else if (stack < 2)
            {
                // Second log
                QuestEventManager.Instance.AddEvent(QE_FixedHunterStart, 1);
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), false);
                ShowUIMessage("I should sell my goods in the city...");

            }
            if (stack < 3 && AreaManager.Instance.GetIsCurrentAreaTownOrCity() == true)
            {
                QuestEventManager.Instance.AddEvent(QE_FixedHunterStart, 2);

                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                // Third log just auto-completes.
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_C), true);

                // Our quest is finished i guess
                progress.DisableQuest(QuestProgress.ProgressState.Successful);

                VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
                ShowUIMessage("Huh... Should I join a faction?");
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
            character.Inventory.ReceiveSkillReward(8205160); //hunters eye
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
