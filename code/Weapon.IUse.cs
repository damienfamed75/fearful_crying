using Sandbox;

public partial class Weapon : BaseWeapon, IUse
{
	/// <summary>
	/// OnUse is the interaction between the player and this weapon.
	/// </summary>
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

	/// <summary>
	/// Returns if this weapon is usable.
	/// </summary>
	public bool IsUsable( Entity user )
	{
		// Cast the entity as a sandbox player.
		var player = user as Player;
		// If someone already owns this weapon, then return false.
		if ( Owner != null )
			return false;

		// Try to add the usable item.
		if ( player.Inventory is Inventory inventory )
			return inventory.CanAdd( this );

		return true;
	}
}
