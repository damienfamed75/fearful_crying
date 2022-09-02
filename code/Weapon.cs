using Sandbox;
using System.Collections.Generic;

public partial class Weapon : BaseWeapon, IUse
{
	public virtual float ReloadTime => 3.0f;

	public PickupTrigger PickupTrigger { get; protected set; }

	[Net, Predicted]
	public TimeSince TimeSinceReload { get; set; }

	[Net, Predicted]
	public bool IsReloading { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceDeployed { get; set; }

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

	// When a player equipped the item.
	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}

	public override void Reload()
	{
		// Don't reload if already reloading.
		if ( IsReloading )
			return;

		TimeSinceReload = 0;
		IsReloading = true;

		(Owner as AnimatedEntity)?.SetAnimParameter( "b_reload", true );

		// Reload Animation.
		StartReloadEffects();
	}

	public override void Simulate( Client player )
	{
		if ( TimeSinceDeployed < 0.6f )
			return;

		if (!IsReloading ) {
			base.Simulate( player );
		}

		if (IsReloading && TimeSinceDeployed > ReloadTime) {
			OnReloadFinish();
		}
	}

	public virtual void OnReloadFinish()
	{
		IsReloading = false;
	}

	[ClientRpc]
	public virtual void StartReloadEffects()
	{
		ViewModelEntity?.SetAnimParameter( "reload", true );

		//! TODO third person camera model reload.
	}

	public override void CreateViewModel()
	{
		Host.AssertClient();

		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};

		ViewModelEntity.SetModel( ViewModelPath );
	}

	[ClientRpc]
	protected virtual void ShootEffects()
	{
		// Assert that this is being called by a client.
		Host.AssertClient();

		Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );

		ViewModelEntity?.SetAnimParameter( "fire", true );
	}

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
			yield return tr;
		}
	}

	public void Remove()
	{
		Delete();
	}
}
