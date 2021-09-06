using System;
using System.Collections.Generic;

namespace Game1
{
    public class ResAmountsPacketsByDestin
    {
        public ConstULongArray ResAmounts { get; private set; }
        public ulong TotalWeight { get; private set; }
        public bool Empty
            => TotalWeight is 0;

        private Dictionary<Position, ResAmountsPacket> resAmountsPacketsByDestin;

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

        public void Add(Position destination, ConstULongArray resAmounts)
        {
            if (destination is null)
                throw new ArgumentNullException();

            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            resAmountsPacketsByDestin[destination].Add(resAmounts: resAmounts);
            TotalWeight += resAmounts.TotalWeight();
        }

        public void Add(Position destination, int resInd, ulong resAmount)
        {
            if (destination is null)
                throw new ArgumentNullException();

            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                resAmountsPacketsByDestin[destination] = new(destination: destination);
            resAmountsPacketsByDestin[destination].Add(resInd: resInd, resAmount: resAmount);
            TotalWeight += Resource.all[resInd].weight * resAmount;
        }

        public ULongArray ReturnAndRemove(Position destination)
        {
            if (!resAmountsPacketsByDestin.ContainsKey(destination))
                return new();
            
            var resAmountsPacket = resAmountsPacketsByDestin[destination];
            resAmountsPacketsByDestin.Remove(destination);
            TotalWeight -= resAmountsPacket.TotalWeight;
            return resAmountsPacket.ResAmounts.ToULongArray();
        }

        public ULongArray ResToDestinAmounts(Position destination)
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
