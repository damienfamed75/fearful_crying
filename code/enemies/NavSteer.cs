using Sandbox;

namespace FearfulCry.Enemies;

public class NavSteer
{
    public NavPath Path { get; private set; }
	public TimeUntil TimeUntilCanMove { get; set; } = 0;
    
    public Vector3 Target { get; set; }

    /// <summary>
	/// Output for the navsteer.
	/// </summary>
	public NavSteerOutput Output;
	public struct NavSteerOutput
    {
		public bool Finished;
		public Vector3 Direction;
	}

	public NavSteer()
    {
		Path = new NavPath();
	}

    public virtual void Tick(Vector3 currentPosition, Vector3 velocity = new Vector3(), float sharpStartAngle = 60f)
    {
        if (TimeUntilCanMove > 0)
			return;
        // Update the navigation path.
        Path.Update( currentPosition, Target, velocity, sharpStartAngle );
        // If there's nowhere left to go then we have finished.
		Output.Finished = Path.IsEmpty;
        if (Output.Finished) {
			Output.Direction = Vector3.Zero;
			return;
		}

		Output.Direction = Path.GetDirection( currentPosition );

		// Get the avoidance amount.
		var avoidance = GetAvoidance( currentPosition, 500f ); //! TODO remove magic number.
        if (!avoidance.IsNearlyZero()) {
			Output.Direction = (Output.Direction + avoidance).Normal;
		}
	}

    /// <summary>
	/// GetAvoidance returns the offset in order to avoid an obstacle.
	/// </summary>
    Vector3 GetAvoidance(Vector3 position, float radius)
    {
		var center = position + Output.Direction * radius * 0.5f;

		var objectRadius = 200f; //! TODO remove magic number.
		Vector3 avoidance = default;

		var distanceToTarget = (position - Target).Length;
        if (distanceToTarget < 300) { //! TODO remove magic number
			objectRadius -= distanceToTarget.LerpInverse( 300, 0 ) * 100;
		}
        // Get all the entities within a sphere of the radius.
        foreach (var ent in Entity.FindInSphere(center, radius)) {
            if (ent is not BaseNpc && ent is not Player)
                continue;
            
            if (ent.IsWorld)
				continue;

			var delta = position - ent.Position;
			var dist = delta.Length;
            
            if (dist < 0.001f) //! TODO remove magic number
				continue;

			var thrust = ((objectRadius - dist) / objectRadius).Clamp( 0, 1 );
            if (thrust <= 0)
				continue;

			// or maybe...
			// avoidance += delta.Cross( Output.Direction ).Normal * thrust * 2.5f;
			avoidance += delta.Normal * thrust * thrust;
		}

		return avoidance;
	}
}