using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct BuildingCostPropors
    {
        public readonly EfficientReadOnlyHashSet<IProductClass> neededProductClasses;
        public readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts;
        public readonly AreaInt usefulArea;
        public readonly MechComplexity complexity;
        public readonly EfficientReadOnlyDictionary<IProductClass, Propor> productClassPropors;

        public BuildingCostPropors(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts)
        {
            this.ingredProdToAmounts = ingredProdToAmounts;

            Dictionary<IProductClass, AreaInt> productClassUsefulAreas = new();
            foreach (var (prodParams, amount) in ingredProdToAmounts)
            {
                if (amount is 0)
                    throw new ArgumentException();
                productClassUsefulAreas.TryAdd(key: prodParams.productClass, value: AreaInt.zero);
                productClassUsefulAreas[prodParams.productClass] += prodParams.usefulArea * amount;
            }
            usefulArea = productClassUsefulAreas.Values.Sum();
            Debug.Assert(usefulArea == ingredProdToAmounts.Sum(prodParamsAndAmount => prodParamsAndAmount.prodParams.usefulArea * prodParamsAndAmount.amount));

            neededProductClasses = productClassUsefulAreas.Keys.ToEfficientReadOnlyHashSet();

            complexity = ResAndIndustryAlgos.IndustryMechComplexity(ingredProdToAmounts: ingredProdToAmounts, productClassPropors: productClassPropors);
            // Needed to satisfy compiler
            AreaInt usefulAreaCopy = usefulArea;
            productClassPropors = productClassUsefulAreas.ToEfficientReadOnlyDict
            (
                keySelector: prodClassAndArea => prodClassAndArea.Key,
                elementSelector: prodClassAndArea => Propor.Create(part: prodClassAndArea.Value.valueInMetSq, usefulAreaCopy.valueInMetSq)!.Value
            );
            if (!productClassPropors.ContainsKey(IProductClass.roof))
                throw new ArgumentException();
        }
    }
}
