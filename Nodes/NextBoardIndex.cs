#region usings
using System;
using System.ComponentModel.Composition;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Nodes.Mapping.Database
{
	#region PluginInfo
	[PluginInfo(Name = "NextBoardIndex", Category = "Mapping", Version = "Correspondences", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class NextBoardIndex : IReadsDatabase, IPluginEvaluate
	{
		#region fields & pins
		[Input("Projector Index")]
		ISpread<int> FInProjectorIndex;

		[Output("Next Board Index")]
		ISpread<int> FOutNextBoardIndex;

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
			FOutNextBoardIndex.SliceCount = SpreadMax;
			for (int i=0; i<SpreadMax; i++)
			{
				FOutNextBoardIndex[i] = FDatabase.GetNextBoardIndex(FInProjectorIndex[i]);
			}
		}
	}
}
