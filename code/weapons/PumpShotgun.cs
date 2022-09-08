using Sandbox;
using SandboxEditor;

[Library("weapon_pumpshotgun"), HammerEntity]
[Title("Pump Shotgun"), Category("Weapon"), Icon("local_fire_department")]
[EditorModel("weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl")]
public partial class PumpShotgun : AmmoWeapon
{
    // Shooting information
	protected static float Spread => 0.5f;
	protected static float Force => 4.0f;
	protected static float BulletDamage => 8.0f;
	protected static float BulletSize => 2.0f; // Should this be big or small?

    // First person viewmodel
	public override string ViewModelPath => "weapons/rust_pumpshotgun/v_rust_pumpshotgun.vmdl";
	public override float DeployTime => 2f;

	// Sounds
	private static string DryFireSound => "rust_pumpshotgun-dryfire";
	private static string FireSound => "rust_pumpshotgun.shoot";

	public override float PrimaryRate => 0.9f;
	public override float SecondaryRate => 1f;
    public TimeSince TimeSinceDischarge { get; set; }

	public override float ReloadTime => 0.65f;
	public override bool SingleBulletReloading => true;

	public override void Spawn()
    {
		base.Spawn();

		MagSize = 8;
		TotalBulletCount = MagSize;
		CurrentBulletCount = MagSize;

		SetModel( "weapons/rust_pumpshotgun/rust_pumpshotgun.vmdl" );
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

        if (Owner is AnimatedEntity at) {
			at.SetAnimParameter( "b_attack", true );
		}

		ShootEffects();
		PlaySound( FireSound );
		ShootBullets( 12, Spread, Force, BulletDamage, BulletSize );
	}

	/// <summary>
	/// Discharge is used for when this weapon is a physics object and encounters
	/// a force great enough to discharge it.
	/// </summary>
	private void Discharge()
	{
		if (!HasAmmo || TimeSinceDischarge < 0.5f)
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
		ApplyAbsoluteImpulse( rot.Backward * 400f );
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