using System;
using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace MyCustomTools
{
	public class InvisibleLayerFromObjectCommand : Command
	{
		static InvisibleLayerFromObjectCommand _instance;
		public InvisibleLayerFromObjectCommand()
		{
			_instance = this;
		}

		///<summary>The only instance of the InvisibleLayerFromObjectCommand command.</summary>
		public static InvisibleLayerFromObjectCommand Instance
		{
			get { return _instance; }
		}

		public override string EnglishName => "InvisibleLayerFromObject";
		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			RhinoObject rhObj;
			if(!SelectObject("非表示にしたいレイヤーのオブジェクトを選択", out rhObj)) return Result.Failure;

			int layerIndex = rhObj.Attributes.LayerIndex;

			Layer layer = doc.Layers.FindIndex(layerIndex);
			layer.IsVisible = false;

			return Result.Success;
		}

		private bool SelectObject(string callText, out RhinoObject rhObj)
		{
			rhObj = null;
			using (GetObject go = new GetObject())
			{
				go.SetCommandPrompt(callText);
				go.EnablePreSelect(true, true);
				go.Get();
				if (go.CommandResult() != Result.Success)
				{
					return false;
				}
				rhObj = go.Object(0).Object();
			}
			return true;
		}
	}
}