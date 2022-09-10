using System.Collections.Generic;
using System.Linq;
using Sandbox;
using SandboxEditor;

namespace FearfulCry.Enemies;

[Library("enemy_commonundead"), HammerEntity]
[Title("CommonUndead"), Category("Enemy"), Description("A common undead")]
[EditorModel("models/citizen/citizen.vmdl")]
public partial class CommonUndead : BaseEnemy
{
    // Movement
	public override BBox BoundingBox => BBox.FromHeightAndRadius( 52, 12 );
	public override float WalkSpeed => Rand.Float( 40, 50 );
	public override float RunSpeed => Rand.Float( 150, 170 );
	public override float StepSize => 20f;

    // Attack
	public override float AttackSpeed { get; set; } = 1.0f;
	public override float AttackDamage { get; set; } = 15.0f;
	public override float AttackRange => 70f;

	public override string ModelPath => "models/citizen/citizen.vmdl";
	public override int MaxHealth => 50;

	public override float MaxVisionRadius => 700f;
	public override float ChaseVisionRadius => 200f;

	public override void Spawn()
    {
		base.Spawn();
        // Set the eye position of the undead.
		EyePosition = Position + Vector3.Up * 60;
		// Create physics capsule.
        SetupPhysicsFromCapsule(
            PhysicsMotionType.Keyframed,
            Capsule.FromHeightAndRadius( 72, 8 )
        );
        // Dress the undead since they're using the citizen model.
		UpdateClothes();
		Dress(Color.Parse("#8cab7e"), Color.Parse("#A3A3A3"));

		StartWander();

		Tags.Add( "undead" );
	}

    public override void Tick()
    {
		base.Tick();
        // Debug draw the bounding box
		// DebugOverlay.Box( Position + BoundingBox.Mins, Position + BoundingBox.Maxs, Color.Cyan );
	}

    public override void TryAttack()
    {
		if (TimeUntilUnstunned > 0)
			return;

		PlaySoundOnClient( "rust_flashlight.attack" );
		SetAnimParameter( "b_jump", true );
		MeleeAttack();
		Velocity *= .1f;
    }

	public void MeleeAttack(Vector3 rotForward = default)
	{
		Rand.SetSeed( Time.Tick );

		if (!IsServer || !IsValid || TimeUntilUnstunned > 0)
			return;

		Velocity = 0;
		TimeSinceAttacked = 0 - Rand.Float(1); // Some variation in attacks.

		if (rotForward == default)
			rotForward = Rotation.Forward;

		var forward = rotForward;
		// var forward = Rotation.Forward;
		forward += (Vector3.Random + Vector3.Random + Vector3.Random) * .1f;
		forward = forward.Normal;

		foreach (var tr in TraceMelee(EyePosition, EyePosition+forward * AttackRange, 50)) {
			// Create a bullet impact where the attack was made.
			tr.Surface.DoBulletImpact( tr );
			tr.Entity.ApplyLocalImpulse( forward * 5 );

			if (!IsServer || !tr.Entity.IsValid)
				continue;

			Log.Info( $"hit ent[{tr.Entity}]" );
			// Create the damage information.
			var damageInfo = DamageInfo.FromBullet( tr.EndPosition, forward * 100, AttackDamage )
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
			.WithoutTags( "zombie", "trigger" )
			.EntitiesOnly()
			.Ignore( this )
			// .WithTag( "player" ) // If you remove this, zombies hit the air...?
			.Size( radius )
			.Run();

		if (tr.Hit)
			yield return tr;
	}
}