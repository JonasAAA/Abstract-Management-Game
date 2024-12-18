﻿using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public sealed class ResCounter : EnergyCounter<AllResAmounts>
    {
        public new static ResCounter CreateEmpty()
            => new(createdByMagic: false);

        public new static ResCounter CreateByMagic(AllResAmounts count)
            => new(createdByMagic: true)
            {
                Count = count
            };

        private ResCounter(bool createdByMagic)
            : base(createdByMagic: createdByMagic)
        { }

        public void TransformFrom(ResCounter source, ResRecipe recipe)
        {
            source.Count -= recipe.ingredients;
            Count += recipe.results;
        }

        public void TransformTo(ResCounter destin, ResRecipe recipe)
        {
            Count -= recipe.ingredients;
            destin.Count += recipe.results;
        }
    }
}
