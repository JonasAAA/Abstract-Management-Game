using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Game1
{
    public class TextBox
    {
        public Color TextColor { private get; set; }
        public string Text { private get; set; }

        private readonly SpriteFont font;
        private readonly float scale;
        private readonly Vector2 relPos;

        public TextBox(Vector2 relPos, float letterHeight)
        {
            font = C.Content.Load<SpriteFont>("font");
            this.relPos = relPos;
            scale = letterHeight / font.MeasureString("F").Y;
            TextColor = Color.Black;
            Text = "";
        }

        public void Draw()
        {
        }
    }
}
