using Sandbox;

namespace FearfulCry.Enemies.Nav
{
    public class Wander : NavSteer
    {
		public float MinRadius { get; set; } = 500;
		public float MaxRadius { get; set; } = 1000;

        public Wander()
        {

        }

		public override void Tick( Vector3 currentPosition, Vector3 velocity = default, float sharpStartAngle = 60 )
		{
			base.Tick( currentPosition, velocity * 10, 360f );

            if (Path.IsEmpty && TimeUntilCanMove < 0) {
                if (Rand.Int(60) == 0)
					FindNewTarget( currentPosition );
			}
		}

        public virtual bool FindNewTarget(Vector3 center)
        {
			Log.Info( $"navmesh loaded {NavMesh.IsLoaded}" );

			var t = NavMesh.GetPointWithinRadius( center, MinRadius, MaxRadius );
            if (t.HasValue) {
				Target = t.Value;
			}

			return t.HasValue;
		}
	}
}
