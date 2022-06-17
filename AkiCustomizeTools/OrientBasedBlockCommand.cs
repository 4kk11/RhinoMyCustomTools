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
	public class OrientBasedBlockCommand : Command
	{
		public OrientBasedBlockCommand()
		{
			Instance = this;
		}

		public static OrientBasedBlockCommand Instance { get; private set; }
		public override string EnglishName => "OrientBasedBlock";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			Transform transform;
			List<Transform> transforms;
			Transform inversed;
			List<ObjRef> objRefs;

			if (!SelectMoveObject("移動オブジェクトを選択", out objRefs)) return Result.Failure;

			if (!GetInstanceXform("起点となるブロックインスタンスを選択", out transform)) return Result.Failure;

			if (!GetInstanceXform("移動先のブロックインスタンスを指定", out transforms)) return Result.Failure;

			transform.TryGetInverse(out inversed);
			foreach (ObjRef objRef in objRefs)
			{
				foreach (Transform trs in transforms)
				{
					doc.Objects.Transform(objRef, trs * inversed, false);
				}
			}

			return Result.Success;
		}

		private bool GetInstanceXform(string callText, out Transform transform)
		{
			using (GetObject go = new GetObject())
			{
				go.SetCommandPrompt(callText);
				go.GeometryFilter = ObjectType.InstanceReference;
				go.EnablePreSelect(false, true);
				go.Get();
				if (go.CommandResult() != Result.Success)
				{
					transform = new Transform();
					return false;
				}
				InstanceReferenceGeometry iRefGeo = go.Object(0).Geometry() as InstanceReferenceGeometry;
				if (iRefGeo == null)
				{
					transform = new Transform();
					return false;
				} 
				transform = iRefGeo.Xform;
			}
			return true;
		}

		private bool GetInstanceXform(string callText, out List<Transform> transforms)
		{
			transforms = new List<Transform>();
			using (GetObject go = new GetObject())
			{
				go.SetCommandPrompt(callText);
				go.GeometryFilter = ObjectType.InstanceReference;
				go.EnablePreSelect(false, true);
				go.GetMultiple(1, 0);
				if (go.CommandResult() != Result.Success)
				{
					transforms = null;
					return false;
				}
				foreach (var objRef in go.Objects())
				{
					InstanceReferenceGeometry iRefGeo = objRef.Geometry() as InstanceReferenceGeometry;
					if (iRefGeo == null) return false;
					transforms.Add(iRefGeo.Xform);
				}				
			}
			return true;
		}

		private bool SelectMoveObject(string callText, out List<ObjRef> objRefs)
		{
			objRefs = new List<ObjRef>();
			using (GetObject go2 = new GetObject())
			{
				go2.SetCommandPrompt("移動オブジェクトを選択");
				go2.GroupSelect = true;
				go2.EnablePreSelect(true, true);
				go2.GetMultiple(1, 0);
				if (go2.CommandResult() != Result.Success)
				{
					objRefs = null;
					return false;
				}
				foreach (ObjRef objRef in go2.Objects())
				{
					objRefs.Add(objRef);
				}
				
			}
			return true;
		}
		
	}
}
