#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using VVVV.Nodes.OpenCV;
#endregion usings

namespace VVVV.Nodes.Mapping.Database
{
	#region PluginInfo
	[PluginInfo(Name = "Intrinsics", Category = "Mapping", Version = "Correspondences", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class IntrinsicsNode : IReadsDatabase, IPluginEvaluate
	{
		#region fields & pins
		[Input("Projector Index")]
		IDiffSpread<int> FInProjectorIndex;

		[Output("Intrinsics")]
		ISpread<Intrinsics> FOutIntrinsics;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public override void Evaluate2(int SpreadMax)
		{
			if (FInProjectorIndex.IsChanged)
				this.Invalidate();
		}

		public override void UpdateOutput(int SpreadMax)
		{
			int ProjectorCount = FDatabase.Projectors.Count;
			FOutIntrinsics.SliceCount = ProjectorCount;

			for (int slice = 0; slice < ProjectorCount; slice++)
			{
				if (FDatabase.Projectors.ContainsKey(FInProjectorIndex[slice]))
				{
					var Projector = FDatabase.Projectors[FInProjectorIndex[slice]];
					var Correspondences = Projector.Correspondences;

					FOutIntrinsics[slice] = Projector.Intrinsics;
				}
				else
				{
					FOutIntrinsics[slice] = null;
				}
			}
		}
	}
}
