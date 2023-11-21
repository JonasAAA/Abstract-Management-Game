using Game1.ContentNames;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Game1.UI
{
    [Serializable]
    public sealed class Image : IImage
    {
        public UDouble Width { get; private set; }
        public UDouble Height { get; }

        private readonly TextureName name;
        [NonSerialized] private Texture2D image;
        private Vector2Bare origin;
        private UDouble scale;

        public Image(TextureName name, UDouble height)
        {
            this.name = name;
            Height = height;
            //// THIS WIDTH IS ONLY TEMPRORARY
            //Width = height;
            LoadTexture();
        }

        [MemberNotNull(nameof(image))]
        private void LoadTexture()
        {
            image = C.LoadTexture(name);
            scale = Height / (UDouble)image.Height;
            Width = (UDouble)image.Width * scale;
            origin = new(image.Width * .5, image.Height * .5);
        }

        [OnDeserialized]
        private void LoadTextureAfterDeserialization(StreamingContext streamingContext)
            => LoadTexture();

        public void Draw(Vector2Bare center)
            //=> throw new NotImplementedException();
            => C.Draw
            (
                texture: image,
                position: center,
                color: Color.White,
                rotation: 0,
                origin: origin,
                scale: scale
            );
    }
}
