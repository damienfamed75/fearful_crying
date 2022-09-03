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
		/// <summary>
		/// Tick gets called every server tick and finds new random locations
		/// for this enemy to entity around within the NavMesh.
		/// </summary>
		public override void Tick( Vector3 currentPosition, Vector3 velocity = default, float sharpStartAngle = 60 )
		{
			base.Tick( currentPosition, velocity * 10, 360f );

            if (Path.IsEmpty && TimeUntilCanMove < 0) {
                if (Rand.Int(60) == 0)
					FindNewTarget( currentPosition );
			}
		}

		/// <summary>
		/// Finds a new random location around a radii within the NavMesh.
		/// </summary>
		/// <returns>
		/// a boolean that represents whether the new target has a value or not.
		/// </returns>
        public virtual bool FindNewTarget(Vector3 center)
        {
			var t = NavMesh.GetPointWithinRadius( center, MinRadius, MaxRadius );
            if (t.HasValue) {
				Target = t.Value;
			}

			return t.HasValue;
		}
	}
}
