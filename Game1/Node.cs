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
        public Position Position
            => state.position;
        public readonly float radius;
        public IEmployer Employer
            => industry;
        public IEnumerable<Person> UnemployedPeople
            => state.unemployedPeople;

        private readonly NodeState state;
        private readonly Image image;
        private readonly List<Link> links;
        private readonly MyArray<ProporSplitter> resToLinksSplitters;
        private Industry industry;
        private readonly ReadOnlyCollection<KeyButton> constrKeyButtons;
        private readonly MyArray<Position> resDestins;
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
            Keys[] constrKeys = new Keys[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9 };
            constrKeyButtons = new
            (
                list: (from key in constrKeys select new KeyButton(key: key)).ToArray()
            );
            for (int i = 0; i < startPersonCount; i++)
                state.unemployedPeople.Add(Person.GenerateNew());
            resDestins = new(value: null);

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
            => Vector2.Distance(Position.ToVector2(), position) <= radius;

        public void Add(ResAmountsPacketsByDestin resAmountsPackets)
            => state.waitingResAmountsPackets.Add(resAmountsPackets: resAmountsPackets);

        public void Add(IEnumerable<Person> people)
            => state.waitingPeople.AddRange(people);

        public void Add(Person person)
            => state.waitingPeople.Add(person);

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
                node.AddText(text: $"personal distance {Graph.PersonDists[(Position, node.Position)]:0.##}\nresource distance {Graph.ResDists[(Position, node.Position)]:0.##}\n");
        }

        public void Update(TimeSpan elapsed)
        {
            foreach (var person in state.unemployedPeople.Concat(state.waitingPeople))
                person.UpdateNotWorking(elapsed: elapsed);

            if (industry is not null)
                industry = industry.Update(elapsed: elapsed);

            // deal with resources
            ULongArray undecidedResAmounts = state.waitingResAmountsPackets.ReturnAndRemove(destination: Position) + state.storedRes;
            state.storedRes = new();

            state.storedRes = industry switch
            {
                null => new(),
                not null => undecidedResAmounts.Min(industry.TargetStoredResAmounts())
            };
            undecidedResAmounts -= state.storedRes;

            for (int resInd = 0; resInd < Resource.Count; resInd++)
            {
                Debug.Assert(resDestins[resInd] != Position);

                if (undecidedResAmounts[resInd] is 0)
                    continue;

                Position destination = resDestins[resInd];
                if (destination is null)
                    state.storedRes[resInd] += undecidedResAmounts[resInd];
                else
                    state.waitingResAmountsPackets.Add
                    (
                        destination: destination,
                        resInd: resInd,
                        resAmount: undecidedResAmounts[resInd]
                    );
                undecidedResAmounts[resInd] = 0;
            }

            foreach (var resAmountsPacket in state.waitingResAmountsPackets.DeconstructAndClear())
            {
                Position destination = resAmountsPacket.destination;
                Debug.Assert(destination is not null && destination != Position);

                Graph.ResFirstLinks[(Position, destination)].Add(start: this, resAmountsPacket: resAmountsPacket);
            }

            state.waitingResAmountsPackets = new();

            // deal with people
            state.unemployedPeople.RemoveAll
            (
                match: person =>
                {
                    if (person.Destination is not null)
                    {
                        state.waitingPeople.Add(person);
                        return true;
                    }
                    return false;
                }
            );

            // take appropriate people and split the rest
            foreach (var person in state.waitingPeople)
            {
                if (person.Destination is null)
                {
                    state.unemployedPeople.Add(person);
                    continue;
                }

                if (person.Destination == Position)
                {
                    if (industry.IfEmploys(person: person))
                    {
                        person.StopTravelling();
                        industry.Take(person: person);
                        continue;
                    }
                    state.unemployedPeople.Add(person);
                    throw new Exception("Why were they travelling here is particular then?");
                    continue;
                }

                Graph.PersonFirstLinks[(Position, person.Destination)].Add(start: this, person: person);
            }
            state.waitingPeople = new();
        }

        public void Draw(bool active)
        {
            //Draw amount of resources in storage
            //or write percentage of required res
            if (active)
                image.Color = Color.Yellow;
            else
                image.Color = Color.White;
            image.Draw(position: Position.ToVector2());

            if (industry is not null)
                text += industry.GetText();
            text += $"unemployed {state.unemployedPeople.Count}\nstored total res weight {state.storedRes.TotalWeight()}";

            C.SpriteBatch.DrawString
            (
                spriteFont: C.Content.Load<SpriteFont>("font"),
                text: text,
                position: state.position.ToVector2(),
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
