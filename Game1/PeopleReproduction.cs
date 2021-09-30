using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public static class PeopleReproduction
    {
        private class PersonInfo
        {
            public readonly Person person;
            public readonly Vector2 pos;
            /// <summary>
            /// the bigger it is, the sooner person is considered
            /// </summary>
            public readonly double priority;
            public bool isPaired;

            public PersonInfo(Person person, Vector2 pos)
            {
                this.person = person;
                this.pos = pos;
                priority = C.Random(min: 0, max: 1);
                isPaired = false;
            }
        }

        public static void Match()
        {
            Dictionary<Vector2, List<PersonInfo>> childWantingPeopleByPos = new();
            foreach (var node in Graph.World.Nodes)
            {
                var personInfos =
                    (from person in node.ChildWantingPeople()
                     select new PersonInfo(person: person, pos: node.Position))
                     .OrderBy(personInfo => personInfo.priority).ToList();
                if (personInfos.Count > 0)
                    childWantingPeopleByPos[node.Position] = personInfos;
            }
            PersonInfo[] childWantingPeople =
                (from personInfos in childWantingPeopleByPos.Values
                 from personInfo in personInfos
                 select personInfo).OrderBy(personInfo => personInfo.priority).ToArray();

            void SetIsPairedTrue(PersonInfo personInfo)
            {
                if (personInfo.isPaired || childWantingPeopleByPos[personInfo.pos][^1] != personInfo)
                    throw new ArgumentException();
                personInfo.isPaired = true;
                childWantingPeopleByPos[personInfo.pos].RemoveAt(childWantingPeopleByPos[personInfo.pos].Count - 1);
            }

            foreach (var personInfo in childWantingPeople.Reverse())
            {
                if (personInfo.isPaired)
                    continue;
                SetIsPairedTrue(personInfo: personInfo);
                var otherPersonInfo =
                    (from pos in Graph.World.PosToNode[personInfo.pos].NeighbPositions().Append(personInfo.pos)
                     where childWantingPeopleByPos.ContainsKey(pos)
                     select childWantingPeopleByPos[pos][^1])
                     .ArgMaxOrDefault(personInfo => personInfo.priority);
                if (otherPersonInfo is null)
                    continue;
                SetIsPairedTrue(personInfo: otherPersonInfo);
                // now send personInfo and otherPersonInfo to have a child
                throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }
    }
}
