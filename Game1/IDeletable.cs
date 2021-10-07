using System;

namespace Game1
{
    public interface IDeletable
    {
        public event Action Deleted;
    }
}
