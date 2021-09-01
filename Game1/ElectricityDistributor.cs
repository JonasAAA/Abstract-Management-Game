using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public static class ElectricityDistributor
    {
        private static readonly double ambientWattsPerSec;
        private static readonly HashSet<IElectrProducer> electrProducers;
        private static readonly HashSet<IElectrConsumer> electrConsumers;
        private static double totReqWattsPerSec, totProdWattsPerSec;

        static ElectricityDistributor()
        {
            ambientWattsPerSec = 100;
            electrProducers = new();
            electrConsumers = new();
            totReqWattsPerSec = 0;
            totProdWattsPerSec = ambientWattsPerSec;
        }

        public static void AddElectrProducer(IElectrProducer electrProducer)
        {
            if (!electrProducers.Add(electrProducer))
                throw new ArgumentException();
        }

        public static void AddElectrConsumer(IElectrConsumer electrConsumer)
        {
            if (!electrConsumers.Add(electrConsumer))
                throw new ArgumentException();
        }

        public static void RemoveElectrConsumer(IElectrConsumer electrConsumer)
        {
            if (!electrConsumers.Remove(electrConsumer))
                throw new ArgumentException();
        }

        public static void DistributeElectr()
        {
            totProdWattsPerSec = ambientWattsPerSec + electrProducers.Sum(electrProducer => electrProducer.ProdWattsPerSec());
            totReqWattsPerSec = 0;

            double remainWattsPerSec = totProdWattsPerSec;
            SortedDictionary<ulong, HashSet<IElectrConsumer>> electrConsumersSortedDict = new();
            foreach (var electrConsumer in electrConsumers)
            {
                ulong priority = electrConsumer.ElectrPriority;
                if (!electrConsumersSortedDict.ContainsKey(key: priority))
                    electrConsumersSortedDict[priority] = new();
                electrConsumersSortedDict[priority].Add(electrConsumer);
            }
            foreach (var (priority, samePriorElectrConsumers) in electrConsumersSortedDict)
            {
                double reqWattsPerSec = samePriorElectrConsumers.Sum(electrConsumer => electrConsumer.ReqWattsPerSec()),
                    electrPropor;

                totReqWattsPerSec += reqWattsPerSec;

                if (remainWattsPerSec < reqWattsPerSec)
                {
                    electrPropor = remainWattsPerSec / reqWattsPerSec;
                    remainWattsPerSec = 0;
                    if (priority is 0)
                        throw new Exception();
                }
                else
                {
                    electrPropor = 1;
                    remainWattsPerSec -= reqWattsPerSec;
                }
                Debug.Assert(electrPropor >= 0);
                foreach (var electrConsumer in samePriorElectrConsumers)
                    electrConsumer.ConsumeElectr(electrPropor: electrPropor);
            }
        }

        public static void DrawHUD()
        {
            C.SpriteBatch.DrawString
            (
                spriteFont: C.Content.Load<SpriteFont>("font"),
                text: $"required: {totReqWattsPerSec:0.##}\nproduced: {totProdWattsPerSec:0.##}",
                position: new Vector2(10, 10),
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
