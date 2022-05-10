namespace Game1.Resources
{
    [Serializable]
    public class NonBasicRes : IResource
    {
        public ulong Mass
            => mass switch
            {
                0 => throw new InvalidOperationException($"must initialize {nameof(NonBasicRes)} by calling {nameof(Initialize)} first"),
                > 0 => mass
            };

        public readonly NonBasicResInd resInd;
        public readonly ResAmounts recipe;
        private ulong mass;

        public NonBasicRes(NonBasicResInd resInd, ResAmounts recipe)
        {
            this.resInd = resInd;
            this.recipe = recipe;
            foreach (var otherResInd in ResInd.All)
                if (otherResInd >= resInd && recipe[otherResInd] > 0)
                    throw new ArgumentException();
            if (recipe.IsEmpty())
                throw new ArgumentException();
        }

        public void Initialize(Func<ResInd, ulong> masses)
        {
            if (mass != 0)
                throw new InvalidOperationException($"{nameof(NonBasicRes)} is alrealy initialized, so can't initialize it a second time");
            mass = 0;
            foreach (var otherResInd in ResInd.All)
                if (recipe[otherResInd] != 0)
                    mass += recipe[otherResInd] * masses(otherResInd);
        }

        ResInd IResource.ResInd
            => resInd;

        ulong IResource.Mass
            => Mass;
    }
}
