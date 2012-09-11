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
	public abstract class IReadsDatabase
	{
		#region fields & pins
		[Input("Database", IsSingle = true, Order=-1)]
		Pin<Database> FInDatabase;

		protected Database FDatabase;
		#endregion fields & pins

		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (FDatabase != FInDatabase[0])
				this.SetDatabase(FInDatabase[0]);

			Evaluate2(SpreadMax);

			if (!FValid)
			{
				FValid = true;
				if (FDatabase != null)
					UpdateOutput(SpreadMax);
			}
		}

		public abstract void UpdateOutput(int SpreadMax);
        public abstract void Evaluate2(int SpreadMax);

		void SetDatabase(Database database)
		{
			if (FDatabase != null)
				FDatabase.Update -= FDatabase_Update;
			FDatabase = database;
			if (FDatabase != null)
			{
				FDatabase.Update += FDatabase_Update;
				this.Invalidate();
			}
		}

		bool FValid = true;
		void FDatabase_Update(object sender, EventArgs e)
		{
			this.Invalidate();
		}

		protected void Invalidate()
		{
			FValid = false;
		}
	}
}
