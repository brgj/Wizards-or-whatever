using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace WizardsOrWhatever
{
    public static class CoordinateHelper
    {
        public static Vector2 ToScreen(Vector2 worldCoordinates)
        {
            return ConvertUnits.ToDisplayUnits(worldCoordinates);
        }

        public static Vector2 ToWorld(Vector2 screenCoordinates)
        {
            return ConvertUnits.ToSimUnits(screenCoordinates);
        }
    }
}
