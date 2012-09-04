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
	[PluginInfo(Name = "Database", Category = "Mapping", Version = "Correspondences", Help = "Basic template with one value in/out", Tags = "")]
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

		[Import()]
		ILogger FLogger;

        Database FDatabase = new Database();
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
            if (FOutput[0] != FDatabase)
                FOutput[0] = FDatabase;

            if (FInLoad[0])
                FDatabase.Load();

            if (FInSave[0])
                FDatabase.Save();

            if (FInAutoSave.IsChanged)
                FDatabase.AutoSave = FInAutoSave[0];

            if (FInFilename.IsChanged)
                FDatabase.Filename = FInFilename[0];
		}
	}
}
