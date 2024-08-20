using Rhino;
using Rhino.Commands;
using System;
using System.Collections.Generic;
using Rhino.DocObjects;
using Rhino.Input.Custom;
using Rhino.Input;
using System.Linq;


namespace MyCustomTools
{
	public class MatchObjectAttrCommand : Command
	{
		static MatchObjectAttrCommand _instance;
		public MatchObjectAttrCommand()
		{
			_instance = this;
		}

		///<summary>The only instance of the MyCommand1 command.</summary>
		public static MatchObjectAttrCommand Instance
		{
			get { return _instance; }
		}

		public override string EnglishName => "MatchObjectAttributes";

		private bool boolLayer;
		private bool boolUserStrings;
		private bool boolMaterial;

		protected override Result RunCommand(RhinoDoc doc, RunMode mode)
		{
			
			List<ObjRef> objects = new List<ObjRef>();
			ObjRef baseObject;
			//get child objects 
			if (!SelectObjects(out objects)) return Result.Failure;
			

			//get parent object attribute
			if (!SelectBaseObject(out baseObject)) return Result.Failure;

			RhinoApp.WriteLine(baseObject.Object().Name);

			//substiitution 
			ObjectAttributes baseAtt = baseObject.Object().Attributes;
			
			foreach (ObjRef obj in objects)
			{
				ObjectAttributes att = obj.Object().Attributes;
				bool isAttChanged = false;
				if (boolLayer)
				{
					att.LayerIndex = baseAtt.LayerIndex;
					isAttChanged = true;
				}

				if (boolUserStrings)
				{
					if (baseAtt.UserStringCount == 0) break;
					foreach (string key in baseAtt.GetUserStrings().AllKeys)
					{
						att.SetUserString(key, baseAtt.GetUserString(key));
					}
					isAttChanged = true;
				}

				if (boolMaterial)
				{
					att.MaterialSource = baseAtt.MaterialSource;
					att.MaterialIndex = baseAtt.MaterialIndex;
					isAttChanged = true;
				}

				if(isAttChanged) doc.Objects.ModifyAttributes(obj, att, true);
			}


			return Result.Success;
			



		}

		private bool SelectObjects(out List<ObjRef> objectRefs)
		{
			using (GetObject go = new GetObject())
			{

				go.SetCommandPrompt("変更するオブジェクトを選択してください。");
	
				go.GetMultiple(1, 0);

				if (go.CommandResult() != Result.Success)
				{
					objectRefs = null;
					return false;
				}

				objectRefs = new List<ObjRef>(go.ObjectCount);

				for (int i = 0; i < go.ObjectCount; i++)
				{
					objectRefs.Add(go.Object(i));
				}

				

			}

			return true;
		}

		private bool SelectBaseObject(out ObjRef objectRef)
		{
			using (GetObject go = new GetObject())
			{
				OptionToggle boolLayerOption = new OptionToggle(boolLayer, "Off", "On");
				OptionToggle boolUsOption = new OptionToggle(boolUserStrings, "Off", "On");
				OptionToggle boolMaterialOption = new OptionToggle(boolMaterial, "Off", "On");

				go.SetCommandPrompt("ベースとなるオブジェクトを選択してください。");
				go.EnablePreSelect(false, true);

				

				go.AddOptionToggle("Layer", ref boolLayerOption);
				go.AddOptionToggle("UserStrings", ref boolUsOption);
				go.AddOptionToggle("Material", ref boolMaterialOption);
				
				while(true)
				{
					GetResult get_rc = go.Get();
					if (get_rc == GetResult.Option) continue;
					break;
				}
				


				if (go.CommandResult() != Result.Success)
				{
					objectRef = null;
					return false;
				}

				objectRef = go.Object(0);

				boolLayer = boolLayerOption.CurrentValue;
				boolUserStrings = boolUsOption.CurrentValue;
				boolMaterial = boolMaterialOption.CurrentValue;
			}

			return true;
		}
	}
}