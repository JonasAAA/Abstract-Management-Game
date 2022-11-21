using System.Diagnostics.CodeAnalysis;

namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacketsByDestin
    {
        public static ResAmountsPacketsByDestin CreateEmpty(LocationCounters locationCounters)
            => new(locationCounters: locationCounters);

        public static ResAmountsPacketsByDestin CreateFromSource(ResAmountsPacketsByDestin sourcePackets)
            => CreateFromSource(sourcePackets: sourcePackets, locationCounters: sourcePackets.locationCounters);

        public static ResAmountsPacketsByDestin CreateFromSource(ResAmountsPacketsByDestin sourcePackets, LocationCounters locationCounters)
        {
            ResAmountsPacketsByDestin newPackets = new(locationCounters: locationCounters);
            newPackets.TransferAllFrom(sourcePackets: sourcePackets);
            return newPackets;
        }

        public ResAmounts ResAmounts { get; private set; }
        public Mass Mass { get; private set; }
        public bool Empty
            => Mass.isZero;

        private readonly LocationCounters locationCounters;
        private Dictionary<NodeID, ResAmountsPacket> resAmountsPacketsByDestin;

        private ResAmountsPacketsByDestin(LocationCounters locationCounters)
        {
            this.locationCounters = locationCounters;
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
                resAmountsPacketsByDestin[sourcePacket.destination] = new(destination: sourcePacket.destination, locationCounters: locationCounters);
            resAmountsPacketsByDestin[sourcePacket.destination].resPile.TransferAllFrom(source: sourcePacket.resPile);
        }

        public void TransferAllFrom([DisallowNull] ref ReservedResPile? source, NodeID destination)
        {
            ResAmounts += source.ResAmounts;
            Mass += source.Mass;

            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination, locationCounters: locationCounters);
            resAmountsPacketsByDestin[destination].resPile.TransferAllFrom(reservedSource: ref source);
        }

        public ResPile ReturnAndRemove(NodeID destination)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                return ResPile.CreateEmpty(locationCounters: locationCounters);

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
