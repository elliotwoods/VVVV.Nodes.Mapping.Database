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
    public class SelectNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Database", IsSingle = true)]
        Pin<Database> FInDatabase;

        [Output("Projector Index")]
        ISpread<int> FOutProjectorIndex;

        [Output("Board Index")]
		ISpread<int> FOutBoardIndex;
		
        [Output("World")]
		ISpread<Vector3D> FOutWorld;

        [Output("Projection")]
		ISpread<Vector2D> FOutProjection;

        [Import()]
        ILogger FLogger;

		Database FDatabase;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
			if (FDatabase != FInDatabase[0])
				this.SetDatabase(FInDatabase[0]);

			if (!FValid)
			{
				FValid = true;
				if (FDatabase == null)
					return;
				
				int count = FDatabase.Correspondences.Count;
				FOutProjectorIndex.SliceCount = count;
				FOutBoardIndex.SliceCount = count;
				FOutWorld.SliceCount = count;
				FOutProjection.SliceCount = count;

				int slice = 0;
				foreach(var point in FDatabase.Correspondences)
				{
					FOutProjectorIndex[slice] = point.ProjectorIndex;
					FOutBoardIndex[slice] = point.BoardIndex;
					FOutWorld[slice] = point.World;
					FOutProjection[slice] = point.Projection;
					slice++;
				}
			}
        }

		void SetDatabase(Database database)
		{
			if (FDatabase != null)
				FDatabase.Update -= FDatabase_Update;
			FDatabase = database;
			if (FDatabase != null)
			{
				FDatabase.Update += FDatabase_Update;
				FValid = false;
			}
		}

		bool FValid = true;
		void FDatabase_Update(object sender, EventArgs e)
		{
			FValid = false;
		}
    }
}
