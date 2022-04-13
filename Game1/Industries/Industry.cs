﻿using Game1.Delegates;
using Game1.Shapes;
using Game1.UI;

namespace Game1.Industries
{
    [Serializable]
    public abstract class Industry : IDeletable
    {
        // all fields and properties in this and derived classes must have unchangeable state
        [Serializable]
        public abstract class Params
        {
            public readonly string name;
            public string Explanation { get; }

            protected Params(string name, string explanation)
            {
                this.name = name;
                Explanation = explanation;
            }

            public abstract bool CanCreateWith(NodeState state);

            public Industry CreateIndustry(NodeState state)
            {
                if (!CanCreateWith(state: state))
                    throw new InvalidOperationException();
                return InternalCreateIndustry(state: state);
            }

            protected abstract Industry InternalCreateIndustry(NodeState state);
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

        protected readonly NodeState state;
        //TODO: implement deletion behaviour, then make all buildings subclasses of this
        //consider turning this into an intherface (though that would lead to deletion code duplication)
        private bool isDeleted;
        private readonly Event<IDeletedListener> deleted;

        protected readonly UIRectPanel<IHUDElement> UIPanel;
        private readonly TextBox textBox;

        protected Industry(NodeState state)
        {
            this.state = state;

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
                explanation: "deletes this industry",
                text: "delete"
            );
            deleteButton.clicked.Add(listener: new DeleteButtonClickedListener(Industry: this));
            UIPanel.AddChild(child: deleteButton);
        }

        public abstract ResAmounts TargetStoredResAmounts();

        public Industry Update()
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
