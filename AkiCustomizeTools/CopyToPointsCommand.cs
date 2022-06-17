using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace AkiCustomizeTools
{
	public class CopyToPointsCommand : Command
	{
		public CopyToPointsCommand()
		{
			Instance = this;
		}

		public static CopyToPointsCommand Instance { get; private set; }
		public override string EnglishName => "CopyToPoints";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			List<ObjRef> objects = new List<ObjRef>();
			Point3d basePt = new Point3d();
			List<Point3d> pts = new List<Point3d>();
			if (!SelectObject(out objects)) return Result.Failure;
			if (!SelectBasePoint(out basePt)) return Result.Failure;
			if (!SelectPoints(out pts)) return Result.Failure;

			CopyObjects(objects, basePt, pts, doc);

			return Result.Success;
		}

		private bool SelectObject(out List<ObjRef> objectsRef)
		{
			
			using (GetObject go = new GetObject())
			{
				go.SetCommandPrompt("オブジェクトを選択してください。");
				go.GetMultiple(1, 0);

				if (go.CommandResult() != Result.Success)
				{
					objectsRef = null;
					return false;
				}
				objectsRef = new List<ObjRef>(go.ObjectCount);

				for (int i = 0; i < go.ObjectCount; i++)
				{
					objectsRef.Add(go.Object(i));
				}
			}

			return true;
		}

		private bool SelectBasePoint(out Point3d pt)
		{
			pt = new Point3d();
			using (GetPoint gp = new GetPoint())
			{
				gp.SetCommandPrompt("基点を指定してください。");
				gp.Get();

				if (gp.CommandResult() != Result.Success)
				{
					return false;
				}
				
				pt = gp.Point();
			}
			return true;
		}

		private bool SelectPoints(out List<Point3d> pts)
		{
			pts = new List<Point3d>();
			using (GetObject go = new GetObject())
			{
				go.SetCommandPrompt("コピー先のポイントを指定してください。");
				go.GeometryFilter = ObjectType.Point;
				go.GetMultiple(1, 0);

				if (go.CommandResult() != Result.Success)
				{
					return false;
				}

				for (int i = 0; i < go.ObjectCount; i++)
				{
					Point3d pt = go.Object(i).Point().Location;
					pts.Add(pt);
				}
			}

			return true;
		}

		private void CopyObjects(List<ObjRef> objRefs, Point3d basePt, List<Point3d> pts, RhinoDoc doc)
		{
			foreach (ObjRef objRef in objRefs)
			{
				foreach (Vector3d vec in GenelateVector(basePt, pts))
				{
					doc.Objects.Transform(objRef, Transform.Translation(vec), false);
				}
			}
		}

		private List<Vector3d> GenelateVector(Point3d basePt, List<Point3d> pts)
		{
			List<Vector3d> vectors = new List<Vector3d>();
			foreach (Point3d pt in pts)
			{
				vectors.Add(pt - basePt);
			}
			return vectors;
		}
	}
}
