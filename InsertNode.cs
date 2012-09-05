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
    [PluginInfo(Name = "Insert", Category = "Mapping", Version = "Correspondences", Tags = "", AutoEvaluate=true)]
    #endregion PluginInfo
    public class InsertNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Database", IsSingle = true)]
        Pin<Database> FInDatabase;

        [Input("Projector Index")]
        ISpread<int> FInProjectorIndex;

        [Input("Board Index")]
        ISpread<int> FInBoardIndex;

        [Input("World")]
        ISpread<Vector3D> FInWorld;

        [Input("Projection")]
        ISpread<Vector2D> FInProjection;

        [Input("Insert", IsBang = true)]
        IDiffSpread<bool> FInInsert;

        [Output("Status")]
        ISpread<string> FOutStatus;

        [Import()]
        ILogger FLogger;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FInInsert.IsChanged)
            {
                if (FInDatabase[0] == null)
                {
                    FOutStatus[0] = "No database connected";
                    return;
				}
				else if (FOutStatus[0] == "No database connected")
				{
					FOutStatus[0] = "OK";
				}

                DateTime Timestamp = DateTime.Now;
                int BoardIndex = FInDatabase[0].NextBoardIndex;

                int countInsert = 0;
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FInInsert[i])
                    {
						FInDatabase[0].Insert(FInProjectorIndex[i], FInBoardIndex[i], FInWorld[i], FInProjection[i], Timestamp);
                        countInsert++;
                    }
                }
                if (countInsert > 0)
                    FOutStatus[0] = "Inserted " + countInsert.ToString() + " correspondences." ;
            }
        }
    }
}
