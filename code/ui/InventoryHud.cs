using System.Collections.Generic;
using FearfulCry.player;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class InventoryHud : Panel
{
	public Label Items;
	public InventoryHud Current;

	public List<InventoryIcon> Slots = new();
	private int itemCount;

	private TimeSince TimeSinceActive { get; set; }
	private static float FadeAwayTime => 4f;
	private bool IsActive { get; set; }

	public InventoryHud()
    {
		Current = this;
		Items = Add.Label( "", "items" );
		StyleSheet.Load( "/ui/InventoryHud.scss" );
	}

    public override void Tick()
    {
		base.Tick();

		var player = Local.Pawn as Player;
        if (player == null)
            return;
		if (player.Inventory == null)
			return;

		for ( int i = 0; i < Slots.Count; i++) {
			UpdateIcon( player.Inventory.GetSlot( i ), Slots[i], i );
		}

		if ( itemCount != player.Inventory.Count() )
		{
			itemCount = player.Inventory.Count();
			// Delete all of the existing slots.
			for ( int i = 0; i < Slots.Count; i++) {
				Slots[i].Delete();
			}
			Slots.Clear();

			for ( int i = 0; i < itemCount; i++ )
			{
				Slots.Add( new InventoryIcon( i + 1, this ) );
			}

			SetActive();
		}

		if (IsActive && TimeSinceActive > FadeAwayTime ) {
			IsActive = false;

			for ( int i = 0; i < player.Inventory.Count(); i++) {
				Slots[i].SetClass( "fade", true );
			}
		}
	}

	private void SetActive()
	{
		var player = Local.Pawn as Player;
		if (player == null)
			return;

		TimeSinceActive = 0;
		IsActive = true;
		for ( int i = 0; i < player.Inventory.Count(); i++) {
			Slots[i].SetClass( "fade", false );
		}
	}

	private static void UpdateIcon(Entity ent, InventoryIcon inventoryIcon, int i)
	{
		var player = Local.Pawn as Player;
		if (player == null)
			return;

		if (ent == null) {
			inventoryIcon.Clear();
			return;
		}

		var displayInfo = DisplayInfo.For( ent );
		inventoryIcon.TargetEnt = ent;
		inventoryIcon.Label.Text = displayInfo.Name;
		inventoryIcon.SetClass( "active", player.ActiveChild == ent );
	}

	[Event("buildinput")]
	public void ProcessClientInput(InputBuilder input)
	{
		var player = Local.Pawn as Player;
		if (player == null)
			return;

		var inventory = player.Inventory;
		if (inventory == null)
			return;

		if (input.Pressed(InputButton.Slot1)) SetActiveSlot( input, inventory, 0 );
		if (input.Pressed(InputButton.Slot2)) SetActiveSlot( input, inventory, 1 );
		if (input.Pressed(InputButton.Slot3)) SetActiveSlot( input, inventory, 2 );
		if (input.Pressed(InputButton.Slot4)) SetActiveSlot( input, inventory, 3 );
		if (input.Pressed(InputButton.Slot5)) SetActiveSlot( input, inventory, 4 );
		if (input.Pressed(InputButton.Slot6)) SetActiveSlot( input, inventory, 5 );
		if (input.Pressed(InputButton.Slot7)) SetActiveSlot( input, inventory, 6 );
		if (input.Pressed(InputButton.Slot8)) SetActiveSlot( input, inventory, 7 );
		if (input.Pressed(InputButton.Slot9)) SetActiveSlot( input, inventory, 8 );

		var mwheel = input.MouseWheel;
		if (mwheel != 0) SwitchActiveSlot( input, inventory, -mwheel );
	}

	private void SetActiveSlot(InputBuilder input, IBaseInventory inventory, int i)
	{
		var player = Local.Pawn as Player;
		if (player == null)
			return;

		var ent = inventory.GetSlot( i );
		if (player.ActiveChild == ent)
			return;

		if (ent == null)
			return;

		SetActive();
		input.ActiveChild = ent;
	}

	private void SwitchActiveSlot(InputBuilder input, IBaseInventory inventory, int idelta)
	{
		var count = inventory.Count();
		if (count == 0)
			return;

		var slot = inventory.GetActiveSlot();
		var nextSlot = slot + idelta;

		while (nextSlot < 0)
			nextSlot += count;

		while (nextSlot >= count)
			nextSlot -= count;

		Log.Info( $"currActive[{slot}] nextSlot[{nextSlot}] idelta[{idelta}]" );
		SetActiveSlot( input, inventory, nextSlot );
	}
}