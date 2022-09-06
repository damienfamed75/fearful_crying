using Sandbox;
using SandboxEditor;

[Library("weapon_pistol"), HammerEntity]
[Title("Pistol"), Category("Weapon"), Icon("place")]
[EditorModel("weapons/rust_pistol/rust_pistol.vmdl")]
public partial class Pistol : AmmoWeapon
{
	// Shooting information
	protected static float Spread => 0.02f;
	protected static float Force => 1.0f;
	protected static float BulletDamage => 10.0f;
	protected static float BulletSize => 3.0f;

	// First person viewmodel
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	// Sounds
	private static string DryFireSound => "revolver-dryfire";
	private static string FireSound => "rust_pistol.shoot";

	// Firing rates
	public override float PrimaryRate => 6.5f;
	public override float SecondaryRate => 1f;
	public TimeSince TimeSinceDischarge { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		MagSize = 13;
		TotalBulletCount = 54;
		CurrentBulletCount = MagSize;

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed( InputButton.PrimaryAttack );
	}

	public override void AttackPrimary()
	{
		// If there's no ammo left in the magazine, then play blind fire sound.
		if (!HasAmmo) {
			PlaySound( DryFireSound );
			return;
		}
		base.AttackPrimary();

		// reset time since attack
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		// Player animation for pistol attack.
		if (Owner is AnimatedEntity at) {
			at.SetAnimParameter( "b_attack", true );
		}

		ShootEffects();
		PlaySound( FireSound );
		ShootBullet( Spread, Force, BulletDamage, BulletSize );
	}

	/// <summary>
	/// Discharge is used for when this weapon is a physics object and encounters
	/// a force great enough to discharge it.
	/// </summary>
	private void Discharge()
	{
		if (!HasAmmo)
			return;

		TimeSinceDischarge = 0;

		// Get the muzzle location on the pistol model.
		var muzzle = GetAttachment( "muzzle" ).GetValueOrDefault();
		var pos = muzzle.Position;
		var rot = muzzle.Rotation;

		ShootEffects();
		PlaySound( FireSound );
		ShootBullet( pos, rot.Forward, Spread, Force, BulletDamage, BulletSize );

		// Apply impulse backward on the weapon.
		ApplyAbsoluteImpulse( rot.Backward * 200f );
	}

	/// <summary>
	/// Used for discharging the weapon when there's a great enough force.
	/// </summary>
	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		// If colliding with a player then ignore this.
		// This is it to prevent the weapon from discharging immediately out
		// of the hands of the player.
		if (eventData.Other.Entity is Player)
			return;

		// If the weapon comes into a collision above a speed, then discharge the weapon.
		if (eventData.Speed > 500f) {
			Discharge();
		}
	}
}
