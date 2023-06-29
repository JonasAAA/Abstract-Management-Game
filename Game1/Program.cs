global using Game1.MyMath;
global using Game1.PrimitiveTypeWrappers;
global using Game1.Resources;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Microsoft.Xna.Framework.Input;
global using System;
global using System.Collections.Generic;
global using System.Diagnostics;
global using System.Linq;
global using UpdatePersonSkillsParams = System.Collections.Generic.List<(Game1.Industries.IndustryType industryType, Game1.PrimitiveTypeWrappers.Score.ParamsOfChange paramsOfSkillChange)>;
global using MaterialChoices = Game1.Collections.EfficientReadOnlyDictionary<Game1.Resources.IMaterialPurpose, Game1.Resources.Material>;
global using TextErrors = Game1.Collections.EfficientReadOnlyHashSet<string>;
global using AreaInt = Game1.Resources.Area<ulong>;
global using AreaDouble = Game1.Resources.Area<Game1.PrimitiveTypeWrappers.UDouble>;
global using AllResAmounts = Game1.Collections.SomeResAmounts<Game1.Resources.IResource>;

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
