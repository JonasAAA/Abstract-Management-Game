using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.ObjectModel;

namespace Game1
{
    public abstract class Industry
    {
        // all fields and properties in this and derived classes must have unchangeable state
        public abstract class Params
        {
            public readonly IndustryType industryType;
            public readonly string name;
            
            public Params(IndustryType industryType, string name)
            {
                this.industryType = industryType;
                this.name = name;
            }

            public abstract Industry MakeIndustry(NodeState state);
        }

        public static readonly uint TypeCount;
        public static readonly ReadOnlyCollection<Construction.Params> constrBuildingParams;

        static Industry()
        {
            TypeCount = (uint)Enum.GetValues<IndustryType>().Length;

            constrBuildingParams = new(list: new Construction.Params[]
            {
                new
                (
                    name: "factory costruction",
                    industrParams: new Factory.Params
                    (
                        name: "factory0_lvl1",
                        supply: new()
                        {
                            [0] = 10,
                        },
                        demand: new(),
                        prodTime: TimeSpan.FromSeconds(value: 2),
                        reqWattsPerSec: 10,
                        reqSkill: 10
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new(),
                    reqWattsPerSec: 100
                ),
                new
                (
                    name: "factory costruction",
                    industrParams: new Factory.Params
                    (
                        name: "factory1_lvl1",
                        supply: new()
                        {
                            [1] = 10,
                        },
                        demand: new(),
                        prodTime: TimeSpan.FromSeconds(value: 2),
                        reqWattsPerSec: 10,
                        reqSkill: 10
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new(),
                    reqWattsPerSec: 100
                ),
                new
                (
                    name: "factory costruction",
                    industrParams: new Factory.Params
                    (
                        name: "factory2_lvl1",
                        supply: new()
                        {
                            [2] = 10,
                        },
                        demand: new(),
                        prodTime: TimeSpan.FromSeconds(value: 2),
                        reqWattsPerSec: 10,
                        reqSkill: 10
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new(),
                    reqWattsPerSec: 100
                ),
                new
                (
                    name: "factory costruction",
                    industrParams: new PowerPlant.Params
                    (
                        name: "power_plant_lvl1",
                        prodWattsPerSec: 1000
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new(),
                    reqWattsPerSec: 100
                ),
                new
                (
                    name: "factory costruction",
                    industrParams: new Factory.Params
                    (
                        name: "factory0_lvl2",
                        supply: new()
                        {
                            [0] = 100,
                        },
                        demand: new()
                        {
                            [1] = 50,
                        },
                        prodTime: TimeSpan.FromSeconds(value: 2),
                        reqWattsPerSec: 10,
                        reqSkill: 10
                    ),
                    duration: TimeSpan.FromSeconds(5),
                    cost: new()
                    {
                        [1] = 20,
                    },
                    reqWattsPerSec: 100
                ),
            });
        }

        protected readonly NodeState state;

        protected bool CanStartProduction { get; private set; }
        private readonly Params parameters;
        private readonly KeyButton togglePauseButton;
        
        protected Industry(Params parameters, NodeState state)
        {
            this.parameters = parameters;
            this.state = state;
            CanStartProduction = true;
            togglePauseButton = new
            (
                key: Keys.P,
                action: () => CanStartProduction = !CanStartProduction
            );
        }

        public abstract ULongArray TargetStoredResAmounts();

        public abstract ulong ReqWattsPerSec();

        public abstract ulong ProdWattsPerSec();

        public void ActiveUpdate()
        {
            togglePauseButton.Update();
            //if (upgrade is not null)
            //    return;
            //foreach (var upgrade in parameters.upgrades)
            //{
            //    upgrade.keyButton.Update();
            //    if (upgrade.keyButton.Click)
            //    {
            //        this.upgrade = upgrade;
            //        CanStartProduction = false;
            //    }
            //}
        }

        public abstract Industry Update();

        public abstract string GetText();
        
        public void Draw()
        {
            C.SpriteBatch.DrawString
            (
                spriteFont: C.Content.Load<SpriteFont>("font"),
                text: GetText(),
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



//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;

//namespace Game1
//{
//    public class Industry
//    {
//        public class Params
//        {
//            public readonly string name;
//            public readonly IndustryType industryType;
//            public readonly ReadOnlyCollection<Upgrade> upgrades;

//            public Params(string name, IndustryType industryType, List<Upgrade> upgrades)
//            {
//                this.name = name;
//                this.industryType = industryType;
//                this.upgrades = new(upgrades);
//            }

//            public virtual Industry MakeIndustry(NodeState state)
//                => new(parameters: this, state: state);
//        }

//        public class Upgrade
//        {
//            public readonly Params parameters;
//            public readonly TimeSpan duration;
//            public readonly ConstULongArray cost;
//            public readonly ulong reqWattsPerSec;
//            public readonly KeyButton keyButton;

//            public Upgrade(Params parameters, TimeSpan duration, ConstULongArray cost, ulong reqWattsPerSec, Keys key)
//            {
//                this.parameters = parameters;
//                if (duration < TimeSpan.Zero)
//                    throw new ArgumentException();
//                this.duration = duration;
//                this.cost = cost;
//                if (reqWattsPerSec < 0)
//                    throw new ArgumentOutOfRangeException();
//                this.reqWattsPerSec = reqWattsPerSec;
//                keyButton = new(key: key);
//            }
//        }

//        private enum UpgrStage
//        {
//            None,
//            Queued,
//            Upgrading
//        }

//        public static readonly uint TypeCount;
//        public static readonly Params emptyParams;

//        static Industry()
//        {
//            TypeCount = (uint)Enum.GetValues<IndustryType>().Length;

//            Factory.Params factory0_lvl2 = new
//            (
//                name: nameof(factory0_lvl2),
//                upgrades: new(),
//                supply: new()
//                {
//                    [0] = 100,
//                },
//                demand: new()
//                {
//                    [1] = 50,
//                },
//                prodTime: TimeSpan.FromSeconds(value: 2),
//                reqWattsPerSec: 10,
//                reqSkill: 10
//            );

//            Factory.Params factory1_lvl2 = new
//            (
//                name: nameof(factory1_lvl2),
//                upgrades: new(),
//                supply: new()
//                {
//                    [1] = 100,
//                },
//                demand: new()
//                {
//                    [0] = 50,
//                },
//                prodTime: TimeSpan.FromSeconds(value: 2),
//                reqWattsPerSec: 10,
//                reqSkill: 10
//            );

//            Factory.Params factory2_lvl2 = new
//            (
//                name: nameof(factory2_lvl2),
//                upgrades: new(),
//                supply: new()
//                {
//                    [2] = 100,
//                },
//                demand: new()
//                {
//                    [0] = 50,
//                    [1] = 50,
//                },
//                prodTime: TimeSpan.FromSeconds(value: 2),
//                reqWattsPerSec: 10,
//                reqSkill: 10
//            );

//            PowerPlant.Params power_plant_lvl2 = new
//            (
//                name: nameof(power_plant_lvl2),
//                upgrades: new(),
//                prodWattsPerSec: 10000
//            );

//            Factory.Params factory0_lvl1 = new
//            (
//                name: nameof(factory0_lvl1),
//                upgrades: new()
//                {
//                    new
//                    (
//                        parameters: factory0_lvl2,
//                        duration: TimeSpan.FromSeconds(5),
//                        cost: new()
//                        {
//                            [2] = 10
//                        },
//                        reqWattsPerSec: 10,
//                        key: Keys.D1
//                    ),
//                },
//                supply: new()
//                {
//                    [0] = 10,
//                },
//                demand: new(),
//                prodTime: TimeSpan.FromSeconds(value: 5),
//                reqWattsPerSec: 10,
//                reqSkill: 10
//            );

//            Factory.Params factory1_lvl1 = new
//            (
//                name: nameof(factory1_lvl1),
//                upgrades: new()
//                {
//                    new
//                    (
//                        parameters: factory1_lvl2,
//                        duration: TimeSpan.FromSeconds(5),
//                        cost: new(),
//                        reqWattsPerSec: 10,
//                        key: Keys.D2
//                    ),
//                },
//                supply: new()
//                {
//                    [1] = 10,
//                },
//                demand: new(),
//                prodTime: TimeSpan.FromSeconds(value: 5),
//                reqWattsPerSec: 10,
//                reqSkill: 10
//            );

//            Factory.Params factory2_lvl1 = new
//            (
//                name: nameof(factory2_lvl1),
//                upgrades: new()
//                {
//                    new
//                    (
//                        parameters: factory2_lvl2,
//                        duration: TimeSpan.FromSeconds(5),
//                        cost: new(),
//                        reqWattsPerSec: 10,
//                        key: Keys.D3
//                    ),
//                },
//                supply: new()
//                {
//                    [2] = 10,
//                },
//                demand: new()
//                {
//                    [0] = 5,
//                    [1] = 5,
//                },
//                prodTime: TimeSpan.FromSeconds(value: 5),
//                reqWattsPerSec: 10,
//                reqSkill: 10
//            );

//            PowerPlant.Params power_plant_lvl1 = new
//            (
//                name: nameof(power_plant_lvl1),
//                upgrades: new()
//                {
//                    new
//                    (
//                        parameters: power_plant_lvl2,
//                        duration: TimeSpan.FromSeconds(value: 5),
//                        cost: new()
//                        {
//                            [0] = 20,
//                            [1] = 20,
//                            [2] = 20,
//                        },
//                        reqWattsPerSec: 500,
//                        key: Keys.D4
//                    ),
//                },
//                prodWattsPerSec: 1000
//            );

//            emptyParams = new Params
//            (
//                name: "empty",
//                industryType: IndustryType.Research,
//                upgrades: new()
//                {
//                    new
//                    (
//                        parameters: factory0_lvl1,
//                        duration: TimeSpan.FromSeconds(5),
//                        cost: new(),
//                        reqWattsPerSec: 10,
//                        key: Keys.D1
//                    ),
//                    new
//                    (
//                        parameters: factory1_lvl1,
//                        duration: TimeSpan.FromSeconds(5),
//                        cost: new(),
//                        reqWattsPerSec: 10,
//                        key: Keys.D2
//                    ),
//                    new
//                    (
//                        parameters: factory2_lvl1,
//                        duration: TimeSpan.FromSeconds(5),
//                        cost: new(),
//                        reqWattsPerSec: 10,
//                        key: Keys.D3
//                    ),
//                    new
//                    (
//                        parameters: power_plant_lvl1,
//                        duration: TimeSpan.FromSeconds(10),
//                        cost: new()
//                        {
//                            [0] = 5,
//                            [1] = 5,
//                        },
//                        reqWattsPerSec: 10,
//                        key: Keys.D4
//                    ),
//                }
//            );
//        }

//        protected readonly NodeState state;
//        protected virtual bool IsProducing
//            => false;

//        protected bool CanStartProduction { get; private set; }
//        private readonly Params parameters;
//        private readonly KeyButton togglePauseButton;
//        private Upgrade upgrade;
//        private TimeSpan? upgradeEndTime;
//        private UpgrStage CurUpgrStage
//        {
//            get
//            {
//                if (upgrade is null)
//                    return UpgrStage.None;
//                if (upgradeEndTime is null)
//                    return UpgrStage.Queued;
//                return UpgrStage.Upgrading;
//            }
//        }

//        public Industry(Params parameters, NodeState state)
//        {
//            this.parameters = parameters;
//            this.state = state;
//            CanStartProduction = true;
//            upgrade = null;
//            upgradeEndTime = null;
//            togglePauseButton = new
//            (
//                key: Keys.P,
//                action: () =>
//                {
//                    if (CurUpgrStage is not UpgrStage.Upgrading)
//                        CanStartProduction = !CanStartProduction;
//                }
//            );
//        }

//        public virtual ULongArray TargetStoredResAmounts()
//        {
//            if (CurUpgrStage is UpgrStage.Queued)
//                return upgrade.cost.ToULongArray();
//            return new();
//        }

//        public virtual ulong ReqWattsPerSec()
//            => CurUpgrStage switch
//            {
//                UpgrStage.Upgrading => upgrade.reqWattsPerSec,
//                _ => 0
//            };

//        public virtual ulong ProdWattsPerSec()
//            => 0;

//        public void ActiveUpdate()
//        {
//            togglePauseButton.Update();
//            if (upgrade is not null)
//                return;
//            foreach (var upgrade in parameters.upgrades)
//            {
//                upgrade.keyButton.Update();
//                if (upgrade.keyButton.Click)
//                {
//                    this.upgrade = upgrade;
//                    CanStartProduction = false;
//                }
//            }
//        }

//        public virtual Industry Update()
//        {
//            if (CurUpgrStage is UpgrStage.Queued && !IsProducing && state.storedRes >= upgrade.cost)
//            {
//                upgradeEndTime = C.TotalGameTime + upgrade.duration;
//                state.storedRes -= upgrade.cost;
//                CanStartProduction = false;
//            }

//            if (CurUpgrStage is UpgrStage.Upgrading && upgradeEndTime.Value <= C.TotalGameTime)
//                return upgrade.parameters.MakeIndustry(state: state);
//            return this;
//        }

//        //public virtual void StartProduction()
//        //{
//        //    if (CurUpgrStage is UpgrStage.Queued && !IsProducing && state.storedRes >= upgrade.cost)
//        //    {
//        //        upgradeEndTime = C.TotalGameTime + upgrade.duration;
//        //        state.storedRes -= upgrade.cost;
//        //        CanStartProduction = false;
//        //    }
//        //}

//        //public virtual Industry FinishProduction()
//        //{
//        //    if (CurUpgrStage is UpgrStage.Upgrading && upgradeEndTime.Value <= C.TotalGameTime)
//        //        return upgrade.parameters.MakeIndustry(state: state);
//        //    return this;
//        //}

//        public virtual string GetText()
//            => parameters.name;

//        public void Draw()
//        {
//            SpriteFont font = C.Content.Load<SpriteFont>("font");
//            string text = CurUpgrStage switch
//            {
//                UpgrStage.None => GetText(),
//                UpgrStage.Queued => "waiting to upgrade",
//                UpgrStage.Upgrading => $"upgrading {C.DonePart(endTime: upgradeEndTime.Value, duration: upgrade.duration) * 100: 0.}%",
//                _ => throw new ArgumentException(),
//            };
//            C.SpriteBatch.DrawString
//            (
//                spriteFont: font,
//                text: text,
//                position: state.position,
//                color: Color.Black,
//                rotation: 0,
//                origin: Vector2.Zero,
//                scale: .15f,
//                effects: SpriteEffects.None,
//                layerDepth: 0
//            );
//        }
//    }
//}
