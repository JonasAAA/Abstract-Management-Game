using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace Game1
{
    public class Node : IUIElement
    {
        public Vector2 Position
            => state.position;
        public readonly float radius;
        public IJob Job
            => industry;
        public IEnumerable<Person> UnemployedPeople
            => state.unemployedPeople;

        private readonly NodeState state;
        private readonly Image image;
        private readonly List<Link> links;
        private readonly MyArray<ProporSplitter> resToLinksSplitters;
        private Industry industry;
        private TravelPacket curWaitingTravelPacket;
        private readonly ReadOnlyCollection<KeyButton> constrKeyButtons;
        private string text;

        public Node(NodeState state, Image image, int startPersonCount = 0)
        {
            this.state = state;
            radius = image.Width * .5f;
            this.image = image;
            links = new();
            resToLinksSplitters = new();
            for (int i = 0; i < ConstArray.length; i++)
                resToLinksSplitters[i] = new ProporSplitter();
            industry = null;
            curWaitingTravelPacket = new();
            Keys[] constrKeys = new Keys[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
            constrKeyButtons = new
            (
                list: (from key in constrKeys select new KeyButton(key: key)).ToArray()
            );
            for (int i = 0; i < startPersonCount; i++)
                state.unemployedPeople.Add(Person.GenerateNew());

            text = "";
        }

        public void AddLink(Link link)
        {
            if (!link.Contains(this))
                throw new ArgumentException();
            foreach (var resToLinksSplitter in resToLinksSplitters)
            {
                resToLinksSplitter.InsertVar(index: links.Count);
                resToLinksSplitter.Proportions = new(Enumerable.Repeat(element: 1.0, count: (int)resToLinksSplitter.Count).ToList());
            }
            links.Add(link);
        }

        public void AddText(string text)
            => this.text += text;

        public bool Contains(Vector2 position)
            => Vector2.Distance(this.Position, position) <= radius;

        public void AddTravelPacket(TravelPacket travelPacket)
            => state.waitingTravelPacket.Add(travelPacket: travelPacket);

        public double ReqWattsPerSec()
            => (state.unemployedPeople.Count + state.waitingTravelPacket.NumPeople) * Person.reqWattsPerSec
            + industry switch
            {
                null => 0,
                not null => industry.ReqWattsPerSec()
            };

        public double ProdWattsPerSec()
            => industry switch
            {
                null => 0,
                not null => industry.ProdWattsPerSec()
            };

        public void ActiveUpdate()
        {
            if (constrKeyButtons.Count < Industry.constrBuildingParams.Count)
                throw new Exception();
            if (industry is null)
            {
                for (int i = 0; i < Industry.constrBuildingParams.Count; i++)
                {
                    constrKeyButtons[i].Update();
                    if (constrKeyButtons[i].Click)
                    {
                        industry = Industry.constrBuildingParams[i].MakeIndustry(state: state);
                        break;
                    }
                }
            }
            else
                industry.ActiveUpdate();

            foreach (var node in Graph.Nodes)
                node.AddText(text: $"personal distance {Graph.PersonDists[(this, node)]:0.##}\nresource distance {Graph.ResDists[(this, node)]:0.##}\n");
        }

        public void StartUpdate()
        {
            // employees with jobs should not want to travel anywhere
            Debug.Assert(state.employees.All(person => person.Destination is null));

            // employees with jobs already traveled here, so should not want to travel here again
            Debug.Assert(state.employees.All(person => person.Destination != this));

            if (industry is not null)
                industry = industry.Update();

            state.unemployedPeople.RemoveAll
            (
                match: person =>
                {
                    if (person.Destination is not null)
                    {
                        state.waitingTravelPacket.Add(person: person);
                        return true;
                    }
                    return false;
                }
            );

            Dictionary<Link, TravelPacket> splitTravelPackets = new
            (
                collection: from link in links select KeyValuePair.Create(link, new TravelPacket())
            );

            // store resources which need storing
            curWaitingTravelPacket.Add(resAmounts: state.storedRes);
            state.storedRes = new();

            state.storedRes = industry switch
            {
                null => new(),
                not null => curWaitingTravelPacket.ResAmounts.Min(industry.TargetStoredResAmounts())
            };
            curWaitingTravelPacket.Remove(resAmounts: state.storedRes);
            
            // split the remaining resources
            for (int j = 0; j < ConstArray.length; j++)
            {
                if (!resToLinksSplitters[j].CanSplit(amount: curWaitingTravelPacket.ResAmounts[j]))
                    throw new NotImplementedException();
                ulong[] split = resToLinksSplitters[j].Split(amount: curWaitingTravelPacket.ResAmounts[j]);
                Debug.Assert(split.Length == links.Count);
                for (int i = 0; i < links.Count; i++)
                    splitTravelPackets[links[i]].Add(resInd: j, resAmount: split[i]);
            }

            // take appropriate people and split the rest
            foreach (var person in curWaitingTravelPacket.People)
            {
                if (person.Destination is null)
                {
                    state.unemployedPeople.Add(person);
                    continue;
                }

                if (person.Destination == this)
                {
                    if (state.travelingEmployees.Contains(person))
                    {
                        person.StopTravelling();
                        state.travelingEmployees.Remove(person);
                        state.employees.Add(person);
                        continue;
                    }
                    state.unemployedPeople.Add(person);
                    throw new Exception("Why were they travelling here is particular then?");
                    continue;
                }

                Link firstLink = Graph.PersonFirstLinks[(this, person.Destination)];
                splitTravelPackets[firstLink].Add(person);
            }

            curWaitingTravelPacket = new();

            foreach (var (link, travelPacket) in splitTravelPackets)
                link.AddTravelPacket
                (
                    start: this,
                    travelPacket: travelPacket
                );
        }

        public void EndUpdate()
        {
            curWaitingTravelPacket.Add(travelPacket: state.waitingTravelPacket);
            state.waitingTravelPacket = new();
        }

        public void Draw(bool active)
        {
            //Draw amount of resources in storage
            //or write percentage of required res
            if (active)
                image.Color = Color.Yellow;
            else
                image.Color = Color.White;
            image.Draw(Position);

            if (industry is not null)
                text += industry.GetText();
            text += $"\nemployed {state.employees.Count}\nunemployed {state.unemployedPeople.Count}\ntravelling {state.waitingTravelPacket.NumPeople}";

            C.SpriteBatch.DrawString
            (
                spriteFont: C.Content.Load<SpriteFont>("font"),
                text: text,
                position: state.position,
                color: Color.Black,
                rotation: 0,
                origin: Vector2.Zero,
                scale: .15f,
                effects: SpriteEffects.None,
                layerDepth: 0
            );
            text = "";
        }
    }
}
