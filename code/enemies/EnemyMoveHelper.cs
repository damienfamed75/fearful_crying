using Sandbox;

namespace FearfulCry.Enemies;

public struct EnemyMoveHelper
{
    /// <summary>
	/// Inputs and outputs
	/// </summary>
	public Vector3 Position;
	public Vector3 Velocity;
	public bool HitWall;

    /// <summary>
	/// config
	/// </summary>
	public float GroundBounce;
	public float WallBounce;
	public float MaxStandableAngle;
	public Trace Trace;

    /// <summary>
	/// Create the movehelper and initialize it with the default settings.
	/// Trace and MaxStandableAngle can be adjusted after creation.
	/// </summary>
	/// <example>
	/// var move = new MoveHelper( Position, Velocity );
	/// </example>
    public EnemyMoveHelper(Vector3 position, Vector3 velocity, params string[] solidTags) : this()
    {
		Velocity = velocity;
		Position = position;
		GroundBounce = 0.0f;
		WallBounce = 0.0f;
		MaxStandableAngle = 10.0f;

		Trace = Trace.Ray( 0, 0 )
			.WorldAndEntities()
			.WithAnyTags( solidTags )
			.WithoutTags( "gib", "ragdoll" );
	}

    /// <summary>
	/// Trace this from one position to another.
	/// </summary>
    public TraceResult TraceFromTo(Vector3 from, Vector3 to)
    {
		return Trace.FromTo( from, to ).Run();
	}

    /// <summary>
	/// Trace this from its current Position to a delta.
	/// </summary>
    public TraceResult TraceDirection(Vector3 down)
    {
		return TraceFromTo( Position, Position + down );
	}

    /// <summary>
	/// Based on the MaxStandableAngle, determine if the trace hit a standable floor.
	/// </summary>
    public bool IsFloor(TraceResult tr)
    {
        if (!tr.Hit)
			return false;

		return tr.Normal.Angle( Vector3.Up ) < MaxStandableAngle;
	}

    /// <summary>
	/// TryMove will try to move to the position.
	/// Position and Velocity make most sense to use in this.
	/// </summary>
	/// <return>a fraction of the desired velocity that we traveled.</return>
    public float TryMove(float timestep)
    {
		var timeLeft = timestep;
		float travelFraction = 0;
		HitWall = false;

		using var moveplanes = new VelocityClipPlanes( Velocity );

		for ( int bump = 0; bump < moveplanes.Max; bump++ )
		{
			if ( Velocity.Length.AlmostEqual( 0.0f ) )
				break;

			var pm = TraceFromTo( Position, Position + Velocity * timeLeft );
			travelFraction += pm.Fraction;

            //! TODO remove magic number
			if ( pm.Fraction > 0.03125f ) {
				Position = pm.EndPosition + pm.Normal * 0.001f;

                if (pm.Fraction == 1)
					break;

				moveplanes.StartBump( Velocity );
			}

            if (bump == 0 && pm.Hit && pm.Normal.Angle(Vector3.Up) >= MaxStandableAngle) {
				HitWall = true;
			}

			timeLeft -= timeLeft * pm.Fraction;

            // Get the bounce amount based on whether this is a wall or ground.
			float bounce = GroundBounce;
            if (!IsFloor(pm)) {
				bounce = WallBounce;
			}

			if (moveplanes.TryAdd(pm.Normal, ref Velocity, bounce))
				break;
		}

        if (travelFraction == 0)
			Velocity = 0;

		return travelFraction;
	}

    /// <summary>
	/// Apply an amount of friction to the velocity.
	/// </summary>
    public void ApplyFriction(float frictionAmount, float delta)
    {
		float StopSpeed = 100.0f;

		var speed = Velocity.Length;
        if (speed < 0.1f)
			return;

        // Bleed off some speed, but if we have less than the bleed threshold,
        // bleed the threshold amount instead.
		float control = (speed < StopSpeed) ? StopSpeed : speed;

        // Add the amount to the drop amount.
		var drop = control * delta * frictionAmount;

        // Scale the velocity.
        float newSpeed = speed - drop;
        if (newSpeed < 0)
			newSpeed = 0;
        
        if (newSpeed == speed)
			return;

		newSpeed /= speed;
		Velocity *= newSpeed;
	}

    /// <summary>
	/// Move our position by this delta using a trace. If we hit something we'll
	/// stop, we won't slide across it nicely like TryMove does.
	/// </summary>
    public TraceResult TraceMove(Vector3 delta)
    {
		var tr = TraceFromTo( Position, Position + delta );
		Position = tr.EndPosition;
		return tr;
	}

    /// <summary>
	/// Like TryMove but will also try to step up if it hits a wall.
	/// </summary>
    public float TryMoveWithStep(float timeDelta, float stepSize)
    {
		var startPosition = Position;
		// Make a copy of us to stepMove.
        var stepMove = this;
        // Get the fraction and do a regular movement.
        var fraction = TryMove(timeDelta);

        // Move up as much as we can.
		stepMove.TraceMove( Vector3.Up * stepSize );
        // Move across using the existing velocity.
		var stepFraction = stepMove.TryMove( timeDelta );
		// Move back down.
        var tr = stepMove.TraceMove( Vector3.Down * stepSize );
        // If we didn't land on something then return.
        if (!tr.Hit)
			return fraction;

        // If we landed on a wall then this is isn't valid.
        if (tr.Normal.Angle(Vector3.Up) > MaxStandableAngle)
			return fraction;

        // If the original non-stepped attempt moved further then use that.
        if (startPosition.Distance(Position.WithZ(startPosition.z)) > startPosition.Distance(stepMove.Position.WithZ(startPosition.z)))
            return fraction;

        // StepMove moved further, so copy its data to us.
		Position = stepMove.Position;
		Velocity = stepMove.Velocity;
		HitWall = stepMove.HitWall;

		return stepFraction;
	}

    /// <summary>
	/// First test if we're stuck, if so then try to unstuck.
	/// </summary>
    public bool TryUnstuck()
    {
		var tr = TraceFromTo( Position, Position );
        if (!tr.StartedSolid)
			return true;

		return Unstuck();
	}

    /// <summary>
	/// When you're between a rock and a hard place, get outta there.
	/// </summary>
    bool Unstuck()
    {
        // Attempt to go straight up first.
		for ( int i = 1; i < 20;  i++) {
			var tryPos = Position + Vector3.Up * i;

			var tr = TraceFromTo( tryPos, Position );
            if (!tr.StartedSolid) {
				Position = tryPos + tr.Direction.Normal * (tr.Distance - 0.5f);
				Velocity = 0;
				return true;
			}
		}

        //!TODO more advanced unstucking.

		return false;
	}
}