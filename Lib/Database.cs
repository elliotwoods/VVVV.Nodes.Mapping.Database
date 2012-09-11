#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
#endregion usings

namespace VVVV.Nodes.Mapping.Database
{
	public class Database
	{
		Dictionary<int, Projector> FProjectors = new Dictionary<int,Projector>();
		public Dictionary<int, Projector> Projectors
		{
			get
			{
				return FProjectors;
			}
		}

        string FFilename;
        public string Filename
        {
            set
            {
                this.FFilename = value;
            }
        }

		public string Status { get; private set; }
        public void Load()
        {
			try
			{
				Database loaded;
				Stream stream = File.Open(FFilename, FileMode.Open);
				try
				{
					BinaryFormatter binaryFormatter = new BinaryFormatter();
					FProjectors = (Dictionary<int, Projector>)binaryFormatter.Deserialize(stream);

					this.Status = "Loaded " + this.FProjectors.Count + " projectors.";
					this.OnUpdate();
				}
				catch (Exception e)
				{
					throw (e);
				}
				finally
				{
					stream.Close();
				}
			}
			catch(Exception e)
			{
				this.Status = e.Message;
			}
		}

        public void Save()
        {
			try
			{
				Stream stream = File.Open(FFilename, FileMode.Create);
				try
				{
					BinaryFormatter bFormatter = new BinaryFormatter();
					bFormatter.Serialize(stream, FProjectors);
					this.Status = "Saved " + this.FProjectors.Count.ToString() + " projectors.";
				}
				catch(Exception e)
				{
					throw(e);
				}
				finally
				{
					stream.Close();
				}
			}
			catch(Exception e)
			{
				this.Status = e.Message;
			}
        }

        bool FAutoSave;
        public bool AutoSave
        {
            set
            {
                this.FAutoSave = value;
				if (value)
					Load();
            }
        }

        public void OnUpdate()
        {
            if (Update == null)
                return;
            Update(this, EventArgs.Empty);

            if (FAutoSave)
                Save();
        }

        public event EventHandler Update;

        /// <summary>
        /// Insert rows into the database
        /// </summary>
        /// <param name="ProjectorIndex"></param>
        /// <param name="BoardIndex"></param>
        /// <param name="ProjectorIndex"></param>
        /// <param name="World"></param>
        /// <param name="Projection"></param>
        /// <param name="Timestamp"></param>
        /// <returns>true if we replaced an existing row</returns>
        public void Insert(int ProjectorIndex, int BoardIndex, Vector3D World, Vector2D Projection, DateTime Timestamp)
        {
			if (!this.FProjectors.ContainsKey(ProjectorIndex))
			{
				FProjectors.Add(ProjectorIndex, new Projector());
			}
			FProjectors[ProjectorIndex].Correspondences.Add(new Correspondence(BoardIndex, World, Projection, Timestamp));
			this.OnUpdate();
        }

		public void Clear()
		{
			this.FProjectors.Clear();
			this.OnUpdate();
		}

		public int GetNextBoardIndex(int ProjectorIndex)
		{
			int nextIndex = 0;
			if (FProjectors.ContainsKey(ProjectorIndex))
			{
				foreach (Correspondence point in FProjectors[ProjectorIndex].Correspondences)
				{
					if (point.BoardIndex >= nextIndex)
						nextIndex = point.BoardIndex + 1;
				}
			}

			return nextIndex;
		}
	}
}
