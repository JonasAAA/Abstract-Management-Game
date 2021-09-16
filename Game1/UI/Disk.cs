using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Game1.UI
{
    public class Disk : Shape
    {
        public float Radius
        {
            get => radius;
            set
            {
                if (radius != value)
                {
                    radius = value;
                    RadiusChanged?.Invoke();
                }
            }
        }

        public event Action RadiusChanged;

        private float radius;
        private readonly Texture2D diskTexture;

        public Disk()
            : this(radius: 0)
        { }

        public Disk(float radius)
        {
            this.radius = radius;
            diskTexture = C.Content.Load<Texture2D>("big disk");
        }

        public override bool Contains(Vector2 position)
            => Vector2.Distance(Center, position) < radius;

        public override void Draw()
        {
            if (!Transparent)
                C.Draw
                (
                    texture: diskTexture,
                    position: Center,
                    color: Color,
                    rotation: 0,
                    origin: new Vector2(diskTexture.Width, diskTexture.Height) * .5f,
                    scale: 2 * radius / diskTexture.Width
                );
        }
    }
}
