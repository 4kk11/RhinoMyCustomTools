using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace AkiCustomizeTools
{
	public class CopyBlockInstanceCommand : Command
	{
		static CopyBlockInstanceCommand _instance;
		public CopyBlockInstanceCommand()
		{
			_instance = this;
		}
		public static CopyBlockInstanceCommand Instance
		{
			get { return _instance; }
		}

		public override string EnglishName => "CopyBlockInstance";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			InstanceObject instanceObject;
			ObjRef objRef;
			if (!SelectInstanceObject("コピーしたいブロックインスタンスを選択", out instanceObject, out objRef)) return Result.Failure;
			
			Transform oriTrs = GetOriginalTransform(instanceObject);

			Guid guid = ResetTransform(doc, objRef, oriTrs);

			int index = CreateNewInstance(doc, instanceObject);

			doc.Objects.AddInstanceObject(index, oriTrs, instanceObject.Attributes);
			doc.Objects.Delete(guid,true);

			return Result.Success;
		}

		public static bool SelectInstanceObject(string callText, out InstanceObject instanceObject, out ObjRef objRef)
		{
			using (GetObject go = new GetObject())
			{
				go.SetCommandPrompt(callText);
				go.GeometryFilter = ObjectType.InstanceReference;
				go.EnablePreSelect(true, true);
				go.Get();
				if (go.CommandResult() != Result.Success)
				{
					instanceObject = null;
					objRef = null;
					return false;
				}

				objRef = go.Object(0);
				instanceObject = go.Object(0).Object() as InstanceObject;
				if (instanceObject == null) return false;
			}

			return true;
		}

		public static Guid ResetTransform(RhinoDoc doc , ObjRef objRef, Transform trs)
		{
			Transform inversed;
			trs.TryGetInverse(out inversed);

			
			return doc.Objects.Transform(objRef, inversed, true);
		}

		public static Transform GetOriginalTransform(InstanceObject instanceObject)
		{
			InstanceReferenceGeometry irefGeo = instanceObject.Geometry as InstanceReferenceGeometry;
			return irefGeo.Xform;
		}

		private int CreateNewInstance(RhinoDoc doc, InstanceObject instanceObject)
		{
			InstanceDefinition instanceDefinition = instanceObject.InstanceDefinition;
			string name = instanceDefinition.Name + DateTime.Now.ToString();
			string description = instanceDefinition.Description;
			IEnumerable<GeometryBase> geometryBases = instanceDefinition.GetObjects().Select(x => x.Geometry);
			IEnumerable<ObjectAttributes> innerAttributes = instanceDefinition.GetObjects().Select(x => x.Attributes);

			return doc.InstanceDefinitions.Add(name, description, Point3d.Origin, geometryBases, innerAttributes);

		}
	}
}