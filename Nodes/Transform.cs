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
	[PluginInfo(Name = "Transform", Category = "Mapping", Version = "Correspondences", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class TransformNode : IReadsDatabase, IPluginEvaluate
	{
		#region fields & pins
		[Input("Projector Index")]
		ISpread<int> FInProjectorIndex;

		[Output("View Matrix")]
		ISpread<Matrix4x4> FOutViewMatrix;

		[Output("Projection Matrix")]
		ISpread<Matrix4x4> FOutProjectionMatrix;

		[Output("Calibrated")]
		ISpread<bool> FOutCalibrated;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate2(int SpreadMax)
		{
		}

		public override void UpdateOutput(int SpreadMax)
		{
			int ProjectorCount = FDatabase.Projectors.Count;
			FOutProjectionMatrix.SliceCount = ProjectorCount;
			FOutViewMatrix.SliceCount = ProjectorCount;
			FOutCalibrated.SliceCount = ProjectorCount;

			for (int slice = 0; slice < ProjectorCount; slice++)
			{
				if (FDatabase.Projectors.ContainsKey(FInProjectorIndex[slice]))
				{
					var Projector = FDatabase.Projectors[FInProjectorIndex[slice]];
					var Correspondences = Projector.Correspondences;

					FOutViewMatrix[slice] = Projector.View;
					FOutProjectionMatrix[slice] = Projector.Projection;
					FOutCalibrated[slice] = Projector.Calibrated;
				}
				else
				{
					FOutViewMatrix[slice] = new Matrix4x4();
					FOutProjectionMatrix[slice] = new Matrix4x4();
				}
			}
		}
	}
}
