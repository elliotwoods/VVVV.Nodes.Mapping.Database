#region usings
using System;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core.Logging;
using System.Collections.Generic;
#endregion usings

namespace VVVV.Nodes.Mapping.Database
{
	public class Database
	{
		struct Correspondence
        {
            public Correspondence(Vector3D World, Vector2D Projection)
            {
                this.World = World;
                this.Projection = Projection;
                this.Timestamp = DateTime.Now;
            }

            public Correspondence(Vector3D World, Vector2D Projection, DateTime Timestamp)
            {
                this.World = World;
                this.Projection = Projection;
                this.Timestamp = Timestamp;
            }

			Vector3D World;
            Vector2D Projection;
			DateTime Timestamp;
		}

        /// <summary>
        /// Dictionary of <board index, correspondence>
        /// </summary>
        Dictionary<int, Correspondence> FCorrenspondences;

        string FFilename;
        public string Filename
        {
            set
            {
                this.FFilename = value;
            }
        }

        public void Load()
        {
        }

        public void Save()
        {
        }

        bool FAutoSave;
        public bool AutoSave
        {
            set
            {
                this.FAutoSave = value;
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

        public void Insert(int BoardIndex, Vector3D World, Vector2D Projection, DateTime Timestamp)
        {
            this.FCorrenspondences.Add(BoardIndex, new Correspondence(World, Projection, Timestamp));
            FNextBoardIndex++;
        }

        int FNextBoardIndex = 0;
        public int NextBoardIndex
        {
            get
            {
                return FNextBoardIndex;
            }
        }
	}
}
