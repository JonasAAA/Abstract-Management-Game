using Game1.Inhabitants;

namespace Game1
{
    [Serializable]
    public sealed class TimedPacketQueue : IWithRealPeopleStats
    {
        public int Count
            => timedQueue.Count;
        public RealPeopleStats RealPeopleStats { get; private set; }
        public NumPeople NumPeople { get; private set; }
        public ResAmounts TotalResAmounts { get; private set; }
        public Mass Mass { get; private set; }

        private readonly LocationCounters locationCounters;
        private readonly TimedQueue<(ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople)> timedQueue;

        public TimedPacketQueue(LocationCounters locationCounters)
        {
            this.locationCounters = locationCounters;
            TotalResAmounts = ResAmounts.Empty;
            Mass = Mass.zero;
            NumPeople = NumPeople.zero;
            timedQueue = new();
        }

        public void Update(TimeSpan duration, Propor workingPropor)
            => timedQueue.Update(duration: duration, workingPropor: workingPropor);

        /// <param name="personalUpdate"> if null, will use default update</param>
        public void UpdatePeople(RealPerson.UpdateLocationParams updateLocationParams, Func<RealPerson, UpdatePersonSkillsParams?>? personalUpdate)
        {
            RealPeopleStats = RealPeopleStats.empty;
            foreach (var (_, realPeople) in timedQueue)
            {
                realPeople.Update(updateLocationParams: updateLocationParams, personalUpdateSkillsParams: personalUpdate);
                RealPeopleStats = RealPeopleStats.CombineWith(other: realPeople.RealPeopleStats);
            }
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople)
        {
            resAmountsPackets = ResAmountsPacketsByDestin.CreateFromSource(sourcePackets: resAmountsPackets, locationCounters: locationCounters);
            realPeople = RealPeople.CreateFromSource(realPeopleSource: realPeople, locationCounters: locationCounters);
            if (resAmountsPackets.Empty && realPeople.NumPeople.isZero)
                return;
            timedQueue.Enqueue(element: (resAmountsPackets, realPeople));
            TotalResAmounts += resAmountsPackets.ResAmounts;
            Mass += resAmountsPackets.Mass + realPeople.RealPeopleStats.totalMass;
            NumPeople += realPeople.NumPeople;
        }

        public IEnumerable<(Propor complPropor, ResAmounts resAmounts, NumPeople numPeople)> GetData()
        {
            foreach (var (complPropor, (resAmountsPackets, people)) in timedQueue.GetData())
                yield return
                (
                    complPropor: complPropor,
                    resAmounts: resAmountsPackets.ResAmounts,
                    numPeople: people.NumPeople
                );
        }

        public (ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople) DonePacketsAndPeople()
        {
            var doneResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(locationCounters: locationCounters);
            var donePeople = RealPeople.CreateEmpty(locationCounters: locationCounters);
            foreach (var (resAmountsPackets, people) in timedQueue.DoneElements())
            {
                TotalResAmounts -= resAmountsPackets.ResAmounts;
                Mass -= resAmountsPackets.Mass + people.RealPeopleStats.totalMass;
                NumPeople -= people.NumPeople;

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
