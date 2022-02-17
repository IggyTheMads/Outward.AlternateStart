using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlternateStart
{
    // Not actually using this yet.
    // If you want to use this, you'd have to make Passive Skills for Magic, Stamina, Tanky and Freeform.
    // You'd also have to do "IsAnyChosen<ScenarioTheme>" in ScenarioManager, etc.
    public enum ScenarioTheme
    {
        None,
        Magic,
        Stamina,
        Tanky,
        Freeform
    }
}
