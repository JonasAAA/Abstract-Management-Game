using Game1.Collections;

namespace Game1.Resources
{
    public class ResAllocator
    {
        private readonly Dictionary<RawMaterial, decimal> historicalInaccuracies;

        public ResAllocator()
            => historicalInaccuracies = new();

        public SomeResAmounts<RawMaterial> TakeAtMostFrom(SomeResAmounts<RawMaterial> source, AreaInt maxArea)
        {
#warning test this
            var takeProporOrNull = Propor.Create(part: maxArea.valueInMetSq, whole: source.Area().valueInMetSq);
            if (takeProporOrNull is Propor takePropor)
            {
                foreach (var rawMat in historicalInaccuracies.Keys)
                    if (source[rawMat] == 0)
                        historicalInaccuracies[rawMat] = 0;

                Dictionary<RawMaterial, ulong> destinDict = new();
                AreaInt destinArea = AreaInt.zero;

                foreach (var (rawMat, amount) in source)
                {
                    decimal historicalInaccuracy = historicalInaccuracies.GetValueOrDefault(rawMat),
                        idealResult = (decimal)takePropor * (amount + historicalInaccuracy);
                    if (idealResult < 0)
                    {
                        destinDict[rawMat] = 0;
                        historicalInaccuracies[rawMat] = 0;
                        continue;
                    }
                    if (idealResult > amount)
                    {
                        destinDict[rawMat] = amount;
                        historicalInaccuracies[rawMat] = 0;
                        continue;
                    }
                    ulong result = (ulong)idealResult;
                    destinDict[rawMat] = result;
                    historicalInaccuracies[rawMat] = idealResult - result;
                    destinArea += result * rawMat.Area;
                }

                // ToList is used so that the collection isn't modified while being iterated over
                foreach (var (rawMat, histInaccur) in historicalInaccuracies.OrderByDescending(rawMatAmount => rawMatAmount.Value).ToList())
                {
                    if (destinArea + rawMat.Area > maxArea)
                        break;
                    destinDict[rawMat]++;
                    historicalInaccuracies[rawMat]--;
                    destinArea += rawMat.Area;
                }

                SomeResAmounts<RawMaterial> finalResult = new(resAmounts: destinDict);
                Debug.Assert(finalResult <= source);
                Debug.Assert(finalResult.Area() <= maxArea);
                return finalResult;
            }
            else
            {
                foreach (var rawMat in historicalInaccuracies.Keys)
                    historicalInaccuracies[rawMat] = 0;
                return source;
            }
        }
    }
}
