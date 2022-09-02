using Sandbox;

public partial class Weapon : BaseWeapon, IUse
{
	public bool OnUse( Entity user )
	{
		// If someone already owns this then it's unusable.
		if ( Owner != null )
			return false;

		if ( !user.IsValid() )
			return false;

		// Begin interaction.
		user.StartTouch( this );

		return false;
	}

	public bool IsUsable( Entity user )
	{
		// Cast the user as a player object.
		var player = user as Player;
		if ( Owner != null )
			return false;

		// Try to add the usable item.
		if ( player.Inventory is Inventory inventory )
		{
			return inventory.CanAdd( this );
		}

		return true;
	}
}
