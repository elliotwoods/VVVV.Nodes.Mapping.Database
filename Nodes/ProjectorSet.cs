#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes.Mapping.Database
{
	#region PluginInfo
	[PluginInfo(Name = "ProjectorSet", Category = "Mapping", Version = "Correspondences", Tags = "", Help = "Define a set of projector indices that we should have anyway (otherwise just wait until we've inserted points on all projector ID's).", AutoEvaluate = true)]
	#endregion PluginInfo
	public class ProjectorSetNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Database", IsSingle = true)]
		Pin<Database> FInDatabase;

		[Input("Projector Index")]
		ISpread<int> FInProjectorIndex;

		[Input("Width", DefaultValue=1024)]
		ISpread<int> FInWidth;

		[Input("Height", DefaultValue=768)]
		ISpread<int> FInHeight;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInDatabase[0] != null)
			{
				for(int slice = 0; slice < SpreadMax; slice++)
				{
					bool invalidate = false;
					if (!FInDatabase[0].Projectors.ContainsKey(FInProjectorIndex[slice]))
					{
						invalidate = true;
						FInDatabase[0].Projectors.Add(FInProjectorIndex[slice], new Projector(FInWidth[slice],FInHeight[slice]));
					}
					if (invalidate)
						FInDatabase[0].OnUpdate();
				}
			}
		}
	}
}
