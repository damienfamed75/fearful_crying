using Sandbox;
using System;
using System.Linq;

partial class Inventory : BaseInventory
{
	public Inventory(Player player) : base(player)
	{
	}

	public override bool CanAdd( Entity ent )
	{
		// if the entity is invalid, return false.
		if ( !ent.IsValid())
			return false;

		// If the base cannot add the item, return false.
		if ( !base.CanAdd( ent ) )
			return false;

		// return if carrying type that's the same.
		return !IsCarryingType( ent.GetType() );
	}

	public override bool Add( Entity ent, bool makeActive = false )
	{
		// if the entity is invalid, return false.
		if ( !ent.IsValid() )
			return false;

		// if carrying the same kind of item, return false.
		if ( IsCarryingType( ent.GetType() ) )
			return false;

		return base.Add( ent, makeActive );
	}

	public bool IsCarryingType(Type t)
	{
		return List.Any( x => x?.GetType() == t );
	}

	public override bool Drop( Entity ent )
	{
		
		if ( !Host.IsServer )
			return false;
		// If the player doesn't even contain this item in their inventory, return false.
		if ( !Contains( ent ) )
			return false;

		if (ent is BaseCarriable bc) {
			bc.OnCarryDrop( Owner );
		}

		return ent.Parent == null;
	}
}
