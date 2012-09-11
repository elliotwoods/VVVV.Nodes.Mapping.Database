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
    [PluginInfo(Name = "Select", Category = "Mapping", Version = "Correspondences", Tags = "", AutoEvaluate = true)]
    #endregion PluginInfo
    public class SelectNode : IReadsDatabase, IPluginEvaluate
    {
        #region fields & pins
        [Input("Projector Index")]
		ISpread<int> FInProjectorIndex;

        [Output("Board Index")]
		ISpread<ISpread<int>> FOutBoardIndex;
		
        [Output("World")]
		ISpread<ISpread<Vector3D>> FOutWorld;

		[Output("Projection")]
		ISpread<ISpread<Vector2D>> FOutProjection;

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
			FOutBoardIndex.SliceCount = ProjectorCount;
			FOutProjection.SliceCount = ProjectorCount;
			FOutWorld.SliceCount = ProjectorCount;

			for (int slice = 0; slice < ProjectorCount; slice++)
			{
				if (FDatabase.Projectors.ContainsKey(FInProjectorIndex[slice]))
				{
					var Projector = FDatabase.Projectors[FInProjectorIndex[slice]];
					var Correspondences = Projector.Correspondences;

					FOutBoardIndex[slice].SliceCount = Correspondences.Count;
					FOutWorld[slice].SliceCount = Correspondences.Count;
					FOutProjection[slice].SliceCount = Correspondences.Count;
					
					for (int i = 0; i < Correspondences.Count; i++)
					{
						FOutBoardIndex[slice][i] = Correspondences[i].BoardIndex;
						FOutWorld[slice][i] = Correspondences[i].World;
						FOutProjection[slice][i] = Correspondences[i].Projection;
					}
				}
				else
				{
					FOutWorld[slice] = (ISpread<Vector3D>)new Spread<Vector3D>(0);
					FOutProjection[slice] = (ISpread<Vector2D>)new Spread<Vector2D>(0);
				}
			}
		}
    }
}
