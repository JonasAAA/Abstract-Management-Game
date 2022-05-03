namespace Game1.UI
{
    public interface IExplainableUIElement
    {
        /// <summary>
        /// null when no explanation is available
        /// </summary>
        public string? Explanation { get; }
    }
}
