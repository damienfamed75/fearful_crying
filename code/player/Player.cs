using System;
using Sandbox;

namespace FearfulCry.player;

partial class FearfulCryPlayer : Player
{
	private TimeSince timeSinceDropped;
	private TimeSince timeSinceJumpReleased;

	[Net, Predicted]
	public TimeSince TimeSinceDamage { get; set; }
	private DamageInfo lastDamage;

	public int MaxHealth { get; protected set; } = 100;

	/// <summary>
	/// The clothing container is what dresses the citizen.
	/// </summary>
	public ClothingContainer Clothing = new();

	/// <summary>
	/// Default init
	/// </summary>
	public FearfulCryPlayer()
	{
		Inventory = new Inventory( this );
	}

	/// <summary>
	/// Initialize using this client.
	/// </summary>
	public FearfulCryPlayer( Client cli ) : this()
	{
		// Load clothing from the client data.
		Clothing.LoadFromClient( cli );
	}

	public override PawnController GetActiveController()
	{
		if (DevController != null) {
			return DevController;
		}

		return base.GetActiveController();
	}

	public override void OnKilled()
	{
		base.OnKilled();
		// For certain types of damage, do different things.
		if (lastDamage.Flags.HasFlag(DamageFlags.Blunt))
		{
			Particles.Create( "particles/impact.flesh-big.vpcf", lastDamage.Position );
			PlaySound( "kersplat" );
		}
		// Become a ragdoll.
		BecomeRagdollOnClient(
			Velocity,
			lastDamage.Flags,
			lastDamage.Position,
			lastDamage.Force,
			GetHitboxBone( lastDamage.HitboxIndex )
		);

		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;
		CameraMode = new SpectateRagdollCamera();

		foreach (var child in Children) {
			child.EnableDrawing = false;
		}
		// Drop the currently held item and delete the rest of the items.
		Inventory.DropActive();
		Inventory.DeleteContents();
	}

	public override void TakeDamage( DamageInfo info )
	{
		TimeSinceDamage = 0;
		// If the player was hit in the head (which is HitboxGroup 1)
		// then double the damage.
		if (GetHitboxGroup(info.HitboxIndex) == 1) {
			info.Damage *= 2.0f;
		}
		lastDamage = info;

		TookDamage( lastDamage.Flags, lastDamage.Position, lastDamage.Force );

		base.TakeDamage( info );
	}

	public int GiveHealth(int amount)
	{
		var total = amount + Health;
		if (total > MaxHealth)
			total = MaxHealth;

		var taken = total - Health;
		Health = total;

		return (int)taken;
	}

	[ClientRpc]
	public void TookDamage(DamageFlags damageFlags, Vector3 forcePos, Vector3 force)
	{

	}

	public override void Respawn()
	{
		// Set the player model to the citizen model.
		SetModel( "models/citizen/citizen.vmdl" );

		// Basic walking controller.
		Controller = new WalkController();

		if (DevController is NoclipController) {
			DevController = null;
		}

		EnableAllCollisions = true;
		EnableDrawing = true;
		EnableHideInFirstPerson = true;
		EnableShadowInFirstPerson = true;

		// Dress the citizen.
		Clothing.DressEntity( this );

		//
		// Inventory
		//
		ClearAmmo();

		Inventory.Add( new PumpShotgun(), true);
		Inventory.Add( new Pistol());

		CameraMode = new FirstPersonCamera();

		Tags.Add( "player" );

		Health = 100;
		Log.Info( $"Player default health {Health}" );

		base.Respawn();
	}

	// Called every tick, client and server side.
	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if (Input.ActiveChild != null) {
			ActiveChild = Input.ActiveChild;
		}

		if ( LifeState != LifeState.Alive )
			return;

		var controller = GetActiveController();
		if (controller != null) {
			EnableSolidCollisions = !controller.HasTag( "noclip" );

			SimulateAnimation( controller );
		}

		TickPlayerUse();
		SimulateActiveChild( cl, ActiveChild );
		// Check for flashlight related inputs and update the position/rotation
		// of the flashlight if active.
		FlashlightTick();

		if (Input.Pressed(InputButton.Drop)) {
			var dropped = Inventory.DropActive();
			// If the dropped item is not null then apply physics.
			if (dropped != null) {
				// Throw the item forward and upward.
				dropped.PhysicsGroup.ApplyImpulse( Velocity + EyeRotation.Forward * 500f + Vector3.Up * 100f, true );
				// Apply random rotational force.
				dropped.PhysicsGroup.ApplyAngularImpulse( Vector3.Random * 100f, true );
				// Throw the item facing sideways.
				dropped.Rotation = Rotation.FromYaw( 90f ) * EyeRotation;

				timeSinceDropped = 0;
			}
		}

		// Toggle camera mode.
		if (Input.Pressed(InputButton.View)) {
			if (CameraMode is ThirdPersonCamera) {
				CameraMode = new FirstPersonCamera();
			} else {
				CameraMode = new ThirdPersonCamera();
			}
		}

		// Jump when input.
		if (Input.Released(InputButton.Jump)) {
			//!TODO make constants
			if (timeSinceJumpReleased < 0.3f) {
				Game.Current?.DoPlayerNoclip( cl );
			}

			timeSinceJumpReleased = 0;
		}

		// If cardinal directional movement detected.
		if (Input.Left != 0 || Input.Forward != 0) {
			timeSinceJumpReleased = 1;
		}
	}

	Entity lastWeapon;

	// Every tick, animate the citizen according to the input.
	void SimulateAnimation(PawnController controller)
	{
		if ( controller == null )
			return;

		var turnSpeed = .02f;
		var iRot = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );
		Rotation = Rotation.Slerp( Rotation, iRot, controller.WishVelocity.Length * turnSpeed * Time.Delta );
		// Lock animation facing wihtin 45 degrees of rotation true to the player's eye rotation.
		Rotation = Rotation.Clamp( iRot, 45.0f, out var shuffle );

		CitizenAnimationHelper animHelper = new( this );

		animHelper.WithWishVelocity( controller.WishVelocity );
		animHelper.WithVelocity( controller.Velocity );
		animHelper.WithLookAt( EyePosition + EyeRotation.Forward * 100f, 1f, 1f, 0.5f);
		animHelper.AimAngle = Input.Rotation;
		animHelper.FootShuffle = shuffle;
		animHelper.DuckLevel = MathX.Lerp( animHelper.DuckLevel, controller.HasTag( "ducked" ) ? 1 : 0, Time.Delta * 10.0f );
		animHelper.VoiceLevel = (Host.IsClient && Client.IsValid()) ? Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f : 0.0f;
		animHelper.IsGrounded = GroundEntity != null;
		animHelper.IsSitting = controller.HasTag( "sitting" );
		animHelper.IsWeaponLowered = false;
		//!TODO add rest of params


		if (controller.HasEvent("jump")) {
			animHelper.TriggerJump();
		}

		if ( ActiveChild != lastWeapon) {
			animHelper.TriggerDeploy();
		}

		if (ActiveChild is BaseCarriable carry) {
			carry.SimulateAnimator( animHelper );
		} else {
			animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
			animHelper.AimBodyWeight = 0.5f;
		}

		lastWeapon = ActiveChild;
	}

	public override void StartTouch( Entity other )
	{
		if ( timeSinceDropped < 1 )
			return;

		base.StartTouch( other );
	}

	public override float FootstepVolume()
	{
		return Velocity.WithZ( 0 ).Length.LerpInverse( 0f, 200f ) * 5f;
	}
}

