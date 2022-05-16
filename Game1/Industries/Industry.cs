using Game1.Delegates;
using Game1.Lighting;
using Game1.Shapes;
using Game1.UI;

namespace Game1.Industries
{
    [Serializable]
    public abstract class Industry : IDeletable
    {
        // all fields and properties in this and derived classes must have unchangeable state
        [Serializable]
        public abstract class Factory
        {
            public readonly string name;
            public readonly Color color;

            protected Factory(string name, Color color)
            {
                this.name = name;
                this.color = color;
            }

            public abstract Industry CreateIndustry(NodeState state);

            public abstract Params CreateParams(NodeState state);

            protected ITooltip Tooltip(NodeState state)
                => (CreateParams(state: state) as IWithTooltip).Tooltip;
        }

        [Serializable]
        public abstract class Params : IWithTooltip
        {
            [Serializable]
            private sealed class TextTooltip : TextTooltipBase
            {
                protected override string Text
                    => parameters.TooltipText;

                private readonly Params parameters;

                public TextTooltip(Params parameters)
                    => this.parameters = parameters;
            }

            public readonly NodeState state;
            public readonly string name;
            public readonly Color color;

            private readonly ITooltip tooltip;

            public virtual string TooltipText
                => $"{nameof(name)}: {name}\n";

            public Params(NodeState state, Factory factory)
            {
                this.state = state;

                name = factory.name;
                color = factory.color;
                tooltip = new TextTooltip(parameters: this);
            }

            ITooltip IWithTooltip.Tooltip
                => tooltip;
        }

        [Serializable]
        private readonly record struct DeleteButtonClickedListener(Industry Industry) : IClickedListener
        {
            void IClickedListener.ClickedResponse()
                => Industry.isDeleted = true;
        }

        [Serializable]
        private readonly record struct LightCatchingDiskParams(Industry Industry) : Disk.IParams
        {
            public MyVector2 Center
                => Industry.parameters.state.position;

            public UDouble Radius
                => Industry.parameters.state.Radius + Industry.Height;
        }

        public IEvent<IDeletedListener> Deleted
            => deleted;

        public abstract IEnumerable<Person> PeopleHere { get; }

        public ILightBlockingObject? LightBlockingObject
            => lightCatchingDisk.Radius.IsCloseTo(other: parameters.state.Radius) switch
            {
                true => null,
                false => lightCatchingDisk
            };

        public abstract bool PeopleWorkOnTop { get; }

        public IHUDElement UIElement
            => UIPanel;

        //TODO: implement deletion behaviour, then make all buildings subclasses of this
        //consider turning this into an intherface (though that would lead to deletion code duplication)
        
        protected abstract UDouble Height { get; }

        protected readonly UIRectPanel<IHUDElement> UIPanel;

        private bool isDeleted;
        private readonly Event<IDeletedListener> deleted;

        private readonly LightCatchingDisk lightCatchingDisk;
        private readonly TextBox textBox;
        private readonly Params parameters;

        protected Industry(Params parameters)
        {
            this.parameters = parameters;
            isDeleted = false;
            deleted = new();

            lightCatchingDisk = new(parameters: new LightCatchingDiskParams(Industry: this));

            textBox = new();
            UIPanel = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPos.Left);
            UIPanel.AddChild(child: textBox);
            Button deleteButton = new
            (
                shape: new MyRectangle
                (
                    width: 60,
                    height: 30
                ),
                tooltip: new ImmutableTextTooltip(text: "Delete this industry"),
                text: "delete",
                color: Color.Red
            );
            deleteButton.clicked.Add(listener: new DeleteButtonClickedListener(Industry: this));
            UIPanel.AddChild(child: deleteButton);
        }

        public abstract ResAmounts TargetStoredResAmounts();

        public Industry? Update()
        {
            if (isDeleted)
            {
                PlayerDelete();
                return null;
            }

            var result = InternalUpdate();

            textBox.Text = GetInfo();

            return result;
        }

        protected abstract Industry InternalUpdate();

        protected virtual void PlayerDelete()
            => Delete();

        protected virtual void Delete()
            => deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));

        public abstract string GetInfo();

        public virtual void DrawBeforePlanet(Color otherColor, Propor otherColorPropor)
        {
            if (LightBlockingObject is not null)
                lightCatchingDisk.Draw(baseColor: parameters.color, otherColor: otherColor, otherColorPropor: otherColorPropor);
        }

        public virtual void DrawAfterPlanet()
        { }
    }
}
