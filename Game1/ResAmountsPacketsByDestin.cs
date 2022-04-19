using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public class ResAmountsPacketsByDestin
    {
        public ResAmounts ResAmounts { get; private set; }
        public ulong TotalWeight { get; private set; }
        public bool Empty
            => TotalWeight is 0;

        private Dictionary<NodeId, ResAmountsPacket> resAmountsPacketsByDestin;

        public ResAmountsPacketsByDestin()
        {
            resAmountsPacketsByDestin = new();

            ResAmounts = new();
            TotalWeight = 0;
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
            TotalWeight += resAmountsPacket.TotalWeight;
        }

        public void Add(NodeId destination, ResAmounts resAmounts)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            resAmountsPacketsByDestin[destination].Add(resAmounts: resAmounts);
            TotalWeight += resAmounts.TotalWeight();
        }

        public void Add(NodeId destination, ResInd resInd, ulong resAmount)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            resAmountsPacketsByDestin[destination].Add(resInd: resInd, resAmount: resAmount);
            TotalWeight += CurResConfig.resources[resInd].mass * resAmount;
        }

        public ResAmounts ReturnAndRemove(NodeId destination)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                return new();
            
            var resAmountsPacket = resAmountsPacketsByDestin[destination];
            resAmountsPacketsByDestin.Remove(destination);
            TotalWeight -= resAmountsPacket.TotalWeight;
            return resAmountsPacket.ResAmounts;
        }

        public ResAmounts ResToDestinAmounts(NodeId destination)
            => resAmountsPacketsByDestin.ContainsKey(destination) switch
            {
                true => resAmountsPacketsByDestin[destination].ResAmounts,
                false => new()
            };

        public IEnumerable<ResAmountsPacket> DeconstructAndClear()
        {
            var result = resAmountsPacketsByDestin.Values;
            resAmountsPacketsByDestin = new();
            TotalWeight = 0;
            return result;
        }
    }
}
