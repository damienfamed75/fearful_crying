using System.Linq;
using Sandbox;

namespace FearfulCry.Enemies;

public partial class BaseEnemy : BaseNpc
{
    [ConVar.Replicated("nav_drawpath")]
    public static bool DrawNavPath { get; set; }

	public float Speed { get; set; }
	public Entity target;
	public NavSteer Steer;
	public EnemyState EnemyState = EnemyState.None;

    // Movement
	public virtual float WalkSpeed => 40;
	public virtual float RunSpeed => 150;
	public virtual float StepSize => 20f;
	public virtual float MaxStandableAngle => 50f;
    // BoundingBox is used for movement tracing.
	public virtual BBox BoundingBox => BBox.FromHeightAndRadius( 52, 12 );

	// Attack
	public virtual float AttackSpeed { get; set; } = 1.0f;
	public virtual float AttackDamage { get; set; } = 15.0f;
	public virtual float AttackRange => 60f;

	// Alert
	public virtual bool CanAlertNearby => true;
	public virtual float NearbyAlertChance => 0.1f;
	public virtual float NearbyAlertRadius => 800f;

	// Appearance
	public virtual string ModelPath => null;
	public virtual int MaxHealth => 50;

	// Vision
	public virtual float MaxVisionRadius => 800f;
	public virtual float ChaseVisionRadius => 200f;

	// Time
	public TimeSince TimeSinceAttacked { get; set; }
    public TimeSince TimeSinceSeenTarget { get; set; }
    public TimeSince TimeSinceBurnTicked { get; set; }
	public TimeUntil TimeUntilUnstunned { get; set; }

    // Util
	private Vector3 InputVelocity;
	private Vector3 LookDirection;
	public bool JustSpawned = true;

	public override void Spawn()
    {
		base.Spawn();

		SetModel( ModelPath );

		EnableHitboxes = true;
		Health = MaxHealth;

		Tags.Add( "enemy" );
	}

    /// <summary>
	/// Called every tick on the server.
	/// </summary>
    [Event.Tick.Server]
    public virtual void Tick()
    {
		InputVelocity = 0;

        if (Steer != null) {
            // Update the steering navigation.
			Steer.Tick( Position, Velocity );

            if (!Steer.Output.Finished) {
				InputVelocity = Steer.Output.Direction.Normal;
				Velocity = Velocity.AddClamped( InputVelocity * Time.Delta * 500, Speed ); // 500
				// Debug draw velocity.
                // DebugOverlay.Line( Position, Position + InputVelocity * 500, Color.Cyan );
			}

            if (DrawNavPath) {
                DebugOverlay.Text(
                    $"v→ ‖m‖[{(int)Velocity.Length}]",
                    EyePosition + Vector3.Up * 16
                );
				Steer.DebugDrawPath();
			}
		}

		Move( Time.Delta );

		var walkVelocity = Velocity.WithZ( 0 );
        if (walkVelocity.Length > 1f) {
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 250, true ); // 100
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );

			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20f );
        }

		SimulateAnimation();

		switch (EnemyState) {
            case EnemyState.Wander:
				Wander();
				break;
            case EnemyState.Chase:
				Chase();
				break;
            default:
				break;
		}
	}

    public virtual void TryAttack()
    {

    }

	/// <summary>
	/// TryAlert attempts to get the attention of this particular zombie.
	/// </summary>
	/// <param name="target">target to be chased</param>
	/// <param name="percent">percent chance of success</param>
	/// <returns></returns>
	public bool TryAlert(Entity target, float percent)
	{
		if (Rand.Float(1) < percent) {
			StartChase( target );
			return true;
		}

		return false;
	}

	/// <summary>
	/// TryAlert will attempt to alert nearby fellow zombies with a percent chance
	/// of success.
	/// </summary>
	/// <param name="target">target to chase</param>
	/// <param name="percent">percent chance of success</param>
	/// <param name="radius">radius of zombies to be alerted</param>
	public void TryAlertNearby(Entity target, float percent, float radius)
	{
		foreach (BaseEnemy enemy in FindInSphere(Position, radius).OfType<BaseEnemy>()) {
			var chance = percent; //! todo - decrease chance by distance.
			enemy.TryAlert( target, chance );
		}
	}

    /// <summary>
	/// OVERRIDE THIS IF NOT USING CITIZEN MODEL
	/// 
	/// SimulateAnimation is called every server tick to animate the model
	/// </summary>
    protected virtual void SimulateAnimation()
    {
		var animHelper = new CitizenAnimationHelper( this );

		LookDirection = Vector3.Lerp( LookDirection, InputVelocity.WithZ( 0 ) * 1000, Time.Delta * 100.0f );
		animHelper.WithLookAt( EyePosition + LookDirection );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( InputVelocity );
		animHelper.WithWishVelocity( InputVelocity );
	}

    protected virtual void Move(float timeDelta)
    {
		EnemyMoveHelper move = new( Position, Velocity ) {
			MaxStandableAngle = MaxStandableAngle
		};

		move.Trace = move.Trace.Ignore( this ).Size( BoundingBox );

        if (!Velocity.IsNearlyZero(0.001f)) {
			move.TryUnstuck();

            if (GroundEntity != null) {
				move.TryMoveWithStep( timeDelta, StepSize );
			} else {
				move.TryMove( timeDelta );
			}
		}
		// If the enemy hits a wall and wandering around while the path is not
		// yet finished, then find a new target location.
		if (move.HitWall && EnemyState == EnemyState.Wander && !Steer.Output.Finished) {
			(Steer as Nav.Wander).FindNewTarget(Position);
		}

		//! TODO
		var tr = move.TraceDirection( Vector3.Down * 10.0f );
        if (Velocity.z < 5 && move.IsFloor(tr)) {
			SetAnimParameter( "b_grounded", true );
			GroundEntity = tr.Entity;

            if (!tr.StartedSolid) {
				move.Position = tr.EndPosition;
			}
            if (InputVelocity.Length > 0) {
				var movement = move.Velocity.Dot( InputVelocity.Normal );
				move.Velocity -= movement * InputVelocity.Normal;
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
				move.Velocity += movement * InputVelocity.Normal;
			} else {
				move.ApplyFriction( tr.Surface.Friction * 10.0f, timeDelta );
			}
		} else {
			GroundEntity = null;
			move.Velocity += Vector3.Down * 900 * timeDelta;
		}

        // if we hit a wall or prop/glass. Then we must jump over or break it.
        if (GroundEntity != null && move.HitWall && TimeUntilUnstunned < 0) {
            // Debug draw the jump line trace.
			// DebugOverlay.Line(
			// 	Position + Vector3.Up * 5, // 10
			// 	EyePosition + Vector3.Up * 40 + Steer.Output.Direction * 60,
			// 	Color.White
			// );
			var jumptr = Trace.Ray( Position + Vector3.Up * 10, EyePosition + Vector3.Up * 10 + Steer.Output.Direction * 60 )
				.UseHitboxes()
				.WithoutTags( "enemy", "trigger" )
				.EntitiesOnly()
				.Ignore( this )
				.Size( 10 )
				.Run();

			if (jumptr.Hit && EnemyState == EnemyState.Chase) {
				HitBreakableObject( jumptr );
			}
		}

		Position = move.Position;
		Velocity = move.Velocity;
	}

    public virtual void HitBreakableObject(TraceResult tr)
	{

	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );

        if (info.Attacker is Player attacker) {
            if (Health <= 0) {
				info.Attacker.Client.AddInt( "kills" );
			}

		}
	}
}