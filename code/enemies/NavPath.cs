using System.Collections.Generic;
using System.Linq;
using Sandbox;

namespace FearfulCry.Enemies;

public class NavPath
{
	public Vector3 TargetPosition;
	public List<Vector3> Points = new();

	public bool IsEmpty => Points.Count <= 1;

    public void Update(Vector3 from, Vector3 to, Vector3 velocity = new Vector3(), float sharpStartAngle = 45.0f)
    {
		bool needsBuild = false;

        if (!TargetPosition.AlmostEqual(to, 5)) {
			TargetPosition = to;
			needsBuild = true;
		}

        if (needsBuild) {
			var fromFixed = NavMesh.GetClosestPoint( from );
			var toFixed = NavMesh.GetClosestPoint( to );

            // If this is the case then a zombie could've fallen out of the map.
            if (fromFixed == null || toFixed == null) {
				Log.Warning( "nav out of bounds" );
				return;
			}

			Points.Clear();

            // How a path if calculated: PathLength += (Fall distance * Scale)
            //! TODO remove magic numbers.
			var path = NavMesh.PathBuilder( fromFixed.Value )
				.WithSharpStartAngle( sharpStartAngle )
				.WithStartVelocity( velocity / 2 )
				.WithMaxClimbDistance( 500 )
				.WithMaxDropDistance( 3000 )
				.WithMaxDetourDistance( 100 )
				.WithDropDistanceCostScale( 2f )
				.WithPartialPaths()
				.Build( toFixed.Value );

			var segments = path.Segments;
			Points = segments.Select( s => s.Position ).ToList();
		}

        if (Points.Count <= 1) {
			return;
		}

		var deltaToCurrent = from - Points[0];
		var deltaToNext = from - Points[1];
        var delta = Points[1] - Points[0];
		var deltaNormal = delta.Normal;

        // default 20
        if (deltaToNext.WithZ(0).Length < 45) {
			Points.RemoveAt( 0 );
			return;
		}
        // If we are in front of this line then remove it and move on to the next one.
        if (deltaToNext.Normal.Dot(deltaNormal) >= 1.0f) {
			Points.RemoveAt( 0 );
		}
	}

    public float Distance(int point, Vector3 from)
    {
        if (Points.Count <= point)
			return float.MaxValue;

		return Points[point].WithZ( from.z ).Distance( from );
	}

    public Vector3 GetDirection(Vector3 position)
    {
        // If there's only one point then just return the first point normal.
        if (Points.Count == 1) {
			return (Points[0] - position).WithZ( 0 ).Normal;
		}

		return (Points[1] - position).WithZ( 0 ).Normal;
	}

    public void DebugDraw(float time, float opacity = 1.0f)
    {
        //! TODO
    }
}