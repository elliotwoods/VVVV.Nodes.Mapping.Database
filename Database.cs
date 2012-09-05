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
	[Serializable()]
	public class Database : ISerializable
	{
		[Serializable()]
		public struct Correspondence : ISerializable
        {
			public Correspondence(SerializationInfo info, StreamingContext ctxt)
			{
				this.ProjectorIndex = (int)info.GetValue("ProjectorIndex", typeof(int));
				this.BoardIndex = (int)info.GetValue("BoardIndex", typeof(int));

				double x, y, z;
				x = (double)info.GetValue("World X", typeof(double));
				y = (double)info.GetValue("World Y", typeof(double));
				z = (double)info.GetValue("World Z", typeof(double));
				this.World = new Vector3D(x, y, z);

				x = (double)info.GetValue("Projection X", typeof(double));
				y = (double)info.GetValue("Projection Y", typeof(double));
				this.Projection = new Vector2D(x, y);

				this.Timestamp = (DateTime)info.GetValue("Timestamp", typeof(DateTime));
			}

            public Correspondence(int ProjectorIndex, int BoardIndex, Vector3D World, Vector2D Projection)
            {
                this.ProjectorIndex = ProjectorIndex;
				this.BoardIndex = BoardIndex;
				this.World = World;
                this.Projection = Projection;
                this.Timestamp = DateTime.Now;
            }

            public Correspondence(int ProjectorIndex, int BoardIndex, Vector3D World, Vector2D Projection, DateTime Timestamp)
            {
                this.ProjectorIndex = ProjectorIndex;
				this.BoardIndex = BoardIndex;
                this.World = World;
                this.Projection = Projection;
                this.Timestamp = Timestamp;
            }

            public int ProjectorIndex;
			public int BoardIndex;
			public Vector3D World;
			public Vector2D Projection;
			public DateTime Timestamp;

			public void GetObjectData(SerializationInfo info, StreamingContext context)
			{
				info.AddValue("ProjectorIndex", this.ProjectorIndex);
				info.AddValue("BoardIndex", this.BoardIndex);
				info.AddValue("World X", this.World.x);
				info.AddValue("World Y", this.World.y);
				info.AddValue("World Z", this.World.z);
				info.AddValue("Projection X", this.Projection.x);
				info.AddValue("Projection Y", this.Projection.y);
				info.AddValue("Timestamp", this.Timestamp);
			}
		}

        /// <summary>
        /// Dictionary of <board index, correspondence>
        /// </summary>
        List<Correspondence> FCorrenspondences = new List<Correspondence>();
		public List<Correspondence> Correspondences
		{
			get
			{
				return FCorrenspondences;
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
					loaded = (Database)binaryFormatter.Deserialize(stream);

					this.FCorrenspondences = loaded.FCorrenspondences;
					this.Status = "Loaded";
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
					bFormatter.Serialize(stream, this);
					this.Status = "Saved " + this.FCorrenspondences.Count.ToString() + " rows.";
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
            this.FCorrenspondences.Add(new Correspondence(ProjectorIndex, BoardIndex, World, Projection, Timestamp));
			this.OnUpdate();
        }

		public void Clear()
		{
			this.FCorrenspondences.Clear();
			this.OnUpdate();
		}

		public int NextBoardIndex
		{
			get
			{
				int nextIndex = 0;
				foreach (Correspondence point in FCorrenspondences)
				{
					if (point.BoardIndex >= nextIndex)
						nextIndex = point.BoardIndex + 1;
				}

				return nextIndex;
			}
		}

		public Database()
		{
		}

		public Database(SerializationInfo info, StreamingContext ctxt)
		{
			this.FCorrenspondences = (List<Correspondence>)info.GetValue("Correspondences", typeof(List<Correspondence>));
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Correspondences", this.FCorrenspondences);
		}
	}
}
