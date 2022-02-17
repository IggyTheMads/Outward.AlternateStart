using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AlternateStart
{
    public static class VanillaQuestsHelper
    {
        #region Quest IDs and UIDs

        // ~~~~~ Quest IDs ~~~~~

        public const int lookingToTheFutureQ = 7011002;
        public const int callToAdventureQ = 7011001;
        public const int enrollmentQ = 7011400;

        // ~~~~~ QuestEvent UIDs ~~~~~

        public const string playerInCierzo = "sm812Cio9ki5ssbsiPr3Fw";
        public const string tutorialIntroFinished = "HteYicnCK0atCgd4j5TV1Q";
        public const string introPlayerHouse = "z23QoIdtkU6cUPoUOfDn6w";
        public const string grandmother = "YQD53MKgwke6juWiSWI7jQ";
        public const string rissaBegExtension = "nt9KhXoJtkOalZ-wtfueDA";
        public const string rissaTalk = "n_3w-BcFfEW52Ht4Q3ZCjw";

        // KEEP HOUSE
        public const string debtPAID = "8GvHUbDz90OOJWurd-RZlg";
        public const string callToAdventureENDa = "g3EX5w1mwUaYW1o0cpj0SQ";

        // LOSE HOUSE
        public const string notLighthouseOwner = "-Ku9dHjTl0KeUPxWk0ZWWQ";
        public const string lostLightHouse = "qPEx275DTUSPbnv-PnFn7w";
        public const string debtNOTpaid = "sAc2Dj-T_kysKXFV48Hp0A";
        public const string callToAdventureENDb = "8iAJYhhqj02ODuwZ9VvXMw";

        public const string directionRISA = "seEIFfM9SkeZxc4CpR40oQ";
        public const string directionOLIELE = "Bo4-Xvq4_kudPDnOgkI3VA";
        public const string directionYSAN = "gAJAjuzl7ESFpMooq1oOCg";
        public const string olieleMonsoon = "3_soGcNagk-KcYSeqpEgMg";
        public const string yzanLevant = "BgpOoGQF10O7IQyiB9HODw";
        public const string rissaBerg = "1a6Zs9A_gEmScBetAraQaw";
        public const string readyToChoose = "jNfNsLGBpk2iMRts9kkerw";
        public const string BCproposition = "fo2uI7yiw0WSRE7MsbYFyw";
        public const string HKproposition = "JlFMC_51RUalSa8yLkhTmg";
        public const string HMproposition = "JqL0_JD55US2gL0-GbOBow";
        public const string argensonMet = "QMe_j2TIWEKpssXkLHMMZA";
        public const string argensonStash = "h8jI-dDsfkStb3XkCjqMPw";
        public const string YzanFriendship = "nr9KDCbQzUae1Gwf-6yOIQ";
        public const string callAdventureExpired = "zoKR1_bqiUiAetJ3uxw-Ug"; //house obtain timer
        public const string callAdventureCompleted = "ZYzrMi1skUiJ4BgXXQ3sfw";

        public const string ashAllyFail = "nDy01eTHlUa_BPDlbIhZPQ";
        public const string ashCompleteFail = "f1JVZyhg2UiBA8xmC-w6Hw";
        public const string ashFight = "XqlcpbTJC0aTDZfjD4xCTg";
        public const string ashWarp = "oGkuUgWvfkej_El-rhz2gw";

        #endregion

        internal static void SkipHostToFactionChoice(bool keepHouse)
        {
            var host = CharacterManager.Instance.GetWorldHostCharacter();

            host.Inventory.QuestKnowledge.ReceiveQuest(callToAdventureQ);
            host.Inventory.QuestKnowledge.ReceiveQuest(lookingToTheFutureQ);

            Plugin.Instance.StartCoroutine(AddPrefactionEvents(keepHouse));
        }


        internal static void AddQuestEvent(string questUID)
        {
            QuestEventManager.Instance.AddEvent(QuestEventDictionary.GetQuestEvent(questUID), 1);
        }

        internal static void RemoveEvent(string questUID)
        {
            QuestEventManager.Instance.RemoveEvent(questUID);
        }
        
        
        internal static IEnumerator AddPrefactionEvents(bool keepHouse)
        {
            yield return new WaitForSeconds(0.5f); 
        
            AddQuestEvent(playerInCierzo);
            AddQuestEvent(tutorialIntroFinished);
        
            AddQuestEvent(introPlayerHouse);
            AddQuestEvent(rissaBegExtension);
            AddQuestEvent(grandmother);
            AddQuestEvent(rissaTalk);
        
            if (keepHouse)
            {
                AddQuestEvent(debtPAID);
                AddQuestEvent(callToAdventureENDa);
            }
            else
            {
                AddQuestEvent(notLighthouseOwner);
                AddQuestEvent(lostLightHouse);
                AddQuestEvent(debtNOTpaid);
                AddQuestEvent(callToAdventureENDb);
            }
        
            AddQuestEvent(directionRISA);
            AddQuestEvent(readyToChoose);
            AddQuestEvent(olieleMonsoon);
            AddQuestEvent(directionOLIELE);
            AddQuestEvent(HMproposition);
            AddQuestEvent(HKproposition);
            AddQuestEvent(yzanLevant);
            AddQuestEvent(directionYSAN);
            AddQuestEvent(argensonMet);
            AddQuestEvent(argensonStash);
            AddQuestEvent(YzanFriendship);
            AddQuestEvent(rissaBerg);
            AddQuestEvent(BCproposition);
            AddQuestEvent(callAdventureExpired);
            AddQuestEvent(callAdventureCompleted);
        }
        
        internal static void StartHouseTimer()
        {
            CharacterManager.Instance.GetWorldHostCharacter().Inventory.QuestKnowledge.ReceiveQuest(callToAdventureQ);
        
            Plugin.Instance.StartCoroutine(AddHouseTimerEvents());
        }
        
        static IEnumerator AddHouseTimerEvents()
        {
            yield return new WaitForSeconds(0.5f);
        
            AddQuestEvent(playerInCierzo);
            AddQuestEvent(tutorialIntroFinished);
            AddQuestEvent(introPlayerHouse);
            AddQuestEvent(rissaTalk);
            AddQuestEvent(callAdventureExpired);
        }
    }
}
