using Game1.Inhabitants;

namespace Game1
{
    [Serializable]
    public sealed class TimedPacketQueue : IHasMass
    {
        public int Count
            => timedQueue.Count;
        public ulong PeopleCount { get; private set; }
        public ResAmounts TotalResAmounts { get; private set; }
        public ulong Mass { get; private set; }
        public readonly TimeSpan duration;

        private readonly TimedQueue<(ResAmountsPacketsByDestin resAmountsPackets, RealPeople people)> timedQueue;

        public TimedPacketQueue(TimeSpan duration)
        {
            TotalResAmounts = ResAmounts.Empty;
            Mass = 0;
            this.duration = duration;
            timedQueue = new(duration: duration);
        }

        public void Update(Propor workingPropor)
            => timedQueue.Update(workingPropor: workingPropor);

        /// <param name="personalUpdate"> if null, will use default update</param>
        public void UpdatePeople(RealPerson.UpdateParams updateParams, Action<RealPerson>? personalUpdate)
        {
            foreach (var (_, realPeople) in timedQueue)
                realPeople.Update(updateParams: updateParams, personalUpdate: personalUpdate);
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, RealPeople people)
        {
            if (resAmountsPackets.Empty && people.Count is 0)
                return;
            timedQueue.Enqueue(element: (resAmountsPackets, people));
            TotalResAmounts += resAmountsPackets.ResAmounts;
            Mass += resAmountsPackets.Mass + people.Mass;
            PeopleCount += people.Count;
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

        public (ResAmountsPacketsByDestin resAmountsPackets, RealPeople people) DonePacketsAndPeople()
        {
            ResAmountsPacketsByDestin doneResAmountsPackets = new();
            RealPeople donePeople = new();
            foreach (var (resAmountsPackets, people) in timedQueue.DoneElements())
            {
                TotalResAmounts -= resAmountsPackets.ResAmounts;
                Mass -= resAmountsPackets.Mass + people.Mass;
                PeopleCount -= people.Count;

                var resAmountsPacketsCopy = resAmountsPackets;
                doneResAmountsPackets.TransferAllFrom(sourcePackets: ref resAmountsPacketsCopy);
                donePeople.TransferAllFrom(peopleSource: people);
            }
            return
            (
                resAmountsPackets: doneResAmountsPackets,
                people: donePeople
            );
        }

        public Propor LastCompletionPropor()
            => timedQueue.LastCompletionPropor();
    }
}
