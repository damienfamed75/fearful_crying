using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime;
using Sandbox;
using Sandbox.Internal;
using SandboxEditor;

namespace FearfulCry.Enemies;

/// <summary>
/// States for a zombie to have
/// </summary>
public enum ZombieState
{
    Wander,
    Chase,
}

[Library("enemy_basezombie"), HammerEntity]
[Title("BaseZombie"), Category("Enemy")]
[EditorModel("models/citizen/citizen.vmdl")]
public partial class BaseZombie : BaseNpc
{
    [ConVar.Replicated]
    public static bool nav_drawpath { get; set; }

    [ConCmd.Server("npc_clear")]
    public static void NpcClear()
    {
        foreach (var npc in Entity.All.OfType<BaseZombie>().ToArray()) {
			npc.Delete();
		}
    }

    public float Speed { get; set; }
	public Entity target;

	NavPath path = new NavPath();
	public NavSteer Steer;

	public ZombieState ZombieState = ZombieState.Wander;
	public float WalkSpeed = Rand.Float( 40, 50 );
	public float RunSpeed = Rand.Float( 150, 170 ); // for reference, the player speed is 300
	
	//
	// Attack information
	//
	public float AttackSpeed = 1.0f;
	public float AttackDamage = 15.0f;

	public static float StepSize = 20f;
    public TimeSince TimeSinceAttacked = 0;
	public TimeUntil TimeUntilUnstunned = 0;
	public TimeSince TimeSinceBurnTicked = 0;
	public TimeSince TimeSinceSeenTarget;

	public override void Spawn()
    {
		base.Spawn();

		SetModel( "models/citizen/citizen.vmdl" );
		EyePosition = Position + Vector3.Up * 60;
		SetupPhysicsFromCapsule(
            PhysicsMotionType.Keyframed,
            Capsule.FromHeightAndRadius( 72, 8 )
        );

		UpdateClothes();
		Dress(Color.Parse("#8cab7e"), Color.Parse("#A3A3A3"));

		EnableHitboxes = true;
		Speed = Rand.Float( 250, 300 ); //! TODO remove magic number
		WalkSpeed = Rand.Float( 40, 60 );
		Health = 50;

        // collision tags.
		Tags.Add( "zombie" );
	}

	Vector3 InputVelocity;
	Vector3 LookDirection;

    /// <summary>
	/// Called every tick on the server.
	/// </summary>
    [Event.Tick.Server]
    public virtual void Tick()
    {
		InputVelocity = 0;

        if (Steer != null) {
            // Update the navigation steering.
			Steer.Tick( Position, Velocity );

            if (!Steer.Output.Finished) {
				InputVelocity = Steer.Output.Direction.Normal;
				Velocity = Velocity.AddClamped( InputVelocity * Time.Delta * 200, Speed );
			}

            if (nav_drawpath) {
				DebugOverlay.Text( ((int)Velocity.Length).ToString(), EyePosition + Vector3.Up * 16 );
                // Debug draw path
			}
		}

		// SetAnimParameter( "b_climbing", false );

        Move(Time.Delta);

        var walkVelocity = Velocity.WithZ(0);
        if (walkVelocity.Length > 1f) {
			var turnSpeed = walkVelocity.Length.LerpInverse( 0, 250, true ); // 100
			var targetRotation = Rotation.LookAt( walkVelocity.Normal, Vector3.Up );

			Rotation = Rotation.Lerp( Rotation, targetRotation, turnSpeed * Time.Delta * 20f );
		}

		var animHelper = new CitizenAnimationHelper( this );

		LookDirection = Vector3.Lerp( LookDirection, InputVelocity.WithZ( 0 ) * 1000, Time.Delta * 100.0f );
		animHelper.WithLookAt( EyePosition + LookDirection );
		animHelper.WithVelocity( Velocity );
		animHelper.WithWishVelocity( InputVelocity );
		animHelper.WithWishVelocity( InputVelocity );
	}

    protected virtual void Move(float timeDelta)
    {
		var bbox = BBox.FromHeightAndRadius( 52, 4 );

		EnemyMoveHelper move = new( Position, Velocity );
		move.MaxStandableAngle = 45f;
		move.Trace = move.Trace.Ignore( this ).Size( bbox );

        if (!Velocity.IsNearlyZero(0.001f)) {
			move.TryUnstuck();

            if (GroundEntity != null) {
				move.TryMoveWithStep( timeDelta, StepSize );
			} else {
				move.TryMove( timeDelta );
			}
		}

		//! TODO
		var tr = move.TraceDirection( Vector3.Down * 10.0f );
        if (Velocity.z < 5 && move.IsFloor(tr)) {
			// SetAnimParameter( "b_grounded", true );
			GroundEntity = tr.Entity;

            if (!tr.StartedSolid) {
				move.Position = tr.EndPosition;
			}
            if (InputVelocity.Length > 0) {
				var movement = move.Velocity.Dot( InputVelocity.Normal );
				move.Velocity = move.Velocity - movement * InputVelocity.Normal;
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
        if (GroundEntity != null && move.HitWall) {
            // TODO
        }

		Position = move.Position;
		Velocity = move.Velocity;
	}

    public virtual void TryPathOffNav()
    {
        if (target is player.FearfulCryPlayer) {
			var tr = Trace.Ray( EyePosition, target.EyePosition )
				.WorldOnly()
				.WithAnyTags( "player", "solid" )
				.UseHitboxes()
				.Run();

			InputVelocity = (target.Position - Position).WithZ( 0 ).Normal;

            if (tr.Entity == target) {
				TimeSinceSeenTarget = 0;
			}

            // Try getting back on the navmesh if we lost the player.
            if (TimeSinceSeenTarget > 5) {
				var pos = NavMesh.GetClosestPoint( Position );
                if (pos != null)
					InputVelocity = ((Vector3)pos - Position).WithZ( 0 ).Normal;
			}

			Velocity = Velocity.AddClamped( InputVelocity * Time.Delta * 2000, Speed );

            //!TODO debug
		}
    }

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );

        if (info.Attacker is Player attacker) {
            // attacker.DidDamage()

            if (Health <= 0) {
				info.Attacker.Client.AddInt( "kills" );
			}
        }
	}
}