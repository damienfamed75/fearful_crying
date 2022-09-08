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
		var player = user as Player;

        if (Owner != null)
			return false;

		for ( int i = 0; i < player.Inventory.Count(); i++) {
			var ent = player.Inventory.GetSlot( i );
            // If this entity is not an ammo weapon then continue to the next
			// inventory slot.
            if (ent is not AmmoWeapon aw)
				continue;

			// Check if ammo types match.
			if (aw.AmmoType == AmmoType) {
				aw.TotalBulletCount += AmmoAmount;
				PlaySound( SoundPath );
				Delete();
			}
		}

        return false;
	}
}