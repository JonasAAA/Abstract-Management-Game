//using Game1.Delegates;
//using Game1.Lighting;
//using Game1.Shapes;
//using Game1.UI;
//using static Game1.WorldManager;
//using static Game1.UI.ActiveUIManager;
//using Game1.Inhabitants;

//namespace Game1.Industries
//{
//    [Serializable]
//    public abstract class Industry : IWithRealPeopleStats, IDeletable
//    {
//        // TODO: could rename this class to ParamsOfParams or ParamsIndepFromState
//        // all fields and properties in this and derived classes must have unchangeable state
//        [Serializable]
//        public abstract class Factory
//        {
//            public string Name { get; }
//            public Color Color { get; }

//            protected Factory(string Name, Color color)
//            {
//                Name = Name;
//                Color = color;
//            }

//            public abstract GeneralParams CreateParams(IIndustryFacingNodeState state);

//            protected ITooltip Tooltip(IIndustryFacingNodeState state)
//                => (CreateParams(state: state) as IWithTooltip).Tooltip;
//        }

//        [Serializable]
//        public abstract class GeneralParams : IWithTooltip
//        {
//            [Serializable]
//            private sealed class TextTooltip : TextTooltipBase
//            {
//                protected override string Text
//                    => parameters.TooltipText;

//                private readonly GeneralParams parameters;

//                public TextTooltip(GeneralParams parameters)
//                    => this.parameters = parameters;
//            }

//            public readonly IIndustryFacingNodeState state;
//            public readonly string Name;
//            public readonly Color color;

//            private readonly ITooltip tooltip;

//            public virtual string TooltipText
//                => $"{nameof(Name)}: {Name}";

//            public GeneralParams(IIndustryFacingNodeState state, Factory factory)
//            {
//                this.state = state;

//                Name = factory.Name;
//                color = factory.Color;
//                tooltip = new TextTooltip(parameters: this);
//            }

//            ITooltip IWithTooltip.Tooltip
//                => tooltip;
//        }

//        [Serializable]
//        private readonly record struct DeleteButtonClickedListener(Industry Industry) : IClickedListener
//        {
//            void IClickedListener.ClickedResponse()
//                => Industry.isDeleted = true;
//        }

//        [Serializable]
//        private readonly record struct LightCatchingDiskParams(Industry Industry) : Disk.IParams
//        {
//            public MyVector2 Center
//                => Industry.parameters.state.Position;

//            public UDouble Radius
//                => Industry.parameters.state.Radius + Industry.Height;
//        }

//        public IEvent<IDeletedListener> Deleted
//            => deleted;

//        public ILightBlockingObject? LightBlockingObject
//            => lightBlockingDisk.Radius.IsCloseTo(other: parameters.state.Radius) switch
//            {
//                true => null,
//                false => lightBlockingDisk
//            };

//        public abstract bool PeopleWorkOnTop { get; }

//        /// <summary>
//        /// Null if no building
//        /// </summary>
//        public virtual Propor? SurfaceReflectance
//            => building?.Cost.Reflectance();

//        /// <summary>
//        /// Null if no building
//        /// </summary>
//        public virtual Propor? SurfaceEmissivity
//            => building?.Cost.Emissivity();

//        public abstract RealPeopleStats Stats { get; }

//        public IHUDElement UIElement
//            => UIPanel;

//        protected abstract UDouble Height { get; }

//        protected readonly CombinedEnergyConsumer combinedEnergyConsumer;
//        protected readonly UIRectPanel<IHUDElement> UIPanel;

//        private readonly Building? building;
//        private bool isDeleted;
//        private readonly Event<IDeletedListener> deleted;
//        private readonly LightCatchingDisk lightBlockingDisk;
//        private readonly TextBox textBox;
//        private readonly GeneralParams parameters;

//        protected Industry(GeneralParams parameters, Building? building)
//        {
//            this.parameters = parameters;
//            this.building = building;

//            combinedEnergyConsumer = new
//            (
//                nodeID: parameters.state.NodeID,
//                energyDistributor: CurWorldManager.EnergyDistributor
//            );
//            isDeleted = false;
//            deleted = new();

//            lightBlockingDisk = new(parameters: new LightCatchingDiskParams(Industry: this));

//            textBox = new();
//            UIPanel = new UIRectVertPanel<IHUDElement>(childHorizPos: HorizPos.Left);
//            UIPanel.AddChild(child: textBox);
//            Button deleteButton = new
//            (
//                shape: new MyRectangle
//                (
//                    width: 60,
//                    height: 30
//                ),
//                tooltip: new ImmutableTextTooltip(text: "Delete this industry"),
//                text: "delete",
//                color: colorConfig.deleteButtonColor
//            );
//            deleteButton.clicked.Add(listener: new DeleteButtonClickedListener(Industry: this));
//            UIPanel.AddChild(child: deleteButton);
//        }

//        public void UpdatePeople()
//        {
//            UpdatePeopleInternal();
//            textBox.Text = GetInfo();
//        }

//        protected abstract void UpdatePeopleInternal();

//        public abstract ResAmounts TargetStoredResAmounts();

//        public Industry? Update()
//        {
//            if (isDeleted)
//            {
//                PlayerDelete();
//                return null;
//            }
//            if (MyMathHelper.AreClose(CurWorldManager.Elapsed, TimeSpan.Zero))
//                return this;

//            return InternalUpdate();
//        }

//        protected abstract Industry InternalUpdate();

//        protected virtual void PlayerDelete()
//            => Delete();

//        protected virtual void Delete()
//        {
//            building?.Delete(resDestin: parameters.state.StoredResPile);
//            combinedEnergyConsumer.Delete();
//            deleted.Raise(action: listener => listener.DeletedResponse(deletable: this));
//        }

//        public abstract string GetInfo();

//        public virtual void Draw(Color otherColor, Propor otherColorPropor)
//        {
//            if (LightBlockingObject is not null)
//                lightBlockingDisk.Draw(baseColor: parameters.color, otherColor: otherColor, otherColorPropor: otherColorPropor);
//        }

//        public virtual void DrawAfterPlanet()
//        { }
//    }
//}
