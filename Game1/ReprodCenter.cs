//using Game1.UI;
//using Microsoft.Xna.Framework;
//using System;

//namespace Game1
//{
//    public class ReprodCenter : Industry
//    {
//        public new class Params : Industry.Params
//        {
//            public readonly ulong maxCouples;
//            public readonly ConstULongArray resPerChild;
//            // reqWattsPerSec doesn't make sence as this industry can accomodate variuos number of couples.
//            public Params(string name, ulong reqWattsPerSecPerChild, )
//                : base
//                (
//                    industryType: IndustryType.Reproduction,
//                    name:,
//                    electrPriority:,
//                    reqSkill:,
//                    reqWattsPerSec:,
//                    explanation:
//                )
//            { }
//        }

//        public ReprodCenter(Params parameters, NodeState state)
//            : base
//            (
//                parameters: parameters,
//                state: state,
//                UIPanel: new UIRectVertPanel<IUIElement<NearRectangle>>(color: Color.White, childHorizPos: HorizPos.Left)
//            )
//        { }

//        public override ULongArray TargetStoredResAmounts()
//        {
//            throw new NotImplementedException();
//        }

//        protected override bool IsBusy()
//        {
//            throw new NotImplementedException();
//        }

//        protected override Industry Update(TimeSpan elapsed, double workingPropor)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
