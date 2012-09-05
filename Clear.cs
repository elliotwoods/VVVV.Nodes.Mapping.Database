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
	[PluginInfo(Name = "Clear", Category = "Mapping", Version = "Correspondences", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class ClearNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Database", IsSingle = true)]
		ISpread<Database> FInDatabase;

		[Input("Clear", IsBang=true)]
		IDiffSpread<bool> FInClear;

		[Output("Status")]
		ISpread<string> FOutStatus;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInClear.IsChanged)
			{
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FInDatabase[i] != null)
						if (FInClear[i]) {
							FOutStatus[0] = "Cleared " + FInDatabase[i].Correspondences.Count + " correspondences.";
							FInDatabase[i].Clear();
						}
				}
			}
		}
	}
}
