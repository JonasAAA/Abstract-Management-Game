using Game1.Inhabitants;

namespace Game1
{
    [Serializable]
    public sealed class TimedPacketQueue
    {
        public int Count
            => timedQueue.Count;
        public ulong PeopleCount { get; private set; }
        public ResAmounts TotalResAmounts { get; private set; }
        public Mass Mass { get; private set; }
        public readonly TimeSpan duration;

        private readonly MassCounter locationMassCounter;
        private readonly TimedQueue<(ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople)> timedQueue;

        public TimedPacketQueue(TimeSpan duration, MassCounter locationMassCounter)
        {
            this.duration = duration;
            this.locationMassCounter = locationMassCounter;
            TotalResAmounts = ResAmounts.Empty;
            Mass = Mass.zero;
            PeopleCount = 0;
            timedQueue = new(duration: duration);
        }

        public void Update(Propor workingPropor)
            => timedQueue.Update(workingPropor: workingPropor);

        /// <param name="personalUpdate"> if null, will use default update</param>
        public void UpdatePeople(RealPerson.UpdateLocationParams updateLocationParams, Func<RealPerson, UpdatePersonSkillsParams?>? personalUpdate)
        {
            foreach (var (_, realPeople) in timedQueue)
                realPeople.Update(updateLocationParams: updateLocationParams, personalUpdateSkillsParams: personalUpdate);
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople)
        {
            resAmountsPackets = ResAmountsPacketsByDestin.CreateFromSource(sourcePackets: resAmountsPackets, locationMassCounter: locationMassCounter);
            realPeople = RealPeople.CreateFromSource(realPeopleSource: realPeople, locationMassCounter: locationMassCounter);
            if (resAmountsPackets.Empty && realPeople.Count is 0)
                return;
            timedQueue.Enqueue(element: (resAmountsPackets, realPeople));
            TotalResAmounts += resAmountsPackets.ResAmounts;
            Mass += resAmountsPackets.Mass + realPeople.Mass;
            PeopleCount += realPeople.Count;
        }

        public IEnumerable<(Propor complPropor, ResAmounts resAmounts, ulong peopleCount)> GetData()
        {
            foreach (var (complPropor, (resAmountsPackets, people)) in timedQueue.GetData())
                yield return
                (
                    complPropor: complPropor,
                    resAmounts: resAmountsPackets.ResAmounts,
                    peopleCount: people.Count
                );
        }

        public (ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople) DonePacketsAndPeople()
        {
            var doneResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(locationMassCounter: locationMassCounter);
            var donePeople = RealPeople.CreateEmpty(locationMassCounter: locationMassCounter);
            foreach (var (resAmountsPackets, people) in timedQueue.DoneElements())
            {
                TotalResAmounts -= resAmountsPackets.ResAmounts;
                Mass -= resAmountsPackets.Mass + people.Mass;
                PeopleCount -= people.Count;

                doneResAmountsPackets.TransferAllFrom(sourcePackets: resAmountsPackets);
                donePeople.TransferAllFrom(realPeopleSource: people);
            }
            return
            (
                resAmountsPackets: doneResAmountsPackets,
                realPeople: donePeople
            );
        }

        public Propor LastCompletionPropor()
            => timedQueue.LastCompletionPropor();
    }
}
