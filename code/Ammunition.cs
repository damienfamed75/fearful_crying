using Sandbox;

public partial class Ammunition : ModelEntity, IUse
{
    // Override all of these in ammunition definition.
    protected virtual System.Type WeaponType { get; set; }
    protected virtual int AmmoAmount { get; set; }
    protected virtual string ModelPath { get; set; }
    protected virtual string SoundPath { get; set; }

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
            // If the matching weapon type is found in the inventory
            // then add to the total bullet count.
            if (ent.GetType() == WeaponType) {
				(ent as AmmoWeapon).TotalBulletCount += AmmoAmount;
				PlaySound(SoundPath);
				Delete();
			}
		}

        return false;
	}
}