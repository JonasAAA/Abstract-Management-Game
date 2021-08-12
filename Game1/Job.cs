using System;

namespace Game1
{
    // TODO:
    // MinAcceptableQualification needs to decrease if job stays vacant
    // and need to actually use MinAcceptableQualification
    public class Job
    {
        public readonly Node node;
        public readonly IndustryType industryType;
        public readonly TimeSpan offerStartTime;
        public bool IsFull { get; private set; }

        public Job(Node node, IndustryType industryType, TimeSpan offerStartTime)
        {
            this.node = node;
            this.industryType = industryType;
            this.offerStartTime = offerStartTime;
            IsFull = false;
        }

        public void Hire(Person person)
        {
            // TODO:
            // set IsFull appropriately and
            // if already was filled, need to fire someone

            throw new NotImplementedException();
        }
    }
}
