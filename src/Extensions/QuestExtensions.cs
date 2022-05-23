using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternateStart
{
    internal static class QuestExtensions
    {
        internal static void UpdateLogEntry(this QuestProgress progress, QuestLogEntrySignature signature, bool displayTime)
        {
            progress.UpdateLogEntry(signature.UID, displayTime, signature);
        }
    }
}
