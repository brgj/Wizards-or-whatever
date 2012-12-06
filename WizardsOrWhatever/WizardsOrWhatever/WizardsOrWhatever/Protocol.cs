using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WizardsOrWhatever
{
    public enum Protocol:byte
    {
        Disconnected = 0,
        Connected = 1,
        Movement = 2,
        Initialize = 3,
        AddCharacter = 4,
        CreateTerrain = 5,
        FireProjectile = 6,
        ModifyTerrain = 7,
    }
}
