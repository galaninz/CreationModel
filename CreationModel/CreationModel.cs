using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreationModel
{
    [TransactionAttribute(TransactionMode.Manual)]
    public class CreationModel : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {

            Document doc = commandData.Application.ActiveUIDocument.Document;
            Application application = doc.Application;

            List<Level> listLevel = new List<Level>();
            List<XYZ> points = new List<XYZ>();

            //var res1 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(Wall))
            //    //.Cast<Wall>()
            //    .OfType<Wall>()
            //    .ToList();

            //var res2 = new FilteredElementCollector(doc)
            //    .OfClass(typeof(FamilyInstance))
            //    .OfCategory(BuiltInCategory.OST_Doors)
            //    //.Cast<Wall>()
            //    .OfType<FamilyInstance>()
            //    .Where(x=>x.Name.Equals("0915 x 2134 мм"))
            //    .ToList();

            //var res3 = new FilteredElementCollector(doc)
            //   .WhereElementIsNotElementType()
            //   .ToList();

            LevelUtils.GetLevels(doc);
            WallUtils.GetWallsPoints(doc);
            List<Wall> walls = new List<Wall>();
            CurveArray curveArray = new CurveArray();
            CurveArray footPrint = application.Create.NewCurveArray();

            ElementId id = doc.GetDefaultElementTypeId(ElementTypeGroup.RoofType);
            RoofType type = doc.GetElement(id) as RoofType;

            for (int i = 0; i < walls.Count; i++)
            {
                LocationCurve curve = walls[i].Location as LocationCurve;
                XYZ p1 = curve.Curve.GetEndPoint(0);
                XYZ p2 = curve.Curve.GetEndPoint(1);
                Line line = Line.CreateBound(p1 + points[i], p2 + points[i + 1]);
                footPrint.Append(line);
            }

            Transaction ts = new Transaction(doc, "Wall creation");
            ts.Start();

            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, listLevel[0].Id, false);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(listLevel[1].Id);
                walls.Add(wall);

            }
            ReferencePlane plane = doc.Create.NewReferencePlane(new XYZ(0, 0, 0), new XYZ(0, 0, 20), new XYZ(0, 20, 0), doc.ActiveView);
            doc.Create.NewExtrusionRoof(curveArray, plane, listLevel[1], type, 0, 40);

            ts.Commit();

            AddDoor(doc, listLevel[0], walls[0]);

            AddWindow(doc, listLevel[0], walls[1]);
            AddWindow(doc, listLevel[0], walls[2]);
            AddWindow(doc, listLevel[0], walls[3]);

            AddRoof(doc, listLevel[1], walls);

            return Result.Succeeded;
        }

        private void AddRoof(Document doc, Level level2, List<Wall> walls)
        {
            //RoofType roofType = new FilteredElementCollector(doc)
            //    .OfClass(typeof(RoofType))
            //    .OfType<RoofType>()
            //    .Where(x => x.Name.Equals("Default"))
            //    .Where(x => x.FamilyName.Equals("Default"))
            //    .FirstOrDefault();

            //double wallWidth = walls[0].Width;
            //double dt = wallWidth / 2;
            //List<XYZ> points = new List<XYZ>();
            //points.Add(new XYZ(-dt, -dt, 0));
            //points.Add(new XYZ(dt, -dt, 0));
            //points.Add(new XYZ(dt, dt, 0));
            //points.Add(new XYZ(-dt, dt, 0));
            //points.Add(new XYZ(-dt, -dt, 0));


            //Application application = doc.Application;
            //CurveArray footPrint = application.Create.NewCurveArray();
            //for (int i = 0; i < walls.Count; i++)
            //{
            //    LocationCurve curve = walls[i].Location as LocationCurve;
            //    XYZ p1 = curve.Curve.GetEndPoint(0);
            //    XYZ p2 = curve.Curve.GetEndPoint(1);
            //    Line line = Line.CreateBound(p1 + points[i], p2 + points[i + 1]);
            //    footPrint.Append(line);
            //}

            //ModelCurveArray footPrintModelCurveMapping = new ModelCurveArray();
            //FootPrintRoof footPrintRoof = doc.Create.NewFootPrintRoof(footPrint, level2, roofType, out footPrintModelCurveMapping);

            //ModelCurveArrayIterator iterator = footPrintModelCurveMapping.ForwardIterator();
            //iterator.Reset();
            //while (iterator.MoveNext())
            //{
            //    ModelCurve modelCurve = iterator.Current as ModelCurve;
            //    footPrintRoof.set_DefinesSlope(modelCurve, true);
            //    footPrintRoof.set_SlopeAngle(modelCurve, 0.5);
            //}
        }

        private void AddDoor(Document doc, Level level1, Wall wall)
        {
            FamilySymbol doorType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Doors)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("0915 х 2134 мм"))
                .Where(x => x.FamilyName.Equals("Одиночные-Щитовые"))
                .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!doorType.IsActive)
                doorType.Activate();

            doc.Create.NewFamilyInstance(point, doorType, wall, level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        private void AddWindow(Document doc, Level level1, Wall wall)
        {
            FamilySymbol windowType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_Windows)
                .OfType<FamilySymbol>()
                .Where(x => x.Name.Equals("-"))
                .Where(x => x.FamilyName.Equals("-"))
                .FirstOrDefault();

            LocationCurve hostCurve = wall.Location as LocationCurve;
            XYZ point1 = hostCurve.Curve.GetEndPoint(0);
            XYZ point2 = hostCurve.Curve.GetEndPoint(1);
            XYZ point = (point1 + point2) / 2;

            if (!windowType.IsActive)
                windowType.Activate();

            doc.Create.NewFamilyInstance(point, windowType, wall, level1, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }
    }

    public class LevelUtils
    {
        public static List<Level> GetLevels(Document doc)
        {

            List<Level> listLevel = new FilteredElementCollector(doc)
               .OfClass(typeof(Level))
               .OfType<Level>()
               .ToList();

            return listLevel;
        }
    }

    public class WallUtils
    {

        public static List<XYZ> GetWallsPoints(Document doc)
        {
            double width = UnitUtils.ConvertToInternalUnits(10000, UnitTypeId.Millimeters);
            double depth = UnitUtils.ConvertToInternalUnits(5000, UnitTypeId.Millimeters);
            double dx = width / 2;
            double dy = depth / 2;

            List<XYZ> points = new List<XYZ>();
            points.Add(new XYZ(-dx, -dy, 0));
            points.Add(new XYZ(dx, -dy, 0));
            points.Add(new XYZ(dx, dy, 0));
            points.Add(new XYZ(-dx, dy, 0));
            points.Add(new XYZ(-dx, -dy, 0));

            return points;
        }
    }
}
