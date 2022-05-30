using System.Diagnostics.CodeAnalysis;

namespace Game1
{
    [Serializable]
    public sealed class ResAmountsPacketsByDestin
    {
        public ResAmounts ResAmounts { get; private set; }
        public ulong TotalMass { get; private set; }
        public bool Empty
            => TotalMass is 0;

        private Dictionary<NodeID, ResAmountsPacket> resAmountsPacketsByDestin;

        public ResAmountsPacketsByDestin()
        {
            resAmountsPacketsByDestin = new();

            ResAmounts = new();
            TotalMass = 0;
        }

        public void TransferAllFrom([DisallowNull] ref ResAmountsPacketsByDestin? sourcePackets)
        {
            foreach (var resAmountsPacket in sourcePackets.DeconstructAndClear())
            {
                var resAmountsPacketCopy = resAmountsPacket;
                TransferAllFrom(sourcePacket: ref resAmountsPacketCopy);
            }

            sourcePackets = null;
        }

        public void TransferAllFrom([DisallowNull] ref ResAmountsPacket? sourcePacket)
        {
            ResAmounts += sourcePacket.resPile.ResAmounts;
            TotalMass += sourcePacket.TotalMass;

            if (!resAmountsPacketsByDestin.ContainsKey(sourcePacket.destination))
                resAmountsPacketsByDestin[sourcePacket.destination] = new(destination: sourcePacket.destination);
            sourcePacket.resPile.TransferAllTo(destin: resAmountsPacketsByDestin[sourcePacket.destination].resPile);

            sourcePacket = null;
        }

        public void TransferAllFrom([DisallowNull] ref ReservedResPile? source, NodeID destination)
        {
            ResAmounts += source.ResAmounts;
            TotalMass += source.TotalMass;

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
            TotalMass -= resAmountsPacket.TotalMass;
            return resAmountsPacket.resPile;
        }

        public ResAmounts ResToDestinAmounts(NodeID destination)
            => resAmountsPacketsByDestin.ContainsKey(destination) switch
            {
                true => resAmountsPacketsByDestin[destination].resPile.ResAmounts,
                false => new()
            };

        public IEnumerable<ResAmountsPacket> DeconstructAndClear()
        {
            var result = resAmountsPacketsByDestin.Values;
            resAmountsPacketsByDestin = new();
            ResAmounts = new();
            TotalMass = 0;
            return result;
        }
    }
}
