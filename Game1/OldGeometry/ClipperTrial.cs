//using ClipperLib;

//namespace Game1.OldGeometry
//{
//    using Polygon = List<IntPoint>;
//    using Polygons = List<List<IntPoint>>;

//    [Serializable]
//    public class ClipperTrial
//    {
//        public static void TryCommonPerimeter()
//        {
//            Polygon poly1 = new()
//            {
//                new(0, 0),
//                new(0, 100),
//                new(100, 0),
//            };
//            Polygon poly2 = new()
//            {
//                new(0, 0),
//                new(0, 100),
//                new(-100, 0),
//            };

//            Clipper clipper = new();
//            clipper.AddPath(poly1, PolyType.ptSubject, false);
//            clipper.AddPath(poly2, PolyType.ptClip, true);
//            PolyTree polyTree = new();
//            clipper.Execute(ClipType.ctIntersection, polyTree);
//        }

//        public static void F()
//        {
//            Polygons subj = new(2);
//            subj.Add(new Polygon(4));
//            subj[0].Add(new IntPoint(180, 200));
//            subj[0].Add(new IntPoint(260, 200));
//            subj[0].Add(new IntPoint(260, 150));
//            subj[0].Add(new IntPoint(180, 150));

//            subj.Add(new Polygon(3));
//            subj[1].Add(new IntPoint(215, 160));
//            subj[1].Add(new IntPoint(230, 190));
//            subj[1].Add(new IntPoint(200, 190));

//            Polygons clip = new(1);
//            clip.Add(new Polygon(4));
//            clip[0].Add(new IntPoint(190, 210));
//            clip[0].Add(new IntPoint(240, 210));
//            clip[0].Add(new IntPoint(240, 130));
//            clip[0].Add(new IntPoint(190, 130));

//            //DrawPolygons(subj, Color.FromArgb(0x16, 0, 0, 0xFF),
//            //  Color.FromArgb(0x60, 0, 0, 0xFF));
//            //DrawPolygons(clip, Color.FromArgb(0x20, 0xFF, 0xFF, 0),
//            //  Color.FromArgb(0x30, 0xFF, 0, 0));

//            Polygons solution = new();

//            Clipper c = new();
//            c.AddPaths(subj, PolyType.ptSubject, true);
//            c.AddPaths(clip, PolyType.ptClip, true);
//            c.Execute(ClipType.ctIntersection, solution,
//              PolyFillType.pftEvenOdd, PolyFillType.pftEvenOdd);
//        }
//    }
//}
