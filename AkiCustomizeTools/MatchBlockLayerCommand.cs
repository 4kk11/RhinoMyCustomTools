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
	public class MatchBlockLayerCommand : Command
	{
		public MatchBlockLayerCommand()
		{
            Instance = this;
		}

		public static MatchBlockLayerCommand Instance { get; private set; }
		public override string EnglishName => "MatchBlockLayer0";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			List<InstanceObject> instanceObjects;
			if (!GetInstanceObjects(out instanceObjects))
			{
				RhinoApp.WriteLine("false");
				return Result.Failure;
			}

			foreach (var instanceObject in instanceObjects)
			{

				InstanceDefinition instanceDefinition = instanceObject.InstanceDefinition;

				int index = instanceDefinition.Index;
				int layerIndex = instanceObject.Attributes.LayerIndex;
				
				List<GeometryBase> geometryBases = new List<GeometryBase>();
				List<ObjectAttributes> objectAttributes = new List<ObjectAttributes>();


				RhinoObject[] rhinoObjects = instanceDefinition.GetObjects();
				foreach (var rhinoObject in rhinoObjects)
				{
					geometryBases.Add(rhinoObject.Geometry);
					ObjectAttributes att = rhinoObject.Attributes;
					att.LayerIndex = layerIndex;
					objectAttributes.Add(att);
				}

				
				if (doc.InstanceDefinitions.ModifyGeometry(index, geometryBases, objectAttributes))
					RhinoApp.WriteLine("true");
			}



			return Result.Success;
        }

		private bool GetInstanceObjects(out List<InstanceObject> instanceObjects)
		{
			instanceObjects = new List<InstanceObject>();
			using (GetObject go = new GetObject())
			{
				go.SetCommandPrompt("ブロックインスタンスを選択");
				go.GeometryFilter = ObjectType.InstanceReference;
				go.GetMultiple(1, 0);
				if (go.CommandResult() != Result.Success)
				{
					instanceObjects = null;
					return false;
				}
				foreach (ObjRef objRef in go.Objects())
				{
					InstanceObject instanceObject = objRef.Object() as InstanceObject;
					if (instanceObject == null) return false;
					instanceObjects.Add(instanceObject);
				}
			}
			return true;
		}



		private void ChangeLayerIndex(InstanceObject instanceObject)
		{
			instanceObject.Attributes.LayerIndex += 1;
			instanceObject.CommitChanges();
		}

		
	}
}
