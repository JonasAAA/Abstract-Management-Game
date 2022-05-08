using Game1.Delegates;
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

            protected Factory(string name)
                => this.name = name;

            public abstract Industry CreateIndustry(NodeState state);

            public abstract Params CreateParams(NodeState state);

            protected ITooltip Tooltip(NodeState state)
                => (CreateParams(state: state) as IWithTooltip).Tooltip;
        }

        [Serializable]
        public abstract class Params: IWithTooltip
        {
            [Serializable]
            private class TextTooltip : TextTooltipBase
            {
                protected override string Text
                    => parameters.TooltipText;

                private readonly Params parameters;

                public TextTooltip(Params parameters)
                    => this.parameters = parameters;
            }

            public readonly NodeState state;
            public readonly string name;

            private readonly ITooltip tooltip;

            public virtual string TooltipText
                => $"{nameof(name)}: {name}\n";

            public Params(NodeState state, Factory factory)
            {
                this.state = state;

                name = factory.name;
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

        public IEvent<IDeletedListener> Deleted
            => deleted;

        public abstract IEnumerable<Person> PeopleHere { get; }

        public IHUDElement UIElement
            => UIPanel;

        //TODO: implement deletion behaviour, then make all buildings subclasses of this
        //consider turning this into an intherface (though that would lead to deletion code duplication)
        private bool isDeleted;
        private readonly Event<IDeletedListener> deleted;

        protected readonly UIRectPanel<IHUDElement> UIPanel;
        private readonly TextBox textBox;
        private readonly Params parameters;

        protected Industry(Params parameters)
        {
            this.parameters = parameters;
            isDeleted = false;
            deleted = new();

            textBox = new();
            UIPanel = new UIRectVertPanel<IHUDElement>(color: Color.White, childHorizPos: HorizPos.Left);
            UIPanel.AddChild(child: textBox);
            Button deleteButton = new
            (
                shape: new MyRectangle
                (
                    width: 60,
                    height: 30
                )
                {
                    Color = Color.Red
                },
                tooltip: new ImmutableTextTooltip(text: "Delete this industry"),
                text: "delete"
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
    }
}
