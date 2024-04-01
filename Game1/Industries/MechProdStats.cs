namespace Game1.Industries
{
    /// <summary>
    /// <paramref name="ReqWatts"/> doesn't use type ElectricalEnergy as that can only be integer
    /// </summary>
    [Serializable]
    public readonly record struct MechProdStats(UDouble ReqWatts, Result<AreaDouble, TextErrors> ProducedAreaPerSecOrPauseReasons);
}
