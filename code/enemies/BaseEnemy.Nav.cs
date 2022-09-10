using System;
using Sandbox;
using System.Linq;

namespace FearfulCry.Enemies;

public enum EnemyState
{
    None,
	Wander,
	Chase,
}

public partial class BaseEnemy
{
    public void StartWander()
    {
		EnemyState = EnemyState.Wander;
		Speed = WalkSpeed;
		Steer = CreateWander();
	}

    protected virtual Nav.Wander CreateWander()
    {
		return new Nav.Wander {
			MinRadius = 150,
			MaxRadius = 300
		};
	}

	private void Wander()
	{
 		if (JustSpawned) {
			(Steer as Nav.Wander).FindNewTarget( Position );
			JustSpawned = false;
			Log.Info( $"finding new target {target}" );
		}
		// 10% of the time, the zombie will be checking for players around them.
		if (Rand.Int(10) == 1) {
			// Find player within the sphere of vision.
			var playerTarget = Entity
				.FindInSphere( Position, ChaseVisionRadius )
				.OfType<Player>()
				.FirstOrDefault();

			// If player is valid then chase them.
			if (playerTarget.IsValid()) {
				StartChase(playerTarget);
			}
		}
	}

    public void FindTarget()
    {
		target = All
			.OfType<Player>()
			.OrderBy( x => Guid.NewGuid() ) // Order randomly
			.FirstOrDefault();

        if (target == null)
			Log.Warning( $"couldn't find target for {this}" );
    }

    public void StartChase()
    {
        if (target == null)
			FindTarget();

        StartChase( target );
	}

    public void StartChase(Entity targ)
    {
		target = targ;
        if (!target.IsValid()) {
			Log.Warning( $"invalid target[{target}]" );
			return;
		}
        // If enemy state is already chasing then do nothing.
        if (EnemyState == EnemyState.Chase)
			return;

		SetAnimParameter( "b_jump", true ); //!TODO make anim param configurable

		EnemyState = EnemyState.Chase;
		Speed = RunSpeed;
		Steer = new NavSteer {
			Target = target.Position
		};
        // If this enemy can try to alert others nearby
        if (CanAlertNearby) {
			TryAlertNearby( target, NearbyAlertChance, NearbyAlertRadius );
		}
	}

	private void Chase()
	{
		if (Steer == null) {
			if (!target.IsValid()) {
				Log.Warning("invalid target and steer");
				FindTarget();
			}
			// if (target.LifeState == LifeState.Dead) FindTarget();
			if (target.LifeState == LifeState.Dead) {
				Log.Warning("lifestate => dead");
				StartWander();
			}
			Steer = new NavSteer {
				Target = target.Position
			};
		}
		if (target.IsValid()) {
			// Don't do anything while stunned.
			if (TimeUntilUnstunned < 0) {
				var distanceToTarget = (Position - Steer.Target).Length;

				if (distanceToTarget < 100) {
					if (target.LifeState == LifeState.Dead)
						FindTarget();

					Steer.Target = target.Position;
				} else if (Rand.Int(10) == 1) {
					if (target.LifeState == LifeState.Dead)
						FindTarget();

					Steer = new NavSteer {
						Target = target.Position
					};
				} else if (distanceToTarget > MaxVisionRadius ) {
					StartWander();
				}


				// Check if we're on the navmesh.
				Vector3 pNearestPosOut = Vector3.Zero;
				NavArea closestNav = NavArea.GetClosestNav(
					Position,
					NavAgentHull.Default,
					GetNavAreaFlags.NoFlags,
					ref pNearestPosOut,
					200, 600, 70, 16
				);

				if (!closestNav.Valid) {
					Steer = null;
					TryPathOffNav();
				} else if (Steer.Output.Finished && (Position - target.Position).Length > 70) {
					Steer = null;
					TryPathOffNav();
				}

				//! TODO - scale attack speed with difficulty/num of players
				if (TimeSinceAttacked > AttackSpeed && TimeUntilUnstunned < 0) {
					var range = AttackRange; // 60f
					if ( (Position - target.Position).Length < range || (EyePosition - target.Position).Length < range ) {
						TimeSinceAttacked = -3;
						TryAttack();
						// TryMeleeAttack();
					}
				}
			}
		} else {
			//! ------------------------------------------------------------
			//!
			//! TODO - turn back to wandering if target is invalid for x time.
			//!
			//! ------------------------------------------------------------
			FindTarget();
		}
		if (target.LifeState == LifeState.Dead) {
			StartWander();
		}
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
}