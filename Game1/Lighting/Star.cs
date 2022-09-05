﻿using Game1.Shapes;
using Game1.UI;
using Microsoft.Toolkit.HighPerformance;
using static Game1.WorldManager;

namespace Game1.Lighting
{
    [Serializable]
    public sealed class Star : WorldUIElement, ILightSource
    {
        [Serializable]
        private readonly record struct ShapeParams(StarState State) : Disk.IParams
        {
            public MyVector2 Center
                => State.position;

            public UDouble Radius
                => State.radius;
        }

        private readonly StarState state;
        private readonly LightPolygon polygon;
        private readonly TextBox popupTextBox;

        public Star(StarState state, Color color)
            : base
            (
                shape: new Disk(parameters: new ShapeParams(State: state)),
                activeColor: Color.AntiqueWhite,
                inactiveColor: color,
                popupHorizPos: HorizPos.Right,
                popupVertPos: VertPos.Top
            )
        {
            this.state = state;
            polygon = new LightPolygon(strength: state.radius / CurWorldConfig.standardStarRadius, color: color);

            popupTextBox = new(backgroundColor: Color.White);
            SetPopup
            (
                HUDElement: popupTextBox,
                overlays: IOverlay.all
            );

            CurWorldManager.AddLightSource(lightSource: this);
        }

        // let N = lightCatchingObject.Count()
        // the complexity is O(N ^ 2) as each object has O(1) relevant angles
        // and each object checks intersection with all the rays
        // could maybe get the time down to O(N log N) by using modified interval tree
        void ILightSource.GiveWattsToObjects(List<ILightCatchingObject> lightCatchingObjects)
        {
            GetAnglesAndBlockedAngleArcs
            (
                angles: out List<double> angles,
                blockedAngleArcs: out List<(bool start, AngleArc angleArc)> blockedAngleArcs
            );

            PrepareAngles(ref angles);

            blockedAngleArcs.Sort
            (
                comparison: (angleArc1, angleArc2) => angleArc1.angleArc.GetAngle(start: angleArc1.start).CompareTo(angleArc2.angleArc.GetAngle(start: angleArc2.start))
            );

            CalculateLightPolygonAndRayCatchingObjects
            (
                vertices: out List<MyVector2> vertices,
                rayCatchingObjects: out List<ILightCatchingObject?> rayCatchingObjects
            );

            Debug.Assert(rayCatchingObjects.Count == angles.Count && vertices.Count == angles.Count);

            polygon.Update(center: state.position, vertices: vertices);

            DistributeStarPower(usedArc: out UDouble usedArc);

            popupTextBox.Text = $"generates {state.prodWatts} power\n{usedArc / (2 * MyMathHelper.pi) * 100:0.}% of it hits planet";

            return;

            void GetAnglesAndBlockedAngleArcs(out List<double> angles, out List<(bool start, AngleArc angleArc)> blockedAngleArcs)
            {
                angles = new();
                blockedAngleArcs = new();
                foreach (var lightCatchingObject in lightCatchingObjects)
                {
                    var blockedAngleArc = lightCatchingObject.BlockedAngleArc(lightPos: state.position);

                    angles.Add(blockedAngleArc.startAngle);
                    angles.Add(blockedAngleArc.endAngle);

                    if (blockedAngleArc.startAngle <= blockedAngleArc.endAngle)
                        addProperAngleArc(blockedAngleArcs: blockedAngleArcs, angleArc: blockedAngleArc);
                    else
                    {
                        addProperAngleArc
                        (
                            blockedAngleArcs: blockedAngleArcs,
                            angleArc: new
                            (
                                startAngle: blockedAngleArc.startAngle - 2 * MyMathHelper.pi,
                                endAngle: blockedAngleArc.endAngle,
                                radius: blockedAngleArc.radius,
                                lightCatchingObject: blockedAngleArc.lightCatchingObject
                            )
                        );
                        addProperAngleArc
                        (
                            blockedAngleArcs: blockedAngleArcs,
                            angleArc: new
                            (
                                startAngle: blockedAngleArc.startAngle,
                                endAngle: blockedAngleArc.endAngle + 2 * MyMathHelper.pi,
                                radius: blockedAngleArc.radius,
                                lightCatchingObject: blockedAngleArc.lightCatchingObject
                            )
                        );
                    }
                }
                void addProperAngleArc(List<(bool start, AngleArc angleArc)> blockedAngleArcs, AngleArc angleArc)
                {
                    blockedAngleArcs.Add((start: true, angleArc));
                    blockedAngleArcs.Add((start: false, angleArc));
                }
            }

            void PrepareAngles(ref List<double> angles)
            {
                // TODO: move to constants file
                const double small = .0001;
                int oldAngleCount = angles.Count;
                List<double> newAngles = new(2 * angles.Count);

                foreach (var angle in angles)
                {
                    newAngles.Add(angle - small);
                    newAngles.Add(angle + small);
                }

                for (int i = 0; i < 4; i++)
                    newAngles.Add(i * 2 * MyMathHelper.pi / 4);

                for (int i = 0; i < newAngles.Count; i++)
                    newAngles[i] = MyMathHelper.WrapAngle(angle: newAngles[i]);

                newAngles.Sort();
                angles = newAngles;
            }

            void CalculateLightPolygonAndRayCatchingObjects(out List<MyVector2> vertices, out List<ILightCatchingObject?> rayCatchingObjects)
            {
                vertices = new();
                rayCatchingObjects = new();
                // TODO: consider moving this to constants class
                UDouble maxDist = 2000;

                SortedSet<AngleArc> curAngleArcs = new();
                int angleInd = 0, angleArcInd = 0;
                while (angleInd < angles.Count)
                {
                    double curAngle = angles[angleInd];
                    while (angleArcInd < blockedAngleArcs.Count)
                    {
                        var (curStart, curAngleArc) = blockedAngleArcs[angleArcInd];
                        if (curAngleArc.GetAngle(start: curStart) >= curAngle)
                            break;
                        if (curStart)
                            curAngleArcs.Add(curAngleArc);
                        else
                            curAngleArcs.Remove(curAngleArc);
                        angleArcInd++;
                    }

                    MyVector2 rayDir = MyMathHelper.Direction(rotation: curAngle);
                    rayCatchingObjects.Add(curAngleArcs.Count == 0 ? null : curAngleArcs.Min.lightCatchingObject);
                    double minDist = rayCatchingObjects[^1] switch
                    {
                        null => maxDist,
                        // adding 1 looks better, even though it's not needed mathematically
                        // TODO: move the constant 1 to the constants fileooooooooooooooooooooooooooo
                        ILightCatchingObject lightCatchingObject => 1 + lightCatchingObject.CloserInterPoint(lightPos: state.position, lightDir: rayDir)
                    };
                    vertices.Add(state.position + minDist * rayDir);

                    angleInd++;
                }
            }

            void DistributeStarPower(out UDouble usedArc)
            {
                Dictionary<ILightCatchingObject, UDouble> arcsForObjects = lightCatchingObjects.ToDictionary
                (
                    keySelector: lightCatchingObject => lightCatchingObject,
                    elementSelector: lightCatchingObject => (UDouble)0
                );
                usedArc = 0;
                for (int i = 0; i < rayCatchingObjects.Count; i++)
                {
                    UDouble curArc = MyMathHelper.Abs(MyMathHelper.WrapAngle(angles[i] - angles[(i + 1) % angles.Count]));
                    UseArc(rayCatchingObject: rayCatchingObjects[i], usedArc: ref usedArc);
                    UseArc(rayCatchingObject: rayCatchingObjects[(i + 1) % rayCatchingObjects.Count], usedArc: ref usedArc);
                    void UseArc(ILightCatchingObject? rayCatchingObject, ref UDouble usedArc)
                    {
                        if (rayCatchingObject is not null)
                        {
                            arcsForObjects[rayCatchingObject] += curArc / 2;
                            usedArc += curArc / 2;
                        }
                    }
                }

                foreach (var lightCatchingObject in lightCatchingObjects)
                {
                    Propor powerPropor = Propor.Create(part: arcsForObjects[lightCatchingObject], whole: 2 * MyMathHelper.pi)!.Value;
                    lightCatchingObject.SetWatts
                    (
                        starPos: state.starID,
                        watts: powerPropor * state.prodWatts,
                        powerPropor: powerPropor
                    );
                }
            }
        }

        void ILightSource.Draw(Matrix worldToScreenTransform, BasicEffect basicEffect, int actualScreenWidth, int actualScreenHeight)
            => polygon.Draw
            (
                worldToScreenTransform: worldToScreenTransform,
                basicEffect: basicEffect,
                actualScreenWidth: actualScreenWidth,
                actualScreenHeight: actualScreenHeight
            );
    }
}