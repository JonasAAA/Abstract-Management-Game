using System.Diagnostics.CodeAnalysis;

namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacketsByDestin : IHasMass
    {
        public static ResAmountsPacketsByDestin CreateEmpty(MassCounter locationMassCounter)
            => new(locationMassCounter: locationMassCounter);

        public static ResAmountsPacketsByDestin CreateFromSource(ResAmountsPacketsByDestin sourcePackets)
            => CreateFromSource(sourcePackets: sourcePackets, locationMassCounter: sourcePackets.locationMassCounter);

        public static ResAmountsPacketsByDestin CreateFromSource(ResAmountsPacketsByDestin sourcePackets, MassCounter locationMassCounter)
        {
            ResAmountsPacketsByDestin newPackets = new(locationMassCounter: locationMassCounter);
            newPackets.TransferAllFrom(sourcePackets: sourcePackets);
            return newPackets;
        }

        public ResAmounts ResAmounts { get; private set; }
        public Mass Mass { get; private set; }
        public bool Empty
            => Mass.IsZero;

        private readonly MassCounter locationMassCounter;
        private Dictionary<NodeID, ResAmountsPacket> resAmountsPacketsByDestin;

        private ResAmountsPacketsByDestin(MassCounter locationMassCounter)
        {
            this.locationMassCounter = locationMassCounter;
            resAmountsPacketsByDestin = new();

            ResAmounts = ResAmounts.Empty;
            Mass = Mass.zero;
        }

        public void TransferAllFrom(ResAmountsPacketsByDestin sourcePackets)
        {
            foreach (var resAmountsPacket in sourcePackets.DeconstructAndClear())
                TransferAllFrom(sourcePacket: resAmountsPacket);
        }

        public void TransferAllFrom(ResAmountsPacket sourcePacket)
        {
            ResAmounts += sourcePacket.resPile.ResAmounts;
            Mass += sourcePacket.Mass;

            if (!resAmountsPacketsByDestin.ContainsKey(sourcePacket.destination))
                resAmountsPacketsByDestin[sourcePacket.destination] = new(destination: sourcePacket.destination, locationMassCounter: locationMassCounter);
            resAmountsPacketsByDestin[sourcePacket.destination].resPile.TransferAllFrom(source: sourcePacket.resPile);
        }

        public void TransferAllFrom([DisallowNull] ref ReservedResPile? source, NodeID destination)
        {
            ResAmounts += source.ResAmounts;
            Mass += source.Mass;

            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination, locationMassCounter: locationMassCounter);
            ReservedResPile.TransferAllFrom(reservedSource: ref source, destin: resAmountsPacketsByDestin[destination].resPile);
        }

        public ResPile ReturnAndRemove(NodeID destination)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                return ResPile.CreateEmpty(locationMassCounter: locationMassCounter);

            var resAmountsPacket = resAmountsPacketsByDestin[destination];
            resAmountsPacketsByDestin.Remove(destination);
            ResAmounts -= resAmountsPacket.resPile.ResAmounts;
            Mass -= resAmountsPacket.Mass;
            return resAmountsPacket.resPile;
        }

        public ResAmounts ResToDestinAmounts(NodeID destination)
            => resAmountsPacketsByDestin.ContainsKey(destination) switch
            {
                true => resAmountsPacketsByDestin[destination].resPile.ResAmounts,
                false => ResAmounts.Empty
            };

        public IEnumerable<ResAmountsPacket> DeconstructAndClear()
        {
            var result = resAmountsPacketsByDestin.Values;
            resAmountsPacketsByDestin = new();
            ResAmounts = ResAmounts.Empty;
            Mass = Mass.zero;
            return result;
        }
    }
}
