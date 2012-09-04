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
    [PluginInfo(Name = "Insert", Category = "Mapping", Version = "Correspondences", Help = "Basic template with one value in/out", Tags = "")]
    #endregion PluginInfo
    public class InsertNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Database", IsSingle = true)]
        Pin<Database> FInDatabase;

        [Input("Board Index")]
        ISpread<int> FInIndex;

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

                DateTime Timestamp = DateTime.Now;
                int BoardIndex = FInDatabase[0].NextBoardIndex;

                int count = 0;
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FInInsert[i])
                    {
                        FInDatabase[0].Insert(BoardIndex, FInWorld[i], FInProjection[i], Timestamp);
                        count++;
                    }
                }
                FOutStatus[0] = "Inserted " + count.ToString() + " correspondences.";
            }
        }
    }
}
