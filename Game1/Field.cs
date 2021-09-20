using System;
using System.Collections.Generic;

namespace Game1
{
    public class Field<T>
    {
        private T value;

        public event Action Changed;

        public Field(T value)
            => this.value = value;

        //public T Get()
        //    => value;

        public static implicit operator T(Field<T> field)
            => field.value;

        public void Set(T value)
        {
            if (EqualityComparer<T>.Default.Equals(this.value, value))
                return;

            this.value = value;
            Changed?.Invoke();
        }
    }
}
