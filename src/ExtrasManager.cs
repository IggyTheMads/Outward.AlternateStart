using System;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AlternateStart
{
    public class ExtrasManager : MonoBehaviour
    {
        public static ExtrasManager Instance;

        Vector3 spawn = new Vector3(-12.2f, 0.2f, -0.6f);
        Vector3 spawnRot = new Vector3(0, 75.2f, 0);

        internal void Awake()
        {
            Instance = this;
        }

        [HarmonyPatch(typeof(Character), "DodgeInput", new Type[] { typeof(Vector3) })]
        public class Character_DodgeInput
        {
            [HarmonyPostfix]
            public static void Postfix(Character __instance, Vector3 _direction)
            {
                if (__instance.IsLocalPlayer && __instance.Inventory.SkillKnowledge.IsItemLearned((int)ScenarioPassives.GiantRisen) && !__instance.DodgeRestricted)
                {
                    Instance.StartCoroutine(Instance.DodgeSlower(__instance));
                }
            }
        }

        public IEnumerator DodgeSlower(Character _character)
        {

            //yield return new WaitForSeconds(0.1f);
            if (_character.Dodging == true)
            {
                _character.Animator.speed = 0.6f;
                while (_character.Dodging == true)
                {
                    yield return new WaitForSeconds(0.2f);
                }
                yield return new WaitForSeconds(0.2f);
                _character.Animator.speed = 1f;
            }
        }

        void Update()
        {
            if(SceneManagerHelper.ActiveSceneName == "DreamWorld")
            {
                foreach (PlayerSystem _character in Global.Lobby.PlayersInLobby)
                {
                    float distance = Vector3.Distance(_character.ControlledCharacter.transform.position, spawn);
                    if (distance > 3f)
                    {
                        _character.ControlledCharacter.Teleport(spawn, spawnRot);
                    }
                }
            }
        }
    }
}