//using Game1.Industries;
//using Microsoft.Xna.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace Game1
//{
//    public static class PeopleReproduction
//    {
//        private class PersonInfo
//        {
//            public readonly Person person;
//            public readonly Vector2 position;
//            /// <summary>
//            /// the bigger it is, the sooner person is considered
//            /// </summary>
//            public readonly double priority;
//            public bool isPaired;

//            public PersonInfo(Person person, Vector2 position)
//            {
//                this.person = person;
//                this.position = position;
//                priority = C.Random(min: 0, max: 1);
//                isPaired = false;
//            }
//        }

//        private static readonly HashSet<IReprodCenter> reprodCenters;

//        static PeopleReproduction()
//            => reprodCenters = new();

//        public static void AddReprodCenter(IReprodCenter reprodCenter)
//        {
//            if (!reprodCenters.Add(reprodCenter))
//                throw new ArgumentException();

//            reprodCenter.Deleted += () => reprodCenters.Remove(reprodCenter);
//        }

//        public static void Match()
//        {
//            Dictionary<Vector2, List<PersonInfo>> childWantingPeopleByPos = new();
//            foreach (var node in Graph.World.Nodes)
//            {
//                var personInfos =
//                    (from person in node.ChildWantingPeople()
//                     select new PersonInfo(person: person, position: node.Position))
//                     .OrderBy(personInfo => personInfo.priority).ToList();
//                if (personInfos.Count > 0)
//                    childWantingPeopleByPos[node.Position] = personInfos;
//            }
//            PersonInfo[] childWantingPeople =
//                (from personInfos in childWantingPeopleByPos.Values
//                 from personInfo in personInfos
//                 select personInfo).OrderBy(personInfo => personInfo.priority).ToArray();

//            void SetIsPairedTrue(PersonInfo personInfo)
//            {
//                if (personInfo.isPaired || childWantingPeopleByPos[personInfo.position][^1] != personInfo)
//                    throw new ArgumentException();
//                personInfo.isPaired = true;
//                childWantingPeopleByPos[personInfo.position].RemoveAt(childWantingPeopleByPos[personInfo.position].Count - 1);
//            }

//            foreach (var personInfo in childWantingPeople.Reverse())
//            {
//                if (personInfo.isPaired)
//                    continue;
//                SetIsPairedTrue(personInfo: personInfo);
//                var otherPersonInfo =
//                    (from pos in Graph.World.PosToNode[personInfo.position].NeighbPositions().Append(personInfo.position)
//                     where childWantingPeopleByPos.ContainsKey(pos)
//                     select childWantingPeopleByPos[pos][^1])
//                     .ArgMaxOrDefault(personInfo => personInfo.priority);
//                if (otherPersonInfo is null)
//                    continue;
//                SetIsPairedTrue(personInfo: otherPersonInfo);
//                SendCouple(personInfo1: personInfo, personInfo2: otherPersonInfo);
//            }
//        }

//        private static void SendCouple(PersonInfo personInfo1, PersonInfo personInfo2)
//        {
//            var closestReprodCenter = 
//                (from reprodCenter in reprodCenters
//                 where !reprodCenter.IsFull()
//                 select reprodCenter)
//                 .ArgMinOrDefault
//                 (
//                     selector: reprodCenter => Math.Max
//                     (
//                         val1: Graph.World.PersonDists[(personInfo1.position, reprodCenter.Position)],
//                         val2: Graph.World.PersonDists[(personInfo2.position, reprodCenter.Position)]
//                     )
//                 );

//            if (closestReprodCenter is null)
//                // could stop all the remaining parings at this point to be more efficient
//                return;

//            closestReprodCenter.AddCouple(person1: personInfo1.person, personInfo2.person);
//            personInfo1.person.GoAndKeepJob(destination: closestReprodCenter.Position);
//            personInfo2.person.GoAndKeepJob(destination: closestReprodCenter.Position);
//        }
//    }
//}
