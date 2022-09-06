using Sandbox;
using FearfulCry.Enemies.Nav;
using System.Linq;
using System;
using SandboxEditor;
using System.Collections.Generic;

namespace FearfulCry.Enemies;

[Library("enemy_commonzombie"), HammerEntity]
[Title("CommonZombie"), Category("Enemy")]
[EditorModel("models/citizen/citizen.vmdl")]
public partial class CommonZombie : BaseZombie
{
	[ConCmd.Server("zombie_forceagro")]
	public static void ForceAgro()
	{
		foreach(var npc in Entity.All.OfType<CommonZombie>().ToArray()) {
			npc.StartChase();
		}
	}

	[ConCmd.Server("zombie_forcewander")]
	public static void ForceWander()
	{
		foreach(var npc in Entity.All.OfType<CommonZombie>().ToArray()) {
			npc.StartWander();
		}
	}

	public bool JustSpawned = true;
	protected static float DefaultZombieHealth => 50f;
	protected static float VisionRadius => 200f; //! todo - raycast vision.
	protected static float MaxVisionRadius => 1200f;
	protected static float AlertChance => 0.1f;
	protected static float AlertRadius => 800f;
	protected static float AttackDistance => 70f;

	public override void Spawn()
	{
		base.Spawn();
		// Set default health to this common zombie.
		Health = DefaultZombieHealth;
		// Wander default
		StartWander();
	}

    public override void Tick()
    {
		base.Tick();

		switch (ZombieState) {
			case ZombieState.Wander:
				Wander();
				break;
			case ZombieState.Chase:
				Chase();
				break;
		}
    }

    public void StartWander()
    {
		ZombieState = ZombieState.Wander;
		Speed = WalkSpeed;

		var wander = new Nav.Wander {
			MinRadius = 150,
			MaxRadius = 300
		};

		Steer = wander;
	}

	private void Wander()
	{
 		if (JustSpawned) {
			(Steer as Wander).FindNewTarget( Position );
			JustSpawned = false;
			Log.Info( $"finding new target {target}" );
		}
		// 10% of the time, the zombie will be checking for players around them.
		if (Rand.Int(10) == 1) {
			// Find player within the sphere of vision.
			var playerTarget = Entity
				.FindInSphere( Position, VisionRadius )
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
        if (ZombieState == ZombieState.Chase)
			return;

		// SetAnimParameter("")

		ZombieState = ZombieState.Chase;
		Speed = RunSpeed;

		Steer = new NavSteer {
			Target = target.Position
		};

		// Try to alert nearby enemies.
		TryAlertNearby(target, AlertChance, AlertRadius );
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
					var range = 60;
					if ( (Position - target.Position).Length < range || (EyePosition - target.Position).Length < range ) {
						// TryMeleeAttack();
						TimeSinceAttacked = -3;
						TryMeleeAttack();
						MeleeAttack();
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
			Log.Warning( "lifestate => dead (indv check)" );
			StartWander();
		}
	}

	public override void HitBreakableObject()
	{
		if (TimeSinceAttacked > AttackSpeed) {
			TimeSinceAttacked = -3;
			TryMeleeAttack();
		}
	}

	public override void TakeDamage( DamageInfo info )
	{
		base.TakeDamage( info );
		// If the zombie is not already chasing and the attacker is a player
		// then begin to chase them.
		if (ZombieState != ZombieState.Chase && info.Attacker is Player player) {
			StartChase( player );
		}
	}

	public void TryMeleeAttack()
	{
		if (TimeUntilUnstunned > 0)
			return;

		// PlaySoundOnClient( "sounds/weapons/extra-gunshot-a.vsnd_c" );
		SetAnimParameter( "b_attack", true );
		Velocity *= .1f;
	}

	public void MeleeAttack()
	{
		Rand.SetSeed( Time.Tick );

		if (!IsServer || !IsValid || TimeUntilUnstunned > 0)
			return;

		Velocity = 0;
		TimeSinceAttacked = 0 - Rand.Float(1); // Some variation in attacks.

		var forward = Rotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random) * .1f;
		forward = forward.Normal;

		foreach (var tr in TraceMelee(EyePosition, EyePosition+forward * AttackDistance, 50)) {
			// Create a bullet impact where the attack was made.
			tr.Surface.DoBulletImpact( tr );
			Log.Info($"Impact {tr.Surface}");
			tr.Entity.ApplyLocalImpulse( forward * 20 );

			if (!IsServer || !tr.Entity.IsValid)
				continue;
			// Create the damage information.
			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 300, AttackDamage )
				.UsingTraceResult( tr )
				.WithAttacker( this )
				.WithWeapon( this );
			// Apply damage to the target entity.
			tr.Entity.TakeDamage( damageInfo );
		}
	}

	/// <summary>
	/// The raycast to detect whether the zombie melee hits something or not.
	/// </summary>
	public virtual IEnumerable<TraceResult> TraceMelee(Vector3 from, Vector3 to, float radius = 2.0f)
	{
		var tr = Trace.Ray( from, to )
			.Ignore( Owner )
			.WithoutTags( "zombie" )
			.EntitiesOnly()
			.Ignore( this )
			.WithTag( "player" ) // If you remove this, zombies hit the air...?
			.Size( radius )
			.Run();
		
		if (tr.Hit)
			yield return tr;
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
		foreach (CommonZombie zombie in Entity.FindInSphere(Position, radius).OfType<CommonZombie>()) {
			var chance = percent; //! todo - decrease chance by distance.
			zombie.TryAlert( target, chance );
		}
	}
}