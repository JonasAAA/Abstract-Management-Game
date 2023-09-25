using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct BuildingCostPropors
    {
        public readonly EfficientReadOnlyHashSet<IProductClass> neededProductClasses;
        public readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts;
        public readonly AreaInt area;
        public readonly MechComplexity complexity;
        public readonly EfficientReadOnlyDictionary<IProductClass, Propor> productClassPropors;

        public BuildingCostPropors(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts)
        {
            this.ingredProdToAmounts = ingredProdToAmounts;

            Dictionary<IProductClass, AreaInt> productClassAmounts = new();
            foreach (var (prodParams, amount) in ingredProdToAmounts)
            {
                if (amount is 0)
                    throw new ArgumentException();
                productClassAmounts.TryAdd(key: prodParams.productClass, value: AreaInt.zero);
                productClassAmounts[prodParams.productClass] += prodParams.area * amount;
            }
            area = productClassAmounts.Values.Sum();
            Debug.Assert(area == ingredProdToAmounts.Sum(prodParamsAndAmount => prodParamsAndAmount.prodParams.area * prodParamsAndAmount.amount));

            neededProductClasses = productClassAmounts.Keys.ToEfficientReadOnlyHashSet();

            complexity = ResAndIndustryAlgos.IndustryMechComplexity(ingredProdToAmounts: ingredProdToAmounts, productClassPropors: productClassPropors);
            // Needed to satisfy compiler
            AreaInt areaCopy = area;
            productClassPropors = productClassAmounts.ToEfficientReadOnlyDict
            (
                keySelector: prodClassAndArea => prodClassAndArea.Key,
                elementSelector: prodClassAndArea => Propor.Create(part: prodClassAndArea.Value.valueInMetSq, areaCopy.valueInMetSq)!.Value
            );
            if (!productClassPropors.ContainsKey(IProductClass.roof))
                throw new ArgumentException();
        }
    }
}
