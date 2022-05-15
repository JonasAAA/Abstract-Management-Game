﻿namespace Game1.Shapes
{
    [Serializable]
    public class Disk : Shape
    {
        public interface IParams
        {
            public MyVector2 Center { get; }

            public UDouble Radius { get; }
        }

        public UDouble Radius
            => parameters.Radius;

        private static readonly Texture2D diskTexture;

        static Disk()
            => diskTexture = C.LoadTexture(name: "big disk");

        public MyVector2 Center
            => parameters.Center;

        protected readonly IParams parameters;

        public Disk(IParams parameters)
            => this.parameters = parameters;

        public override bool Contains(MyVector2 position)
            => MyVector2.Distance(position, Center) < parameters.Radius;

        public override void Draw(Color color)
            => C.Draw
            (
                texture: diskTexture,
                position: Center,
                color: color,
                rotation: 0,
                origin: new MyVector2(diskTexture.Width, diskTexture.Height) * .5,
                scale: 2 * parameters.Radius / (UDouble)diskTexture.Width
            );
    }
}
