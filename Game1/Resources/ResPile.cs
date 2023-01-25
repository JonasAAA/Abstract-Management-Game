namespace Game1.Resources
{
    public readonly struct ResPile : ISourcePile<ResAmounts>, IDestinPile<ResAmounts>
    {
        public ResPile CreateEmpty(LocationCounters locationCounters, ThermalBody thermalBody)
            => new
            (
                resPile: Pile<ResAmounts>.CreateEmpty(locationCounters: locationCounters),
                thermalBody: thermalBody
            );

        private readonly Pile<ResAmounts> resPile;
        private readonly ThermalBody thermalBody;

        private ResPile(Pile<ResAmounts> resPile, ThermalBody thermalBody)
        {
            this.resPile = resPile;
            this.thermalBody = thermalBody;
        }
    }
}
