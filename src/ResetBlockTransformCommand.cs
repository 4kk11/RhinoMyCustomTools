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
	public class ResetBlockTransformCommand : Command
	{
		static ResetBlockTransformCommand _instance;
		public ResetBlockTransformCommand()
		{
			_instance = this;
		}

		///<summary>The only instance of the ResetBlockTransformCommand command.</summary>
		public static ResetBlockTransformCommand Instance
		{
			get { return _instance; }
		}

		public override string EnglishName => "ResetBlockTransform";

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			InstanceObject instanceObject;
			ObjRef objRef;

			if (!CopyBlockInstanceCommand.SelectInstanceObject("ブロックインスタンスを選択", out instanceObject, out objRef)) return Result.Failure;

			Transform oriTrs = CopyBlockInstanceCommand.GetOriginalTransform(instanceObject);

			Guid guid = CopyBlockInstanceCommand.ResetTransform(doc, objRef, oriTrs);

			return Result.Success;
		}


	}
}