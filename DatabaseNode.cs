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
	[PluginInfo(Name = "Database", Category = "Mapping", Version = "Correspondences", Tags = "")]
	#endregion PluginInfo
	public class DatabaseNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Load", IsBang=true, IsSingle=true)]
		ISpread<bool> FInLoad;

        [Input("Save", IsBang = true, IsSingle = true)]
        ISpread<bool> FInSave;

        [Input("Auto Save", IsSingle = true)]
        IDiffSpread<bool> FInAutoSave;

        [Input("Filename", StringType = StringType.Filename, IsSingle = true)]
        IDiffSpread<string> FInFilename;

		[Output("Database")]
		Pin<Database> FOutput;

        [Output("Next Board Index")]
        ISpread<int> FOutNextBoardIndex;

		[Output("Status")]
		ISpread<string> FOutStatus;

		[Import()]
		ILogger FLogger;

        Database FDatabase = new Database();
		#endregion fields & pins

        DatabaseNode()
        {
            FDatabase.Update += FDatabase_Update;
        }

        bool FValid = false;
        void FDatabase_Update(object sender, EventArgs e)
        {
            FValid = false;
        }

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            if (FOutput[0] != FDatabase)
                FOutput[0] = FDatabase;

			if (FInFilename.IsChanged)
				FDatabase.Filename = FInFilename[0];

            if (FInLoad[0])
                FDatabase.Load();

            if (FInSave[0])
                FDatabase.Save();

			if (FInAutoSave.IsChanged)
			{
				FDatabase.AutoSave = FInAutoSave[0];
			}

            if (FValid == false)
            {
                FValid = true;

                FOutNextBoardIndex[0] = FDatabase.NextBoardIndex;
            }

			FOutStatus[0] = FDatabase.Status;
		}
	}
}
