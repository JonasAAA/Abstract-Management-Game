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

            electrProducer.Deleted += () => electrProducers.Remove(electrProducer);
        }

        public static void AddElectrConsumer(IElectrConsumer electrConsumer)
        {
            if (!electrConsumers.Add(electrConsumer))
                throw new ArgumentException();

            electrConsumer.Deleted += () => electrConsumers.Remove(electrConsumer);
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

        public static string Summary()
            => $"required electricity: {totReqWattsPerSec:0.##}\nproduced electricity: {totProdWattsPerSec:0.##}\n";
    }
}
