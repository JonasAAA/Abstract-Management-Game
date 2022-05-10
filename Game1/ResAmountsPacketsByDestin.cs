using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class ResAmountsPacketsByDestin
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

        public void Add(ResAmountsPacketsByDestin resAmountsPackets)
        {
            foreach (var resAmountsPacket in resAmountsPackets.resAmountsPacketsByDestin.Values)
                Add(resAmountsPacket: resAmountsPacket);
        }

        public void Add(ResAmountsPacket resAmountsPacket)
        {
            if (resAmountsPacketsByDestin.ContainsKey(resAmountsPacket.destination))
                resAmountsPacketsByDestin[resAmountsPacket.destination].Add(resAmountsPacket: resAmountsPacket);
            else
                resAmountsPacketsByDestin[resAmountsPacket.destination] = resAmountsPacket;

            ResAmounts += resAmountsPacket.ResAmounts;
            TotalMass += resAmountsPacket.TotalMass;
        }

        public void Add(NodeID destination, ResAmounts resAmounts)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            resAmountsPacketsByDestin[destination].Add(resAmounts: resAmounts);
            TotalMass += resAmounts.TotalMass();
        }

        public void Add(NodeID destination, ResInd resInd, ulong resAmount)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            resAmountsPacketsByDestin[destination].Add(resInd: resInd, resAmount: resAmount);
            TotalMass += CurResConfig.resources[resInd].Mass * resAmount;
        }

        public ResAmounts ReturnAndRemove(NodeID destination)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                return new();
            
            var resAmountsPacket = resAmountsPacketsByDestin[destination];
            resAmountsPacketsByDestin.Remove(destination);
            TotalMass -= resAmountsPacket.TotalMass;
            return resAmountsPacket.ResAmounts;
        }

        public ResAmounts ResToDestinAmounts(NodeID destination)
            => resAmountsPacketsByDestin.ContainsKey(destination) switch
            {
                true => resAmountsPacketsByDestin[destination].ResAmounts,
                false => new()
            };

        public IEnumerable<ResAmountsPacket> DeconstructAndClear()
        {
            var result = resAmountsPacketsByDestin.Values;
            resAmountsPacketsByDestin = new();
            TotalMass = 0;
            return result;
        }
    }
}
