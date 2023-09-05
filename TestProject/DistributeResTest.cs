using Game1;
using Game1.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace TestProject
{
    [TestClass]
    public class DistributeResTest
    {
        private static IEnumerable<object[]> DistributeResTestRandomInput
        {
            get
            {
                Random random = new(Seed: 125);

                return new[]
                {
                    new object[]
                    {
                        new EfficientReadOnlyDictionary<Algorithms.Vertex<int>, Algorithms.VertexInfo<int>>()
                        {
                            [new(ResOwner: 0, IsSource: true)] = new
                            (
                                directedNeighbours: new List<int>() { 2, 3, 4 },
                                amount: 10
                            ),
                            [new(ResOwner: 1, IsSource: true)] = new
                            (
                                directedNeighbours: new List<int>() { 2, 4 },
                                amount: 5
                            ),
                            [new(ResOwner: 2, IsSource: false)] = new
                            (
                                directedNeighbours: new List<int>() { 0, 1 },
                                amount: 20
                            ),
                            [new(ResOwner: 3, IsSource: false)] = new
                            (
                                directedNeighbours: new List<int>() { 0 },
                                amount: 30
                            ),
                            [new(ResOwner: 4, IsSource: false)] = new
                            (
                                directedNeighbours: new List<int>() { 0, 1 },
                                amount: 10
                            ),
                        },
                    },
                    new object[]
                    {
                        GenerateRandomGraph(vertCount: 100, edgeCount: 1000, maxAmountInVert: 100000)
                    },
                    new object[]
                    {
                        GenerateRandomGraph(vertCount: 100, edgeCount: 5000, maxAmountInVert: 30)
                    }
                };

                EfficientReadOnlyDictionary<Algorithms.Vertex<int>, Algorithms.VertexInfo<int>> GenerateRandomGraph(int vertCount, int edgeCount, int maxAmountInVert)
                {
                    List<(int source, int destin)> simpleGraph =
                       (from _ in Enumerable.Range(0, edgeCount)
                        select (source: random.Next(maxValue: vertCount), destin: random.Next(maxValue: vertCount))).ToList();
                    Dictionary<Algorithms.Vertex<int>, Algorithms.VertexInfo<int>> graph = new();
                    foreach (var (source, destin) in simpleGraph)
                    {
                        Algorithms.Vertex<int> sourceVert = new(ResOwner: source, IsSource: true);
                        if (!graph.ContainsKey(sourceVert))
                            graph.Add(key: sourceVert, value: new(directedNeighbours: new(), amount: 0));

                        Algorithms.Vertex<int> destinVert = new(ResOwner: destin, IsSource: false);
                        if (!graph.ContainsKey(destinVert))
                            graph.Add(key: destinVert, value: new(directedNeighbours: new(), amount: 0));
                        Debug.Assert(graph[sourceVert].directedNeighbours.Contains(destin) == graph[destinVert].directedNeighbours.Contains(source));
                        if (!graph[sourceVert].directedNeighbours.Contains(destin))
                        {
                            graph[sourceVert].directedNeighbours.Add(destin);
                            graph[destinVert].directedNeighbours.Add(source);
                        }
                    }
                    foreach (var vertexInfo in graph.Values)
                        vertexInfo.amount = (ulong)random.Next(maxValue: maxAmountInVert);
                    return new(dict: graph);
                }
            }
        }

        /// <summary>
        /// Suppose have route A -> B,
        /// A has amA amount of resource and nA neighbours,
        /// B needs amB amount of resource and nB neighbours.
        /// Then A -> B gets at least min(amA / nA, amB / nB) of resource.
        /// </summary>
        [DynamicData(nameof(DistributeResTestRandomInput))]
        [DataTestMethod]
        public void EachGetsMinOfEvenSplit(EfficientReadOnlyDictionary<Algorithms.Vertex<int>, Algorithms.VertexInfo<int>> graph)
        {
            List<((int source, int destin) route, ulong amount)> minAmounts =
               (from sourceVertAndInfo in graph
                where sourceVertAndInfo.Key.IsSource
                let sourceInfo = sourceVertAndInfo.Value
                from destin in sourceInfo.directedNeighbours
                let destinInfo = graph[new(ResOwner: destin, IsSource: false)]
                select
                (
                    route: (source: sourceVertAndInfo.Key.ResOwner, destin: destin),
                    amount: Math.Min
                    (
                        sourceInfo.amount / (ulong)sourceInfo.directedNeighbours.Count,
                        destinInfo.amount / (ulong)destinInfo.directedNeighbours.Count
                    )
                )).ToList();

            var distributionDict = Algorithms.DistributeRes(graph: graph).ToDictionary
            (
                keySelector: resPacket => (source: resPacket.Source, destin: resPacket.Destin),
                elementSelector: resPacket => resPacket.Amount
            );

            foreach (var (route, minAmount) in minAmounts)
                Assert.IsTrue(distributionDict[route] >= minAmount);
        }
    }
}
