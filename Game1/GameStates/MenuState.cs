﻿using Game1.Shapes;
using Game1.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Game1.GameStates
{
    public class MenuState : GameState
    {
        private readonly ActiveUIManager activeUIManager;
        private readonly UIRectVertPanel<ActionButton> UIPanel;

        public MenuState(List<ActionButton> actionButtons)
        {
            UIPanel = new(color: Color.White, childHorizPos: HorizPos.Middle);
            foreach (var actionButton in actionButtons)
                UIPanel.AddChild(child: actionButton);

            activeUIManager = new();
            activeUIManager.AddHUDElement(HUDElement: UIPanel, horizPos: HorizPos.Middle, vertPos: VertPos.Middle);
        }

        public override void Update(TimeSpan elapsed)
            => activeUIManager.Update(elapsed: elapsed);

        public override void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(color: Color.Black);
            activeUIManager.DrawHUD();
        }
    }
}
