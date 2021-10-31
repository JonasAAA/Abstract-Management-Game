using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1
{
    [DataContract]
    public class ResAmountsPacketsByDestin
    {
        [DataMember]
        public ConstULongArray ResAmounts { get; private set; }
        [DataMember]
        public ulong TotalWeight { get; private set; }
        public bool Empty
            => TotalWeight is 0;

        [DataMember]
        private Dictionary<Vector2, ResAmountsPacket> resAmountsPacketsByDestin;

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

        public void Add(Vector2 destination, ConstULongArray resAmounts)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            resAmountsPacketsByDestin[destination].Add(resAmounts: resAmounts);
            TotalWeight += resAmounts.TotalWeight();
        }

        public void Add(Vector2 destination, int resInd, ulong resAmount)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            resAmountsPacketsByDestin[destination].Add(resInd: resInd, resAmount: resAmount);
            TotalWeight += CurResConfig.resources[resInd].weight * resAmount;
        }

        public ULongArray ReturnAndRemove(Vector2 destination)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                return new();
            
            var resAmountsPacket = resAmountsPacketsByDestin[destination];
            resAmountsPacketsByDestin.Remove(destination);
            TotalWeight -= resAmountsPacket.TotalWeight;
            return resAmountsPacket.ResAmounts.ToULongArray();
        }

        public ULongArray ResToDestinAmounts(Vector2 destination)
            => resAmountsPacketsByDestin.ContainsKey(destination) switch
            {
                true => resAmountsPacketsByDestin[destination].ResAmounts.ToULongArray(),
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
