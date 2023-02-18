using Game1.Inhabitants;
using static Game1.WorldManager;

namespace Game1
{
    [Serializable]
    public sealed class TimedPacketQueue : IWithRealPeopleStats
    {
        public int Count
            => timedQueue.Count;
        public RealPeopleStats Stats { get; private set; }
        public NumPeople NumPeople { get; private set; }
        public ResAmounts TotalResAmounts { get; private set; }
        public Mass Mass { get; private set; }

        private readonly ThermalBody thermalBody;
        private readonly TimedQueue<(ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople)> timedQueue;

        public TimedPacketQueue(ThermalBody thermalBody)
        {
            this.thermalBody = thermalBody;
            TotalResAmounts = ResAmounts.Empty;
            Mass = Mass.zero;
            NumPeople = NumPeople.zero;
            timedQueue = new();
        }

        public void Update(TimeSpan duration, Propor workingPropor)
            => timedQueue.Update(duration: duration, workingPropor: workingPropor);

        /// <param name="personalUpdate"> if null, will use default update</param>
        public void UpdatePeople(RealPerson.UpdateLocationParams updateLocationParams, UpdatePersonSkillsParams? personalUpdate)
        {
            Stats = RealPeopleStats.empty;
            foreach (var (_, realPeople) in timedQueue)
            {
                realPeople.Update(updateLocationParams: updateLocationParams, updatePersonSkillsParams: personalUpdate);
                Stats = Stats.CombineWith(other: realPeople.Stats);
            }
        }

        public void Enqueue(ResAmountsPacketsByDestin resAmountsPackets, RealPeople realPeople)
        {
            resAmountsPackets = ResAmountsPacketsByDestin.CreateFromSource(sourcePackets: resAmountsPackets, thermalBody: thermalBody);
            realPeople = RealPeople.CreateFromSource
            (
                realPeopleSource: realPeople,
                thermalBody: thermalBody,
                energyDistributor: CurWorldManager.EnergyDistributor
            );
            if (resAmountsPackets.Empty && realPeople.NumPeople.IsZero)
                return;
            timedQueue.Enqueue(element: (resAmountsPackets, realPeople));
            TotalResAmounts += resAmountsPackets.ResAmounts;
            Mass += resAmountsPackets.Mass + realPeople.Stats.totalMass;
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
            var doneResAmountsPackets = ResAmountsPacketsByDestin.CreateEmpty(thermalBody: thermalBody);
            var donePeople = RealPeople.CreateEmpty(thermalBody: thermalBody, energyDistributor: CurWorldManager.EnergyDistributor);
            foreach (var (resAmountsPackets, people) in timedQueue.DoneElements())
            {
                TotalResAmounts -= resAmountsPackets.ResAmounts;
                Mass -= resAmountsPackets.Mass + people.Stats.totalMass;
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
