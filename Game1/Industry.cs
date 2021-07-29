using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Game1
{
    public class Industry
    {
        public class Params
        {
            public readonly string name;
            public readonly ReadOnlyCollection<Upgrade> upgrades;

            public Params(string name, List<Upgrade> upgrades)
            {
                this.name = name;
                this.upgrades = new(upgrades);
            }

            public virtual Industry MakeIndustry(NodeState state)
                => new(parameters: this, state: state);
        }

        public class Upgrade
        {
            public readonly Params parameters;
            public readonly TimeSpan duration;
            public readonly ConstUIntArray cost;
            public readonly KeyButton keyButton;

            public Upgrade(Params parameters, TimeSpan minTime, ConstUIntArray cost, Keys key)
            {
                this.parameters = parameters;
                if (minTime.TotalSeconds < 0)
                    throw new ArgumentException();
                this.duration = minTime;
                this.cost = cost;
                keyButton = new(key: key);
            }
        }

        public static readonly Params emptyParams;

        static Industry()
        {
            Factory.Params factory2 = new
            (
                name: nameof(factory2),
                upgrades: new(),
                supply: new()
                {
                    [0] = 100,
                },
                demand: new(),
                prodTime: TimeSpan.FromSeconds(value: 2)
            );

            Factory.Params factory1 = new
            (
                name: nameof(factory1),
                upgrades: new()
                {
                    new
                    (
                        parameters: factory2,
                        minTime: TimeSpan.FromSeconds(5),
                        cost: new(),
                        key: Keys.D2
                    ),
                },
                supply: new()
                {
                    [0] = 10,
                },
                demand: new(),
                prodTime: TimeSpan.FromSeconds(value: 5)
            );

            emptyParams = new Params
            (
                name: "empty",
                upgrades: new()
                {
                    new
                    (
                        parameters: factory1,
                        minTime: TimeSpan.FromSeconds(5),
                        cost: new(),
                        key: Keys.D1
                    ),
                }
            );
        }

        protected readonly NodeState state;
        protected virtual bool IsProducing
            => false;

        private bool canStartProduction;
        private readonly Params parameters;
        private Upgrade activeUpgrade;
        private TimeSpan? upgradeEndTime;
        
        public Industry(Params parameters, NodeState state)
        {
            this.parameters = parameters;
            this.state = state;
            canStartProduction = true;
            activeUpgrade = null;
            upgradeEndTime = null;
        }

        public void ActiveUpdate()
        {
            if (activeUpgrade is not null)
                return;
            foreach (var upgrade in parameters.upgrades)
            {
                upgrade.keyButton.Update();
                if (upgrade.keyButton.Click && state.stored >= upgrade.cost)
                {
                    activeUpgrade = upgrade;
                    canStartProduction = false;
                }
            }
        }

        public void StartProductionIfCan()
        {
            if (canStartProduction)
                StartProduction();
            if (!IsProducing && activeUpgrade is not null && !upgradeEndTime.HasValue)
                upgradeEndTime = C.TotalGameTime + activeUpgrade.duration;
        }

        protected virtual void StartProduction()
        { }

        public virtual Industry FinishProduction()
        {
            if (activeUpgrade is not null && upgradeEndTime.HasValue && upgradeEndTime < C.TotalGameTime)
                return activeUpgrade.parameters.MakeIndustry(state: state);
            return this;
        }

        public virtual string GetText()
            => parameters.name;

        public void Draw()
        {
            SpriteFont font = C.Content.Load<SpriteFont>("font");
            string text = GetText();
            if (activeUpgrade is not null)
            {
                if (upgradeEndTime.HasValue)
                    text = $"upgrading {C.DonePart(endTime: upgradeEndTime.Value, duration: activeUpgrade.duration) * 100 : 0.}%";
                else
                    text = "waiting to upgrade";
            }
            C.SpriteBatch.DrawString
            (
                spriteFont: font,
                text: text,
                position: state.position,
                color: Color.Black,
                rotation: 0,
                origin: Vector2.Zero,
                scale: .15f,
                effects: SpriteEffects.None,
                layerDepth: 0
            );
        }
    }
}
