using Microsoft.Xna.Framework;
using System;

namespace Game1
{
    public interface IPersonFacingActivityCenter
    {
        public Vector2 Position { get; }

        public ulong ElectrPriority { get; }

        /// <summary>
        /// can include some randomness, but repeated measurements should give the same score
        /// gives higher/lower score to the current place of the person depending on
        /// if person recently got queued
        /// </summary>
        public double PersonScoreOfThis(Person person);

        public bool IsPersonHere(Person person);

        public void TakePerson(Person person);

        public void UpdatePerson(Person person, TimeSpan elapsed);

        public void RemovePerson(Person person);
    }
}
