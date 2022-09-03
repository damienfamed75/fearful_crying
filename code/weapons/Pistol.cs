using Sandbox;
using SandboxEditor;

[Library("weapon_pistol"), HammerEntity]
[Title("Pistol"), Category("Weapon"), Icon("place")]
[EditorModel("weapons/rust_pistol/rust_pistol.vmdl")]
public partial class Pistol : Weapon
{
	// shooting information.
	protected float spread => 0.02f;
	protected float force => 1.0f;
	protected float damage => 10.0f;
	protected float bulletSize => 3.0f;

	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

	// Firing rates
	public override float PrimaryRate => 15f;
	public override float SecondaryRate => 1f;

	public TimeSince TimeSinceDischarge { get; set; }

	public override void Spawn()
	{
		base.Spawn();

		SetModel( "weapons/rust_pistol/rust_pistol.vmdl" );
	}

	public override bool CanPrimaryAttack()
	{
		return base.CanPrimaryAttack() && Input.Pressed(InputButton.PrimaryAttack);
	}

	public override void AttackPrimary()
	{
		// reset time since attack
		TimeSincePrimaryAttack = 0;
		TimeSinceSecondaryAttack = 0;

		// Player animation for pistol attack.
		if (Owner is AnimatedEntity at) {
			at.SetAnimParameter( "b_attack", true );
		}

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		ShootBullet( spread, force, damage, bulletSize );
	}

	private void Discharge()
	{
		if ( TimeSinceDischarge < 0.5f)
			return;

		TimeSinceDischarge = 0;

		var muzzle = GetAttachment( "muzzle" ) ?? default;
		var pos = muzzle.Position;
		var rot = muzzle.Rotation;

		ShootEffects();
		PlaySound("rust_pistol.shoot");
		ShootBullet( pos, rot.Forward, spread, force, damage, bulletSize );

		// Apply impulse backward on the weapon.
		ApplyAbsoluteImpulse( rot.Backward * 200f );
	}

	protected override void OnPhysicsCollision( CollisionEventData eventData )
	{
		// If the weapon comes into a collision above a speed, then discharge the weapon.
		if (eventData.Speed > 500f)
		{
			Discharge();
		}
	}
}
