using static Game1.WorldManager;

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

        public void TransferAllFrom(ResAmountsPacketsByDestin sourcePackets)
        {
            foreach (var resAmountsPacket in sourcePackets.resAmountsPacketsByDestin.Values)
                TransferAllFrom(sourcePacket: resAmountsPacket);
        }

        public void TransferAllFrom(ResAmountsPacket sourcePacket)
        {
            ResAmounts += sourcePacket.resPile.ResAmounts;
            TotalMass += sourcePacket.TotalMass;

            if (!resAmountsPacketsByDestin.ContainsKey(sourcePacket.destination))
                resAmountsPacketsByDestin[sourcePacket.destination] = new(destination: sourcePacket.destination);
            ResPile.TransferAll(source: sourcePacket.resPile, destin: resAmountsPacketsByDestin[sourcePacket.destination].resPile);
        }

        // TODO: delete if unused
        //public void TransferAll(NodeID destination, OldResAmounts resAmounts)
        //{
        //    if (!resAmountsPacketsByDestin.ContainsKey(destination))
        //        resAmountsPacketsByDestin[destination] = new(destination: destination);
        //    resAmountsPacketsByDestin[destination].TransferAllFrom(resAmounts: resAmounts);
        //    TotalMass += resAmounts.TotalMass();
        //}

        public void TransferFrom(ResPile source, ResAmount resAmount, NodeID destination)
        {
            ResAmounts += new ResAmounts(resAmount: resAmount);
            TotalMass += CurResConfig.resources[resAmount.resInd].Mass * resAmount.amount;

            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            ResPile.Transfer(source: source, destin: resAmountsPacketsByDestin[destination].resPile, resAmount: resAmount);
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
