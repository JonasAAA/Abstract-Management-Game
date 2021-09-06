﻿using System.Collections.Generic;

namespace Game1
{
    public class ULongArray : ConstULongArray
    {
        public ULongArray()
            : base()
        { }

        public ULongArray(ulong value)
            : base(value: value)
        { }

        public ULongArray(IEnumerable<ulong> values)
            : base(values: values)
        { }

        public new ulong this[int index]
        {
            get => array[index];
            set => array[index] = value;
        }
    }
}