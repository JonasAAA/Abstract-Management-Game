using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Game1
{
    public class NodeState
    {
        public readonly Vector2 position;
        public ULongArray storedRes;
        public readonly ulong maxBatchDemResStored;
        public readonly List<Person> employees, travelingEmployees, unemployedPeople;
        public TravelPacket waitingTravelPacket { get; set; }

        public NodeState(Vector2 position, ulong maxBatchDemResStored)
        {
            this.position = position;
            storedRes = new();
            if (maxBatchDemResStored is 0)
                throw new ArgumentOutOfRangeException();
            this.maxBatchDemResStored = maxBatchDemResStored;
            employees = new();
            travelingEmployees = new();
            unemployedPeople = new();
            waitingTravelPacket = new();
        }

        public void Fire(Person person)
        {
            if (employees.Remove(person))
                unemployedPeople.Add(person);
            else
                travelingEmployees.Remove(person);
            person.Fire();
        }

        public void FireAllMatching(Func<Person, bool> match)
        {
            employees.RemoveAll
            (
                person =>
                {
                    if (match(person))
                    {
                        person.Fire();
                        unemployedPeople.Add(person);
                        return true;
                    }
                    return false;
                }
            );
            travelingEmployees.RemoveAll
            (
                person =>
                {
                    if (match(person))
                    {
                        person.Fire();
                        return true;
                    }
                    return false;
                }
            );
        }

        public void FireAll()
        {
            foreach (var person in employees.Concat(travelingEmployees))
                person.Fire();
            unemployedPeople.AddRange(employees);
            employees.Clear();
            travelingEmployees.Clear();
        }
    }
}
