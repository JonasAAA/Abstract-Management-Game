global using Game1.MyMath;
global using Game1.PrimitiveTypeWrappers;
global using Game1.Resources;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.Diagnostics;
global using System.Linq;
global using UpdatePersonSkillsParams = System.Collections.Generic.List<(Game1.Industries.IndustryType industryType, Game1.PrimitiveTypeWrappers.Score.ParamsOfChange paramsOfSkillChange)>;

namespace Game1
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            using var game = new Game1();
            game.Run();
        }
    }
}
