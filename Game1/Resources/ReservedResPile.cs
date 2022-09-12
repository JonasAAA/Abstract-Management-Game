﻿using System.Diagnostics.CodeAnalysis;

namespace Game1.Resources
{
    [Serializable]
    public class ReservedResPile : ResPileBase
    {
        public static ReservedResPile? CreateIfHaveEnough(ResPile source, ResAmounts resAmounts)
        {
            if (source.ResAmounts >= resAmounts)
            {
                ReservedResPile resPile = new(locationCounters: source.LocationCounters);
                Transfer(source: source, destin: resPile, resAmounts: resAmounts);
                return resPile;
            }
            return null;
        }

        public static ReservedResPile? CreateIfHaveEnough(ResPile source, ResAmount resAmount)
            => CreateIfHaveEnough(source: source, resAmounts: new(resAmount: resAmount));

        public static ReservedResPile CreateFromSource([DisallowNull] ref ReservedResPile? source)
        {
            ReservedResPile resPile = new(locationCounters: source.LocationCounters);
            TransferAllFrom(source: source, destin: resPile);
            source = null;
            return resPile;
        }

        private ReservedResPile(LocationCounters locationCounters)
            : base(locationCounters: locationCounters)
        { }
    }
}
