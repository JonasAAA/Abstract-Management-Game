using Game1.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static Game1.WorldManager;

namespace Game1.Industries
{
    [DataContract]
    public class ReprodIndustry : Industry
    {
        [DataContract]
        public new class Params : Industry.Params
        {
            [DataMember]
            public readonly double reqWattsPerChild;
            [DataMember]
            public readonly ulong maxCouples;
            [DataMember]
            public readonly ConstULongArray resPerChild;
            [DataMember]
            public readonly TimeSpan birthDuration;

            public Params(string name, ulong energyPriority, double reqSkill, ulong reqWattsPerChild, ulong maxCouples, ConstULongArray resPerChild, TimeSpan birthDuration)
                : base
                (
                    industryType: IndustryType.Reproduction,
                    name: name,
                    energyPriority: energyPriority,
                    reqSkill: reqSkill,
                    explanation: $"requires {reqSkill} skill\nneeds {reqWattsPerChild} W/s for a child\nsupports no more than {maxCouples} couples\nrequires {resPerChild} resources for a child\nchildbirth takes {birthDuration} s"
                )
            {
                this.reqWattsPerChild = reqWattsPerChild;
                this.maxCouples = maxCouples;
                this.resPerChild = resPerChild;
                this.birthDuration = birthDuration;
            }

            public override Industry MakeIndustry(NodeState state)
                => new ReprodIndustry(state: state, parameters: this);
        }

        [DataContract]
        private class ReprodCenter : ActivityCenter
        {
            [DataMember]
            public readonly Queue<Person> unpairedPeople;

            [DataMember]
            private readonly Params parameters;

            public ReprodCenter(ulong energyPriority, NodeState state, Params parameters)
                : base(activityType: ActivityType.Reproduction, energyPriority: energyPriority, state: state)
            {
                this.parameters = parameters;
                unpairedPeople = new();
            }
            
            public override bool IsFull()
                => (ulong)allPeople.Count >= 2 * parameters.maxCouples;

            public override bool IsPersonSuitable(Person person)
                // could disalow far travel
                => true;

            public override double PersonScoreOfThis(Person person)
                => .9 * Math.Tanh((CurTime - person.LastActivityTimes[ActivityType]).TotalSeconds / 100)
                + .1 * DistanceToHere(person: person);

            public override void TakePerson(Person person)
            {
                base.TakePerson(person);
                unpairedPeople.Enqueue(person);
            }

            public override void UpdatePerson(Person person)
                => IActivityCenter.UpdatePersonDefault(person: person);

            public override bool CanPersonLeave(Person person)
            {
                throw new NotImplementedException();
            }

            public string GetText()
                => $"{unpairedPeople.Count} waiting people\n{allPeople.Count - peopleHere.Count} people travelling here\n";
        }

        [DataMember]
        private readonly Params parameters;
        [DataMember]
        private readonly ReprodCenter reprodCenter;
        [DataMember]
        private readonly TimedQueue<(Person, Person)> birthQueue;

        public ReprodIndustry(NodeState state, Params parameters)
            : base(state: state, parameters: parameters)
        {
            this.parameters = parameters;
            reprodCenter = new
            (
                energyPriority: parameters.energyPriority,
                state: state,
                parameters: parameters
            );

            birthQueue = new(duration: parameters.birthDuration);
        }

        public override ULongArray TargetStoredResAmounts()
            => parameters.maxCouples * parameters.resPerChild * state.maxBatchDemResStored;

        protected override bool IsBusy()
            => birthQueue.Count > 0;

        protected override Industry Update(double workingPropor)
        {
            birthQueue.Update(workingPropor: workingPropor);

            foreach (var (person1, person2) in birthQueue.DoneElements())
            {
                var newPerson = Person.GenerateChild(nodePos: state.position, person1: person1, person2: person2);
                state.waitingPeople.Add(newPerson);

                reprodCenter.RemovePerson(person: person1);
                reprodCenter.RemovePerson(person: person2);
            }

            while (reprodCenter.unpairedPeople.Count >= 2 && state.storedRes >= parameters.resPerChild)
            {
                Person person1 = reprodCenter.unpairedPeople.Dequeue(),
                    person2 = reprodCenter.unpairedPeople.Dequeue();
                birthQueue.Enqueue((person1, person2));
                state.storedRes -= parameters.resPerChild;
            }

            return this;
        }

        protected override void Delete()
        {
            base.Delete();
            reprodCenter.Delete();
            // need to disalow straight-up deletion.
            // first, births should finish, then people should evacuate, then can delete
            throw new NotImplementedException();
        }

        public override double ReqWatts()
            => birthQueue.Count * parameters.reqWattsPerChild * CurSkillPropor;

        public override string GetText()
        {
            string text = base.GetText() + $"{parameters.name}\n";
            if (CurOverlay is Overlay.People)
            {
                text += $"{birthQueue.Count} children are being born\n";
                text += reprodCenter.GetText();
            }
            return text;
        }
    }
}
