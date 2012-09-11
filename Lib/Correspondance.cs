using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Mapping.Database
{
	[Serializable()]
	public struct Correspondence : ISerializable
	{
		public Correspondence(SerializationInfo info, StreamingContext ctxt)
		{
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

		public Correspondence(int BoardIndex, Vector3D World, Vector2D Projection)
		{
			this.BoardIndex = BoardIndex;
			this.World = World;
			this.Projection = Projection;
			this.Timestamp = DateTime.Now;
		}

		public Correspondence(int BoardIndex, Vector3D World, Vector2D Projection, DateTime Timestamp)
		{
			this.BoardIndex = BoardIndex;
			this.World = World;
			this.Projection = Projection;
			this.Timestamp = Timestamp;
		}

		public int BoardIndex;
		public Vector3D World;
		public Vector2D Projection;
		public DateTime Timestamp;

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("BoardIndex", this.BoardIndex);
			info.AddValue("World X", this.World.x);
			info.AddValue("World Y", this.World.y);
			info.AddValue("World Z", this.World.z);
			info.AddValue("Projection X", this.Projection.x);
			info.AddValue("Projection Y", this.Projection.y);
			info.AddValue("Timestamp", this.Timestamp);
		}
	}
}
