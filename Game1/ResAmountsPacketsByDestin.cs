using System.Diagnostics.CodeAnalysis;

namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacketsByDestin : IHasMass
    {
        public static ResAmountsPacketsByDestin CreateEmpty()
            => new();

        public static ResAmountsPacketsByDestin CreateFromSource(ResAmountsPacketsByDestin sourcePackets)
        {
            ResAmountsPacketsByDestin newPackets = new();
            newPackets.TransferAllFrom(sourcePackets: sourcePackets);
            return newPackets;
        }

        public ResAmounts ResAmounts { get; private set; }
        public ulong Mass { get; private set; }
        public bool Empty
            => Mass is 0;

        private Dictionary<NodeID, ResAmountsPacket> resAmountsPacketsByDestin;

        private ResAmountsPacketsByDestin()
        {
            resAmountsPacketsByDestin = new();

            ResAmounts = ResAmounts.Empty;
            Mass = 0;
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
                resAmountsPacketsByDestin[sourcePacket.destination] = new(destination: sourcePacket.destination);
            sourcePacket.resPile.TransferAllTo(destin: resAmountsPacketsByDestin[sourcePacket.destination].resPile);
        }

        public void TransferAllFrom([DisallowNull] ref ReservedResPile? source, NodeID destination)
        {
            ResAmounts += source.ResAmounts;
            Mass += source.Mass;

            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            ReservedResPile.TransferAll(reservedSource: ref source, destin: resAmountsPacketsByDestin[destination].resPile);
        }

        public ResPile ReturnAndRemove(NodeID destination)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                return ResPile.CreateEmpty();

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
            Mass = 0;
            return result;
        }
    }
}
