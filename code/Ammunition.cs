using System.Security;
using FearfulCry.player;
using Sandbox;

public enum AmmoType
{
	None,
	Pistol,
	Shotgun
}

public partial class Ammunition : ModelEntity, IUse
{
    // Override all of these in ammunition definition.
    protected virtual int AmmoAmount { get; set; }
    protected virtual string ModelPath { get; set; }
    protected virtual string SoundPath { get; set; }
	protected virtual AmmoType AmmoType { get; set; }

	public PickupTrigger PickupTrigger { get; protected set; }
	
    public override void Spawn()
    {
		base.Spawn();
        // Create the pickup trigger.
		PickupTrigger = new PickupTrigger
		{
			Parent = this,
			Position = Position,
			EnableTouch = true,
			EnableSelfCollisions = false,
		};
        // Set the physics body to never sleep.
		PickupTrigger.PhysicsBody.AutoSleep = false;

		SetModel( ModelPath );
	}

	public bool IsUsable( Entity user )
	{
		if (Owner != null)
			return false;

        if (!user.IsValid())
			return false;

		user.StartTouch( this );

		return true;
	}

	public override void Touch( Entity other )
	{
		base.Touch( other );

        if (Owner != null)
			return;

        if (other is Player) {
		    OnUse( other );
        }
	}

	public bool OnUse( Entity user )
	{
		var player = user as FearfulCryPlayer;

        if (Owner != null)
			return false;

		// If the player doesn't have full ammunition of this type then
		// attempt to give the player ammo.
		if (player.AmmoCount(AmmoType) != FearfulCryPlayer.MaxAmmo(AmmoType)) {
			var taken = player.GiveAmmo( AmmoType, AmmoAmount );
			PlaySound( SoundPath );

			// Get the remaining amount of ammo left in this entity.
			var remaining = AmmoAmount - taken;
			AmmoAmount = remaining;

			// If there's nothing remaining then delete.
			if (remaining == 0) {
				Delete();
			}
		}

		return false;
	}
}