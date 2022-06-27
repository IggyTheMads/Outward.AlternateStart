using SideLoader;
using SideLoader.Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using AlternateStart.Characters;
using NodeCanvas.DialogueTrees;
using NodeCanvas.Framework;
using NodeCanvas.Tasks.Actions;

namespace AlternateStart.StartScenarios
{

    public class SandBanditScenario : Scenario
    {
        public override ScenarioQuest Type => ScenarioQuest.Quest_SandBandit;
        public override ScenarioType Difficulty => ScenarioType.WIPtest;
        public override ScenarioPassives Passive => ScenarioPassives.SandBandit;

        public override AreaManager.AreaEnum SpawnScene => AreaManager.AreaEnum.AbrassarDungeon6;
        public override Vector3 SpawnPosition => new(-53.3f, 0.5f, 55.1f);
        public override Vector3 SpawnRotation => new(0, 227.3f, 0);
        public override void Gear(Character character)
        {
            character.Inventory.ReceiveItemReward(9000010, 26, false); //Starter Gold
            character.Inventory.ReceiveItemReward(5100010, 1, true); //lamp
            character.Inventory.ReceiveItemReward(3000087, 1, true); //beggar helm
            character.Inventory.ReceiveItemReward(3000201, 1, true); //desert armor
            character.Inventory.ReceiveItemReward(3000205, 1, true); //desert legs
            character.Inventory.ReceiveItemReward(2000110, 1, true); //curved sword
        }
        public override bool HasQuest => true;
        public override string QuestName => "Sand Corsair Exile";

        const float GRACE_PERIOD_INGAMETIME = 0.3f;

        const string LogSignature_A = "sandbandits.objective.a";
        const string LogSignature_B = "sandbandits.objective.b";
        public override Dictionary<string, string> QuestLogSignatures => new()
        {
            {
                LogSignature_A,
                "You have been exiled from the Sand Corsairs, leave before they turn on you!"
            },
            {
                LogSignature_B,
                "Your grace period is over, Old Levant is no longer your ally."
            }
        };

        static QuestEventSignature QE_StartTimer;

        private Coroutine delayedQuestUpdate;

        internal static SandBanditScenario Instance { get; private set; }
        public SandBanditScenario()
        {
            Instance = this;
        }

        public override void Init()
        {
            base.Init();

            QE_StartTimer = CustomQuests.CreateQuestEvent("iggythemad.sandbandits.starttimer", true, false, true, Plugin.QUEST_EVENT_FAMILY_NAME);

            SL.OnGameplayResumedAfterLoading += SL_OnGameplayResumedAfterLoading;
            SetupTestCharacter();
        }

        public override void OnScenarioChosen()
        {
            VanillaQuestsHelper.SkipHostToFactionChoice(false, true);
        }

        public override void OnScenarioChosen(Character character)
        {

        }

        public override void OnStartSpawn()
        {
            ChangeCharactersFactions(Character.Factions.Bandits, this.QuestLogSignatures[LogSignature_A]);

            QuestEventManager.Instance.AddEvent(QE_StartTimer);

            GetOrGiveQuestToHost();
            StartDelayedQuestUpdate();
        }

        public override void OnStartSpawn(Character character)
        {
        }

        private void ChangeCharactersFactions(Character.Factions faction, string notifText)
        {
            foreach (string uid in CharacterManager.Instance.PlayerCharacters.Values)
            {
                var character = CharacterManager.Instance.GetCharacter(uid);

                if (character.Faction == faction)
                    continue;

                character.ChangeFaction(faction);

                if (!string.IsNullOrEmpty(notifText))
                    ShowUIMessage(notifText);
            }
        }

        private void SL_OnGameplayResumedAfterLoading()
        {
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            var host = CharacterManager.Instance.GetWorldHostCharacter();
            if (host.Inventory.QuestKnowledge.IsItemLearned((int)this.Type))
            {
                var quest = host.Inventory.QuestKnowledge.GetItemFromItemID((int)this.Type) as Quest;
                UpdateQuestProgress(quest);
            }

            if (SceneManagerHelper.ActiveSceneName == "Abrassar_Dungeon6") //only show when  already not bandit
            {
                var enemies = Plugin.FindObjectsOfType<Character>();
                int enemiesAlive = 0;
                foreach (var enemy in enemies)
                {
                    if (enemy.Alive && enemy.IsAI && enemy.Faction == Character.Factions.Bandits) { enemiesAlive += 1; }
                }
                Instance.ShowUIMessage(enemiesAlive + " former friends left alive.");
            }
            else if(SceneManagerHelper.ActiveSceneName == "Levant")
            {
                NetworkLevelLoader.Instance.RequestSwitchArea(AreaManager.Instance.GetArea(AreaManager.AreaEnum.Abrassar).SceneName, 0, 1.5f);
            }
        }

        void StartDelayedQuestUpdate()
        {
            if (delayedQuestUpdate != null)
                Plugin.Instance.StopCoroutine(delayedQuestUpdate);

            delayedQuestUpdate = Plugin.Instance.StartCoroutine(UpdateQuestAfterDelay());
        }

        IEnumerator UpdateQuestAfterDelay()
        {
            var timer = QuestEventManager.Instance.GetEventActiveTimeDelta(QE_StartTimer.EventUID);

            while (timer < GRACE_PERIOD_INGAMETIME)
            {
                yield return new WaitForSeconds(1f);

                if (QuestEventManager.Instance == null || !CharacterManager.Instance?.GetWorldHostCharacter())
                    yield break;

                timer = QuestEventManager.Instance.GetEventActiveTimeDelta(QE_StartTimer.EventUID);
            }

            var host = CharacterManager.Instance?.GetWorldHostCharacter();

            if (!host)
                yield break;

            if (host.Inventory.QuestKnowledge.IsItemLearned((int)this.Type))
            {
                var quest = host.Inventory.QuestKnowledge.GetItemFromItemID((int)this.Type) as Quest;
                UpdateQuestProgress(quest);
            }

            delayedQuestUpdate = null;
        }

        public override void UpdateQuestProgress(Quest quest)
        {
            if (PhotonNetwork.isNonMasterClientInRoom || !IsActiveScenario)
                return;

            var timer = QuestEventManager.Instance.GetEventActiveTimeDelta(QE_StartTimer.EventUID);

            QuestProgress progress = quest.m_questProgress;

            progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_A), timer >= GRACE_PERIOD_INGAMETIME);

            if (timer >= GRACE_PERIOD_INGAMETIME)
            {
                progress.UpdateLogEntry(progress.GetLogSignature(LogSignature_B), true);
                ChangeCharactersFactions(Character.Factions.Player, this.QuestLogSignatures[LogSignature_B]);

                progress.DisableQuest(QuestProgress.ProgressState.Successful);
            }
            else
            {
                ChangeCharactersFactions(Character.Factions.Bandits, string.Empty);
                StartDelayedQuestUpdate();
            }
        }

        [HarmonyPatch(typeof(CharacterStats), "ReceiveDamage")]
        public class Character_ReceiveDamage
        {
            [HarmonyPostfix]
            public static void Postfix(CharacterStats __instance, ref float _damage, ref Character ___m_character)
            {
                if (!Instance.IsActiveScenario || !___m_character.IsAI) return;
                if (SceneManagerHelper.ActiveSceneName != "Abrassar_Dungeon6") { return; }
                if (_damage > __instance.CurrentHealth) //need to check only if player dealt the damage
                {
                    Plugin.Instance.StartCoroutine(Instance.LeftAlive());
                }
            }
        }

        IEnumerator LeftAlive()
        {
            yield return new WaitForSeconds(0.5f);

            var enemies = Plugin.FindObjectsOfType<Character>();
            int enemiesAlive = 0;
            foreach (var enemy in enemies)
            {
                if (enemy.Alive && enemy.IsAI && enemy.Faction == Character.Factions.Bandits) { enemiesAlive += 1; }
            }
            Instance.ShowUIMessage(enemiesAlive + " former friends left alive.");

            //var host = CharacterManager.Instance?.GetWorldHostCharacter();
            //host.Teleport(enemiesAlive.First<Character>().transform.position, host.transform.rotation);
            if (enemiesAlive > 0)
            {
                Instance.ShowUIMessage(enemiesAlive + " former friends left alive.");
                //add quest stack
            }
            else
            {
                //complete vengeance quest
            }
        }

        [HarmonyPatch(typeof(CharacterManager), "RequestAreaSwitch")]
         public class CharacterManager_RequestAreaSwitch
         {
             [HarmonyPrefix]
             public static bool Prefix(CharacterManager __instance, Character _character, Area _areaToSwitchTo, int _longTravelTime, int _spawnPoint, float _offset, string _overrideLocKey)
             {
                 if (!Instance.IsActiveScenario)
                     return true;
         
                 // Do nothing if we are not the host.
                 if (PhotonNetwork.isNonMasterClientInRoom)
                     return true;
         
                 if (_areaToSwitchTo.SceneName == "Levant")
                 {
                    _character.CharacterUI.ShowInfoNotification("You are not welcome here");
                    return false;
                 }
                 else { return true; }
             }
         }

        // ~~~~~~~~~~ Setup custom character dialogue ~~~~~~~~~~~~~~~~~

        // Dialogue character test
        internal static DialogueCharacter levantGuard;
        private void SetupTestCharacter()
        {
            levantGuard = new()
            {
                UID = "levantguard.character",
                Name = "Levant Guard",
                SpawnSceneBuildName = "Abrassar",
                SpawnPosition = new(-159.4f, 131.8f, -532.7f),
                SpawnRotation = new(0, 43.7f, 0),
                HelmetID = 3000115,
                ChestID = 3000112,
                BootsID = 3000118,
                WeaponID = 2130305,
                StartingPose = Character.SpellCastType.IdleAlternate,
            };

            // Create and apply the template
            var template = levantGuard.CreateAndApplyTemplate();

            // Add a listener to set up our dialogue
            levantGuard.OnSetupDialogueGraph += TestCharacter_OnSetupDialogueGraph;

            // Add this func to determine if our character should actually spawn
            template.ShouldSpawn = () => this.IsActiveScenario;
        }

        private void TestCharacter_OnSetupDialogueGraph(DialogueTree graph, Character character)
        {
            var ourActor = graph.actorParameters[0];

            // Add our root statement
            var rootStatement = graph.AddNode<StatementNodeExt>();
            rootStatement.statement = new("Halt! You are not welcome here, bandit. Go away or I'll make you.");
            rootStatement.SetActorName(ourActor.name);

            // Add a multiple choice
            var multiChoice1 = graph.AddNode<MultipleChoiceNodeExt>();
            multiChoice1.availableChoices.Add(new(statement: new("Why dont you trust me?")));
            multiChoice1.availableChoices.Add(new(statement: new("I've left the corsairs behind. Let me in.")));
            multiChoice1.availableChoices.Add(new(statement: new("How can I prove my loyalty to the king?")));

            // Add our answers
            var answer1 = graph.AddNode<StatementNodeExt>();
            answer1.statement = new("We have seen your bunch. Nothing more than soon-to-be-dead outlaws.");
            answer1.SetActorName(ourActor.name);

            var answer2 = graph.AddNode<StatementNodeExt>();
            answer2.statement = new("Should we just believe you? Just like that? Piss off, kid. I'm watching you.");
            answer2.SetActorName(ourActor.name);

            var answer3 = graph.AddNode<StatementNodeExt>();
            answer3.statement = new("Pfff-You can't be serious... Well I suppose getting rid of your people in the Sand Rose cave could ear you a way in.");
            answer3.SetActorName(ourActor.name);

            // ===== finalize nodes =====
            graph.allNodes.Clear();
            // add the nodes we want to use
            graph.allNodes.Add(rootStatement);
            graph.primeNode = rootStatement;
            graph.allNodes.Add(multiChoice1);
            graph.allNodes.Add(answer1);
            graph.allNodes.Add(answer2);
            graph.allNodes.Add(answer3);
            // setup our connections
            graph.ConnectNodes(rootStatement, multiChoice1);    // prime node triggers the multiple choice
            graph.ConnectNodes(multiChoice1, answer1, 0);       // choice1: answer1
            graph.ConnectNodes(answer1, rootStatement);         // - choice1 goes back to root node
            graph.ConnectNodes(multiChoice1, answer2, 1);       // choice2: answer2
            graph.ConnectNodes(answer2, rootStatement);         // - choice2 goes back to root node
            graph.ConnectNodes(multiChoice1, answer3, 2);       // choice3: answer3
            graph.ConnectNodes(answer3, rootStatement);         // - choice3 goes back to root node
        }
    }
}
