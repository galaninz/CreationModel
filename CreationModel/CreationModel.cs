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

            Transaction ts = new Transaction(doc, "Wall creation");
            ts.Start();
            List<Wall> walls = new List<Wall>();
            for (int i = 0; i < 4; i++)
            {
                Line line = Line.CreateBound(points[i], points[i + 1]);
                Wall wall = Wall.Create(doc, line, listLevel[0].Id, false);
                wall.get_Parameter(BuiltInParameter.WALL_HEIGHT_TYPE).Set(listLevel[1].Id);
                walls.Add(wall);
            }
            ts.Commit();

            return Result.Succeeded;
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
