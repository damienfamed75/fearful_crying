using Sandbox;
using System.Collections.Generic;

public partial class Weapon : BaseWeapon, IUse
{
	// the amount of time before you can use this weapon after deploying it.
	protected float _deployTime => 0.6f;
	// the amount of time it takes to reload the weapon.
	public virtual float ReloadTime => 3.0f;
	// The pickup trigger volume for this weapon.
	public PickupTrigger PickupTrigger { get; protected set; }

	// Net, Predicted means the client can dictate the value of this variable within Simulate,
	// but if the server value is mismatched then it will be corrected on the client side.
	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; } // Time since reload beginning.

	// Net, Predicted means the client can dictate the value of this variable within Simulate,
	// but if the server value is mismatched then it will be corrected on the client side.
	[Net, Predicted]
	public bool IsReloading { get; set; }

	// Net, Predicted means the client can dictate the value of this variable within Simulate,
	// but if the server value is mismatched then it will be corrected on the client side.
	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; } // Time since weapon equipped.

	public override void Spawn()
	{
		base.Spawn();
		// Create a new pickup trigger.
		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false,
		};
		// Set the physics body to never sleep.
		PickupTrigger.PhysicsBody.AutoSleep = false;
	}

	public override void Reload()
	{
		// Don't reload if already reloading.
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		if (Owner is AnimatedEntity et) {
			et.SetAnimParameter( "b_reload", true );
		}

		// Reload Animation.
		StartReloadEffects();
	}

	/// <summary>
	/// Simulate gets called every tick and 
	/// </summary>
	public override void Simulate( Client player )
	{
		if ( TimeSinceDeployed < _deployTime )
			return;

		if (!IsReloading ) {
			// Normal weapon functionality is handled by the base.
			base.Simulate( player );
		}

		if (IsReloading && TimeSinceDeployed > ReloadTime) {
			OnReloadFinish();
		}
	}

	/// <summary>
	/// OnReloadFinish sets the status that this weapon is not reloading anymore.
	/// </summary>
	public virtual void OnReloadFinish()
	{
		IsReloading = false;
	}

	/// <summary>
	/// StartReloadEffects plays the reload animation on the weapon.
	/// </summary>
	[ClientRpc] // Can be called by server.
	public virtual void StartReloadEffects()
	{
		if (ViewModelEntity != null) {
			ViewModelEntity.SetAnimParameter( "reload", true );
		}

		//! TODO third person camera model reload.
	}

	/// <summary>
	/// ShootEffects spawns particles at the weapon and plays the fire animation.
	/// </summary>
	[ClientRpc] // Can be called by server.
	protected virtual void ShootEffects()
	{
		// Assert that this is being called by a client.
		Host.AssertClient();
		// Create particles in front fo the weapon (works for first & third person views.)
		//! TODO the particle is only using the pistol muzzleflash.
		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
		// Playback the fire animation on the weapon.
		if (ViewModelEntity != null) {
			ViewModelEntity.SetAnimParameter( "fire", true );
		}
	}

	/// <summary>
	/// Remove deletes this entity.
	/// </summary>
	public void Remove()
	{
		Delete();
	}

	/// <summary>
	/// TraceBullet will trace for any collisions with this bullet with a start
	/// location, end location, and a radius.
	/// 
	/// This method returns multiple responses for cases when the bullet will
	/// go through glass and hit another object, etc.
	/// </summary>
	public override IEnumerable<TraceResult> TraceBullet( Vector3 start, Vector3 end, float radius = 2 )
	{
		bool underWater = Trace.TestPoint( start, "water" );

		var trace = Trace.Ray( start, end )
			.UseHitboxes()
			.WithAnyTags( "solid", "player", "npc", "glass" )
			.Ignore( this )
			.Size( radius );

		// If we're not underwater then we can hit water.
		if (!underWater) {
			trace = trace.WithAnyTags( "water" );
		}

		var tr = trace.Run();
		// If we hit something.
		if (tr.Hit) {
			yield return tr; // Return each trace result once at a time.
		}
	}
}
