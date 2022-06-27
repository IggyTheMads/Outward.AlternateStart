using System;
using BepInEx;
using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SideLoader;
using AlternateStart.StartScenarios;

namespace AlternateStart
{
    public static class GearManager
    {
        //static readonly Dictionary<ScenarioPassives, Scenario> startScenarios = new();

        internal static void Init()
        {

        }

        public static void StartingGear()
        {
            foreach(PlayerSystem playerSys in Global.Lobby.PlayersInLobby)
            {
                //Debug.Log("checking scenarios");
                Character player = playerSys.ControlledCharacter;
                var knows = player.Inventory.SkillKnowledge;
                foreach(Scenario scenario in ScenarioManager.startScenarios.Values)
                {
                    //Debug.Log("scenario read");
                    if (knows.IsItemLearned((int)scenario.Passive))
                    {
                        Debug.Log("adding scenario gear");
                        //may not work online due to item reward
                        scenario.Gear(player);
                        break;
                    }
                    
                }
                //yield return new WaitForSeconds(0.5f);
            }
        }

        public static void RandomPassive()
        {

        }
    }
}