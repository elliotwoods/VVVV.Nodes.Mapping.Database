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
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;
using Emgu.CV.CvEnum;
#endregion usings

namespace VVVV.Nodes.Mapping.Database
{
	#region PluginInfo
	[PluginInfo(Name = "Calibrate", Category = "Mapping", Version = "Correspondences", Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class CalibrateNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Database", IsSingle = true)]
		Pin<Database> FInDatabase;

		[Input("Width", DefaultValue = 1024, IsSingle=true)]
		ISpread<int> FInWidth;

		[Input("Height", DefaultValue = 768, IsSingle = true)]
		ISpread<int> FInHeight;

		[Input("Solve", IsBang = true, IsSingle = true)]
		ISpread<bool> FInSolve;

		[Import()]
		ILogger FLogger;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FInSolve[0])
			{
				if (FInDatabase[0] != null)
				{
					foreach (var Projector in FInDatabase[0].Projectors)
						Projector.Value.Calibrate(new Size(FInWidth[0], FInHeight[0]));
					FInDatabase[0].OnUpdate();
				}
			}
		}
	}
}
