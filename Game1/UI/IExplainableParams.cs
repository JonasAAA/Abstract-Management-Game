namespace Game1.UI
{
    public interface IExplainableParams
    {
        /// <summary>
        /// null when no explanation is available
        /// </summary>
        public string? Explanation { get; }
    }
}
