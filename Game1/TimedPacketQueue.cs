using Game1.Inhabitants;

namespace Game1
{
    [Serializable]
    public sealed class TimedPacketQueue
    {
        public int Count
            => timedQueue.Count;
        public NumPeople NumPeople { get; private set; }
        public ResAmounts TotalResAmounts { get; private set; }
        public Mass Mass { get; private set; }

        private readonly MassCounter locationMassCounter;
        private readonly PeopleCounter locationPeopleCounter;
        private readonly TimedQueue<(ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople)> timedQueue;

        public TimedPacketQueue(MassCounter locationMassCounter, PeopleCounter locationPeopleCounter)
        {
            this.locationMassCounter = locationMassCounter;
            this.locationPeopleCounter = locationPeopleCounter;
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
            foreach (var (_, realPeople) in timedQueue)
                realPeople.Update(updateLocationParams: updateLocationParams, personalUpdateSkillsParams: personalUpdate);
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople)
        {
            resAmountsPackets = ResAmountsPacketsByDestin.CreateFromSource(sourcePackets: resAmountsPackets, locationMassCounter: locationMassCounter);
            realPeople = RealPeople.CreateFromSource(realPeopleSource: realPeople, locationMassCounter: locationMassCounter, locationPeopleCounter: locationPeopleCounter);
            if (resAmountsPackets.Empty && realPeople.Count.IsZero)
                return;
            timedQueue.Enqueue(element: (resAmountsPackets, realPeople));
            TotalResAmounts += resAmountsPackets.ResAmounts;
            Mass += resAmountsPackets.Mass + realPeople.Mass;
            NumPeople += realPeople.Count;
        }

        public IEnumerable<(Propor complPropor, ResAmounts resAmounts, NumPeople numPeople)> GetData()
        {
            foreach (var (complPropor, (resAmountsPackets, people)) in timedQueue.GetData())
                yield return
                (
                    complPropor: complPropor,
                    resAmounts: resAmountsPackets.ResAmounts,
                    numPeople: people.Count
                );
        }

        public (ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople) DonePacketsAndPeople()
        {
            var doneResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(locationMassCounter: locationMassCounter);
            var donePeople = RealPeople.CreateEmpty(locationMassCounter: locationMassCounter, locationPeopleCounter: locationPeopleCounter);
            foreach (var (resAmountsPackets, people) in timedQueue.DoneElements())
            {
                TotalResAmounts -= resAmountsPackets.ResAmounts;
                Mass -= resAmountsPackets.Mass + people.Mass;
                NumPeople -= people.Count;

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
