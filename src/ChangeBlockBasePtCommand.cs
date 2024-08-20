using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using System.Linq;

namespace AkiCustomizeTools
{
	public class ChangeBlockBasePtCommand : Command
	{
		public ChangeBlockBasePtCommand()
		{
			Instance = this;
		}

		public static ChangeBlockBasePtCommand Instance { get; private set; }
		public override string EnglishName => "ChangeBlockBasePt";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			InstanceObject instanceObject;
			Point3d newBasept = new Point3d();

			if (!GetInstanceObject(out instanceObject)) return Result.Failure;
			if(!SelectBasePoint(out newBasept)) return Result.Failure;

			//Selected InstanceObject
			Point3d newBaseLocalpt = GetLocalBasept(instanceObject, newBasept);
			
			//InstanceDefinition
			InstanceDefinition instanceDefinition = instanceObject.InstanceDefinition;
			string name = instanceDefinition.Name;
			string description = instanceDefinition.Description;
			IEnumerable<GeometryBase> geometryBases = instanceDefinition.GetObjects().Select(x => x.Geometry);
			IEnumerable<ObjectAttributes> innerAttributes = instanceDefinition.GetObjects().Select(x => x.Attributes);

			
			//InstanceObjects in Table
			IEnumerable<Transform> transforms = instanceDefinition.GetReferences(0).Select(x => GenerateTransform(x, newBaseLocalpt));
			IEnumerable<ObjectAttributes> objectAttributes = instanceDefinition.GetReferences(0).Select(x => x.Attributes);
			var results = transforms.Zip(objectAttributes, (t, a) => new { transform = t, attribute = a });

			//InstanceDefinitionTable Delete & Add 
			if (doc.InstanceDefinitions.Delete(instanceDefinition))
			{
				int index = doc.InstanceDefinitions.Add(name, description, newBaseLocalpt, geometryBases, innerAttributes);

				//Add to ObjectTable
				foreach (var item in results)
				{
					doc.Objects.AddInstanceObject(index, item.transform, item.attribute);
				}

				RhinoApp.WriteLine(index.ToString());
			}

			

			return Result.Success;
		}


		private Point3d GetLocalBasept(InstanceObject instanceObject, Point3d newBasept)
		{
			Point3d newLocalBasept = newBasept;
			Transform inversed;

			instanceObject.InstanceXform.TryGetInverse(out inversed);
			newLocalBasept.Transform(inversed);

			return newLocalBasept;
		}
		private Transform GenerateTransform(InstanceObject instanceObject, Point3d point)
		{
			Transform xform = instanceObject.InstanceXform;
			Point3d basept = instanceObject.InsertionPoint;
			point.Transform(xform);

			return Transform.Translation((Vector3d)(point - basept)) * xform;
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

		private bool GetInstanceObject(out InstanceObject instanceObject)
		{
			using (GetObject go = new GetObject())
			{
				go.SetCommandPrompt("ブロックインスタンスを選択");
				go.GeometryFilter = ObjectType.InstanceReference;
				go.EnablePreSelect(false, true);
				go.Get();
				if (go.CommandResult() != Result.Success)
				{
					instanceObject = null;
					return false;
				}
			
				instanceObject = go.Object(0).Object() as InstanceObject;
				if (instanceObject == null) return false;
			}
			return true;
		}
	}
}
