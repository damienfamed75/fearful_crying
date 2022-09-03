using Sandbox;
using FearfulCry.Enemies.Nav;
using System.Runtime;
using System.Linq;
using System.Runtime.CompilerServices;
using System;
using SandboxEditor;

namespace FearfulCry.Enemies;

[Library("enemy_commonzombie"), HammerEntity]
[Title("CommonZombie"), Category("Enemy")]
[EditorModel("models/citizen/citizen.vmdl")]
public partial class CommonZombie : BaseZombie
{
	public bool JustSpawned = true;

	public override void Spawn()
	{
		base.Spawn();

		Health = 50;

		StartWander();
	}

    public override void Tick()
    {
		base.Tick();

		if (ZombieState == ZombieState.Wander) {
            if (JustSpawned) {
				(Steer as Wander).FindNewTarget( Position );
				JustSpawned = false;
				Log.Info( $"finding new target {target}" );
			}

            // if (Steer == null) {
			// 	StartWander();
			// }
            // if (Steer.Path.IsEmpty && T)

        } else if (ZombieState == ZombieState.Chase) {
            if (Steer == null) {
                if (!target.IsValid()) FindTarget();
                if (target.LifeState == LifeState.Dead) FindTarget();
				Steer = new NavSteer();
				Steer.Target = target.Position;
			}
            if (target.IsValid()) {
                // Don't do anything while stunned.
                if (TimeUntilUnstunned < 0) {
					var distanceToTarget = (Position - Steer.Target).Length;

                    if (distanceToTarget < 100) {
                        if (!target.IsValid()) FindTarget();
                        if (target.LifeState == LifeState.Dead) FindTarget();

						Steer.Target = target.Position;
					} else if (Rand.Int(10) == 1) {
                        if (!target.IsValid()) FindTarget();
                        if (target.LifeState == LifeState.Dead) FindTarget();

						Steer = new NavSteer();
						Steer.Target = target.Position;
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


				}
            }
        }
    }

    public void StartWander()
    {
		ZombieState = ZombieState.Wander;
		Speed = WalkSpeed;

		var wander = new Nav.Wander();
		wander.MinRadius = 150;
		wander.MaxRadius = 300;
		Steer = wander;
	}

    public void FindTarget()
    {
		target = Entity.All
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
        if (target == null) {
			Log.Warning( $"invalid target for {this}" );
			return;
		}
        // If there's a conflicting state already active, then return.
        if (ZombieState == ZombieState.Chase || ZombieState == ZombieState.Lure || ZombieState == ZombieState.Burning)
			return;

		// SetAnimParameter("")

		ZombieState = ZombieState.Chase;
		Speed = RunSpeed;

		Steer = new NavSteer();
		Steer.Target = target.Position;

        // Try to alert nearby enemies.
        //! TODO
	}
}