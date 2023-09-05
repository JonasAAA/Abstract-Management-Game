namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacketsByDestin
    {
        public static ResAmountsPacketsByDestin CreateEmpty(ThermalBody thermalBody)
            => new(thermalBody: thermalBody);

        public static ResAmountsPacketsByDestin CreateFromSource(ResAmountsPacketsByDestin sourcePackets)
            => CreateFromSource(sourcePackets: sourcePackets, thermalBody: sourcePackets.thermalBody);

        public static ResAmountsPacketsByDestin CreateFromSource(ResAmountsPacketsByDestin sourcePackets, ThermalBody thermalBody)
        {
            ResAmountsPacketsByDestin newPackets = new(thermalBody: thermalBody);
            newPackets.TransferAllFrom(sourcePackets: sourcePackets);
            return newPackets;
        }

        // ResAmounts is not Counter, otherwise it would be impossible to implement void TransferAllFrom(ResAmountsPacket sourcePacket)
        public AllResAmounts ResAmounts { get; private set; }
        public Mass Mass { get; private set; }
        public bool Empty
            => Mass.IsZero;

        private readonly ThermalBody thermalBody;
        private Dictionary<NodeID, ResAmountsPacket> resAmountsPacketsByDestin;

        private ResAmountsPacketsByDestin(ThermalBody thermalBody)
        {
            this.thermalBody = thermalBody;
            resAmountsPacketsByDestin = new();

            ResAmounts = AllResAmounts.empty;
            Mass = Mass.zero;
        }

        public void TransferAllFrom(ResAmountsPacketsByDestin sourcePackets)
        {
            foreach (var resAmountsPacket in sourcePackets.DeconstructAndClear())
                TransferAllFrom(sourcePacket: resAmountsPacket);
        }

        public void TransferAllFrom(ResAmountsPacket sourcePacket)
        {
            ResAmounts += sourcePacket.resPile.Amount;
            Mass += sourcePacket.Mass;

            if (!resAmountsPacketsByDestin.ContainsKey(sourcePacket.destination))
                resAmountsPacketsByDestin[sourcePacket.destination] = new(destination: sourcePacket.destination, thermalBody: thermalBody);
            resAmountsPacketsByDestin[sourcePacket.destination].resPile.TransferAllFrom(source: sourcePacket.resPile);
        }

        public void TransferFrom(ResPile source, NodeID destination, AllResAmounts amount)
        {
            ResAmounts += amount;
            Mass += amount.Mass();

            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination, thermalBody: thermalBody);
            resAmountsPacketsByDestin[destination].resPile.TransferFrom(source: source, amount: amount);
        }

        public void TransferAllFrom(ResPile source, NodeID destination)
            => TransferFrom(source: source, destination: destination, amount: source.Amount);

        public ResPile ReturnAndRemove(NodeID destination)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                return ResPile.CreateEmpty(thermalBody: thermalBody);

            var resAmountsPacket = resAmountsPacketsByDestin[destination];
            resAmountsPacketsByDestin.Remove(destination);
            ResAmounts -= resAmountsPacket.resPile.Amount;
            Mass -= resAmountsPacket.Mass;
            return resAmountsPacket.resPile;
        }

        public AllResAmounts ResToDestinAmounts(NodeID destination)
            => resAmountsPacketsByDestin.ContainsKey(destination) switch
            {
                true => resAmountsPacketsByDestin[destination].resPile.Amount,
                false => AllResAmounts.empty
            };

        public IEnumerable<ResAmountsPacket> DeconstructAndClear()
        {
            var result = resAmountsPacketsByDestin.Values;
            resAmountsPacketsByDestin = new();
            ResAmounts = AllResAmounts.empty;
            Mass = Mass.zero;
            return result;
        }
    }
}
