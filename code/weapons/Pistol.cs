using Sandbox;


public partial class Pistol : Weapon
{
	public override string ViewModelPath => "weapons/rust_pistol/v_rust_pistol.vmdl";

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
		(Owner as AnimatedEntity)?.SetAnimParameter( "b_attack", true );

		ShootEffects();
		PlaySound( "rust_pistol.shoot" );
		ShootBullet( 0.0f, 1.0f, 10f, 3f ); //!TODO make constants.
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
		ShootBullet( pos, rot.Forward, 0.0f, 1.0f, 10f, 3f ); //!TODO make constants.

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
