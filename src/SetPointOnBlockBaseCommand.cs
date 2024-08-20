using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace MyCustomTools
{
	public class SetPointOnBlockBaseCommand : Command
	{
		static SetPointOnBlockBaseCommand _instance;
		public SetPointOnBlockBaseCommand()
		{
			_instance = this;
		}

		///<summary>The only instance of the SetPointOnBlockBaseCommand command.</summary>
		public static SetPointOnBlockBaseCommand Instance
		{
			get { return _instance; }
		}

		public override string EnglishName => "SetPointOnBlockBase";


		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			InstanceObject instanceObject;
			
			if (!SelectInstanceObject("基点を置くブロックインスタンスを選択", out instanceObject)) return Result.Failure;
			

			InstanceDefinition instanceDefinition = instanceObject.InstanceDefinition;
			int index = instanceDefinition.Index;

			RhinoObject[] rhinoObjects = instanceDefinition.GetObjects();
			List<GeometryBase> geometryBases = new List<GeometryBase>();
			List<ObjectAttributes> objectAttributes = new List<ObjectAttributes>();

			foreach (var rhinoObject in rhinoObjects)
			{
				geometryBases.Add(rhinoObject.Geometry);
				objectAttributes.Add(rhinoObject.Attributes);
			}

			geometryBases.Add(new Point(new Point3d(0,0,0)));
			ObjectAttributes pointAtt = new ObjectAttributes();
			objectAttributes.Add(pointAtt);
			
			pointAtt.LayerIndex = instanceObject.Attributes.LayerIndex;
			
			doc.InstanceDefinitions.ModifyGeometry(index, geometryBases, objectAttributes);

			return Result.Success;
		}

		private bool SelectInstanceObject(string callText, out InstanceObject instanceObject)
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
					return false;
				}

				instanceObject = go.Object(0).Object() as InstanceObject;
				if(instanceObject == null) return false;
			}

			return true;
		}
	}
}