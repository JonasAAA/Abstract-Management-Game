//using System;

//namespace Game1
//{
//    // TODO:
//    // MinAcceptableQualification needs to decrease if job stays vacant
//    // and need to actually use MinAcceptableQualification
//    public class Job
//    {
//        public readonly Node node;
//        public readonly IndustryType industryType;
//        public readonly TimeSpan offerStartTime;

//        public Job(Node node, IndustryType industryType, TimeSpan offerStartTime)
//        {
//            this.node = node;
//            this.industryType = industryType;
//            this.offerStartTime = offerStartTime;
//        }

//        // must be between 0 and 1 or double.NegativeInfinity
//        public double OpenSpace()
//            => 1;

//        public void Hire(Person person)
//        {
//            // TODO:
//            // set IsFull appropriately and
//            // if already was filled, need to fire someone

//            throw new NotImplementedException();
//        }
//    }
//}
