namespace Game1.Resources
{
    [Serializable]
    public class ResPile
    {
        public static ResPile CreateEmpty(ThermalBody thermalBody)
            => new
            (
                resPile: EnergyPile<ResAmounts>.CreateEmpty(locationCounters: thermalBody.locationCounters),
                thermalBody: thermalBody
            );

        public static ResPile? CreateIfHaveEnough(ResPile source, ResAmounts amount)
        {
            var newPile = EnergyPile<ResAmounts>.CreateIfHaveEnough(source: source.resPile, amount: amount);
            if (newPile is null)
                return null;
            return new(resPile: newPile, thermalBody: source.thermalBody);
        }

        public static ResPile CreateByMagic(ResAmounts amount)
        {
            // TODO: Look at this, want to insure that the (total amount of energy) * (max heat capacity) fit comfortably into ulong
            // If run into problems with overflow, could use int128 or uint128 instead of ulong from
            // https://learn.microsoft.com/en-us/dotnet/api/system.int128?view=net-7.0 https://learn.microsoft.com/en-us/dotnet/api/system.uint128?view=net-7.0
            var resPile = EnergyPile<ResAmounts>.CreateByMagic(amount: amount);
            return new
            (
                resPile: resPile,
                thermalBody: ThermalBody.CreateEmpty(locationCounters: resPile.LocationCounters)
            //peopleCounter: Counter<NumPeople>.CreateEmpty(),
            //resCounter: EnergyCounter<ResAmounts>.CreateByMagic(count: resAmounts),
            //heatEnergyCounter: EnergyCounter<HeatEnergy>.CreateByMagic(count: HeatEnergy.CreateFromJoules(valueInJ: temperatureInK * resAmounts.HeatCapacity().valueInJPerK)),
            //radiantEnergyCounter: EnergyCounter<RadiantEnergy>.CreateEmpty(),
            //electricalEnergyCounter: EnergyCounter<ElectricalEnergy>.CreateEmpty()
            );
        }

        public ResAmounts Amount
            => resPile.Amount;

        public bool IsEmpty
            => Amount == ResAmounts.Empty;

        private readonly EnergyPile<ResAmounts> resPile;
        private ThermalBody thermalBody;

        private ResPile(EnergyPile<ResAmounts> resPile, ThermalBody thermalBody)
        {
            this.resPile = resPile;
            this.thermalBody = thermalBody;
        }

        public void ChangeLocation(ThermalBody newThermalBody)
        {
            newThermalBody.TransferResFrom(source: thermalBody, amount: Amount);
            resPile.ChangeLocation(newLocationCounters: newThermalBody.locationCounters);
            thermalBody = newThermalBody;
        }

        public void TransferFrom(ResPile source, ResAmounts amount)
        {
            //This must be done first to get accurate source heat capacity in the calculations
            thermalBody.TransferResFrom(source: source.thermalBody, amount: amount);
            resPile.TransferFrom(source: source.resPile, amount: amount);
        }

        public void TransferAtMostFrom(ResPile source, ResAmounts maxAmount)
            => TransferFrom(source: source, amount: MyMathHelper.Min(maxAmount, Amount));

        public void TransferAllFrom(ResPile source)
            => TransferFrom(source: source, amount: source.Amount);

        public void TransferAllSingleResFrom(ResPile source, ResInd resInd)
            => TransferFrom
            (
                source: source,
                amount: new
                (
                    resInd: resInd,
                    amount: source.Amount[resInd]
                )
            );

        public void TransformAndTransferFrom(ResPile source, ResRecipe recipe)
        {
            throw new NotImplementedException();
        }
    }
}
