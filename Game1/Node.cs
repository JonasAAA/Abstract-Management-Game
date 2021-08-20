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
        // resToLinksSplitters[i][j] - resource i to link j
        private readonly MyArray<ProporSplitter> resToLinksSplitters;
        private Industry industry;
        private ULongArray curWaitingRes;
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
            curWaitingRes = new();
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

        public void AddRes(ConstULongArray resAmounts)
            => state.waitingRes += resAmounts;

        public void AddPerson(Person person)
        {
            if (person.Destination == this)
            {
                if (state.travelingEmployees.Contains(person))
                {
                    state.travelingEmployees.Remove(person);
                    state.employees.Add(person);
                }
                else
                    state.unemployedPeople.Add(person);
            }
            else
                state.travellingPeople.Add(person);
        }

        public double ReqWattsPerSec()
            => (state.employees.Count + state.unemployedPeople.Count + state.travellingPeople.Count) * Person.reqWattsPerSec
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
                node.AddText(text: $"distance {Graph.ElectrDists[(this, node)]:0.##} \n");
        }

        public void StartUpdate()
        {
            if (industry is not null)
                industry = industry.Update();

            state.unemployedPeople.RemoveAll
            (
                match: person =>
                {
                    if (person.Destination is not null)
                    {
                        if (person.Destination == this)
                        {
                            if (state.travelingEmployees.Contains(person))
                            {
                                state.travelingEmployees.Remove(person);
                                state.employees.Add(person);
                                return true;
                            }
                            return false;
                        }
                        person.Destination.AddPerson(person);
                        return true;
                    }
                    return false;
                }
            );

            curWaitingRes += state.storedRes;
            state.storedRes = new();

            state.storedRes = industry switch
            {
                null => new(),
                not null => curWaitingRes.Min(industry.TargetStoredResAmounts())
            };
            curWaitingRes -= state.storedRes;

            // maybe I should just send one resource at a time rather then pack them to ULongArray
            ULongArray[] resSplitAmounts = new ULongArray[links.Count];
            for (int i = 0; i < resSplitAmounts.Length; i++)
                resSplitAmounts[i] = new();

            for (int j = 0; j < ConstArray.length; j++)
            {
                if (!resToLinksSplitters[j].CanSplit(amount: curWaitingRes[j]))
                    throw new NotImplementedException();
                ulong[] split = resToLinksSplitters[j].Split(amount: curWaitingRes[j]);
                Debug.Assert(split.Length == resSplitAmounts.Length);
                for (int i = 0; i < resSplitAmounts.Length; i++)
                    resSplitAmounts[i][j] = split[i];
            }

            for (int i = 0; i < links.Count; i++)
                links[i].AddRes(start: this, resAmounts: resSplitAmounts[i]);

            curWaitingRes = new();
        }

        public void EndUpdate()
        {
            curWaitingRes += state.waitingRes;
            state.waitingRes = new();
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

            //string text = "";
            if (industry is not null)
                text += industry.GetText();
            text += $"\nemployed {state.employees.Count}\nunemployed {state.unemployedPeople.Count}\ntravelling {state.travellingPeople.Count}";

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
