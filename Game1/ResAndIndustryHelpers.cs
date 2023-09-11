using Game1.Collections;
using Game1.Industries;
using static Game1.WorldManager;

namespace Game1
{
    public static class ResAndIndustryHelpers
    {
        public static AreaInt Area(this RawMatAmounts rawMatAmounts)
            => rawMatAmounts.Sum(resAmount => resAmount.res.Area * resAmount.amount);

        public static Color Color(this RawMatAmounts rawMatAmounts)
        {
            Vector3 colorSum = Vector3.Zero;
            foreach (var (rawMaterial, amount) in rawMatAmounts)
                colorSum += rawMaterial.Area.valueInMetSq * amount * rawMaterial.Color.ToVector3();
            float weightSum = rawMatAmounts.Area().valueInMetSq;
            return new Color(colorSum / weightSum);
        }

        public static AreaDouble ToDouble(this AreaInt area)
            => AreaDouble.CreateFromMetSq(valueInMetSq: area.valueInMetSq);

        public static AreaInt RoundDown(this AreaDouble area)
            => AreaInt.CreateFromMetSq(valueInMetSq: (ulong)area.valueInMetSq);

        public static MaterialChoices FilterOutUnneededMaterials(this MaterialChoices materialChoices, EfficientReadOnlyHashSet<IMaterialPurpose> neededMaterialPurposes)
            => materialChoices.Where(matChoice => neededMaterialPurposes.Contains(matChoice.Key)).ToEfficientReadOnlyDict
            (
                keySelector: matChoice => matChoice.Key,
                elementSelector: matChoice => matChoice.Value
            );

        public static TEnergy CurEnergy<TEnergy>(this HistoricRounder energyHistoricRounder, UDouble watts, Propor proporUtilized, TimeSpan elapsed)
            where TEnergy : struct, IUnconstrainedEnergy<TEnergy>
            => IUnconstrainedEnergy<TEnergy>.CreateFromJoules
            (
                valueInJ: energyHistoricRounder.Round
                (
                    value: (decimal)watts * (decimal)proporUtilized * (decimal)elapsed.TotalSeconds,
                    curTime: CurWorldManager.CurTime
                )
            );

        public static Propor WorkingPropor(Propor proporUtilized, ElectricalEnergy allocatedEnergy, ElectricalEnergy reqEnergy)
            => proporUtilized * Propor.Create(part: allocatedEnergy.ValueInJ, whole: reqEnergy.ValueInJ)!.Value;

        public static Propor UpdateDonePropor(this Propor donePropor, Propor workingPropor, UDouble producedAreaPerSec, TimeSpan elapsed, AreaDouble areaInProduction)
        {
            UDouble areaProduced = workingPropor * (UDouble)elapsed.TotalSeconds * producedAreaPerSec;
            return Propor.CreateByClamp((UDouble)donePropor + areaProduced / areaInProduction.valueInMetSq);
        }

        public static AllResAmounts CurNeededBuildingComponents(EfficientReadOnlyCollection<(IProduct prod, UDouble amountPUBA)> buildingComponentsToAmountPUBA, AreaDouble curBuildingArea)
            => new
            (
                buildingComponentsToAmountPUBA.Select
                (
                    prodAndAmountPUBA => new ResAmount<IResource>
                    (
                        prodAndAmountPUBA.prod,
                        MyMathHelper.Ceiling(prodAndAmountPUBA.amountPUBA * curBuildingArea.valueInMetSq)
                    )
                )
            );
    }
}
