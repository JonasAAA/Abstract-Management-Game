namespace Game1.UI
{
    public interface IWithTooltip : IMaybeWithTooltip
    {
        public new ITooltip Tooltip { get; }

        ITooltip? IMaybeWithTooltip.Tooltip
            => Tooltip;
    }
}
