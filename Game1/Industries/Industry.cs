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
            public readonly string explanation;

            protected Factory(string name, string explanation)
            {
                this.name = name;
                this.explanation = explanation;
            }

            public abstract Industry CreateIndustry(NodeState state);
        }

        [Serializable]
        public abstract class Params
        {
            public readonly NodeState state;
            public readonly string name;
            public readonly string explanation;

            public Params(NodeState state, Factory factory)
            {
                this.state = state;

                name = factory.name;
                explanation = factory.explanation;
            }
        }

        [Serializable]
        private readonly record struct TextBoxParams(Industry Industry) : TextBox.IParams
        {
            public string? Text
                => Industry.GetInfo();

            public Color BackgroundColor
                => Color.Transparent;
        }

        private readonly record struct DeleteButtonParams(Industry Industry) : Button.IParams, MyRectangle.IParams
        {
            public string? Text
                => "delete";

            public string? Explanation
                => "delete the industry from this planet";

            public Color Color
                => Industry.DeleteButtonColor;

            public Color BackgroundColor
                => Industry.DeleteButtonColor;
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

            UIPanel = new UIRectVertPanel<IHUDElement>
            (
                parameters: new UIRectVertPanel<IHUDElement>.ImmutableParams
                (
                    backgroundColor: Color.White
                ),
                childHorizPos: HorizPos.Left
            );
            textBox = new(parameters: new TextBoxParams(Industry: this));
            UIPanel.AddChild(child: textBox);
            DeleteButtonParams deleteButtonParams = new(Industry: this);
            Button deleteButton = new
            (
                // TODO: move the constants to appropriate file
                shape: new MyRectangle
                (
                    width: 60,
                    height: 30,
                    parameters: deleteButtonParams
                ),
                parameters: deleteButtonParams
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
            return InternalUpdate();

            // TODO: delete
            //var result = InternalUpdate();

            //textBox.Text = GetInfo();

            //return result;
        }

        protected abstract Industry InternalUpdate();

        protected virtual void PlayerDelete()
            => Delete();

        protected virtual void Delete()
            => deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));

        public abstract string GetInfo();

        protected virtual Color DeleteButtonColor
            => Color.Red;
    }
}
