namespace Game1.GameStates
{
    [Serializable]
    public abstract class GameState
    {
        public virtual void OnEnter()
        { }

        public virtual void OnLeave()
        { }

        public abstract void Update(TimeSpan elapsed);

        public abstract void Draw();
    }
}
