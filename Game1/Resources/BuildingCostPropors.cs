using Game1.Collections;

namespace Game1.Resources
{
    [Serializable]
    public readonly struct BuildingCostPropors
    {
        public readonly EfficientReadOnlyHashSet<ProductClass> neededProductClasses;
        public readonly EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts;
        public readonly AreaInt area;
        public readonly EfficientReadOnlyDictionary<ProductClass, Propor> neededProductClassPropors;

        public BuildingCostPropors(EfficientReadOnlyCollection<(Product.Params prodParams, ulong amount)> ingredProdToAmounts)
        {
            this.ingredProdToAmounts = ingredProdToAmounts;

            Dictionary<ProductClass, AreaInt> productClassAmounts = new();
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

            // Needed to satisfy compiler
            AreaInt areaCopy = area;
            neededProductClassPropors = productClassAmounts.ToEfficientReadOnlyDict
            (
                keySelector: prodClassAndArea => prodClassAndArea.Key,
                elementSelector: prodClassAndArea => Propor.Create(part: prodClassAndArea.Value.valueInMetSq, areaCopy.valueInMetSq)!.Value
            );
            if (!neededProductClassPropors.ContainsKey(ProductClass.roof))
                throw new ArgumentException();
        }
    }
}
