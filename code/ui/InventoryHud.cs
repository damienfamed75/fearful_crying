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

	/// <summary>
	/// (InputButton, int) tuple that represents (button, slot number)
	/// 
	/// This is used to loop and check for inputs and switch to the corresponding
	/// inventory slot.
	/// </summary>
	private readonly (InputButton, int)[] slotInputs = new[]{
			(InputButton.Slot1, 0), (InputButton.Slot2, 1), (InputButton.Slot3, 2),
			(InputButton.Slot4, 3), (InputButton.Slot5, 4), (InputButton.Slot6, 5),
			(InputButton.Slot7, 6), (InputButton.Slot8, 7), (InputButton.Slot9, 8)
	};

	public InventoryHud()
    {
		Current = this;
		Items = Add.Label( "", "items" );
		StyleSheet.Load( "/ui/InventoryHud.scss" );
	}

    public override void Tick()
    {
		var player = Local.Pawn as Player;
        if (player == null)
            return;
		if (player.Inventory == null)
			return;

		for ( int i = 0; i < Slots.Count; i++) {
			UpdateIcon( player.Inventory.GetSlot( i ), Slots[i], i );
		}

		// If the item count doesn't match the number of items in the inventory.
		if ( itemCount != player.Inventory.Count() ) {
			itemCount = player.Inventory.Count();
			// Delete all of the existing slots.
			Slots.ForEach( x => x.Delete() );
			// Clear the inventory slots.
			Slots.Clear();
			// Re-add the inventory slots.
			for ( int i = 0; i < itemCount; i++ ) {
				Slots.Add( new InventoryIcon( i + 1, this ) );
			}

			SetActive();
		}

		// If the inventory is active and the TimeSinceActive is greater than
		// the fade away time then fade out all of the slot icons.
		if (IsActive && TimeSinceActive > FadeAwayTime ) {
			IsActive = false;
			// Loop through the inventory slots and and fade them out.
			for ( int i = 0; i < player.Inventory.Count(); i++) {
				Slots[i].SetClass( "fade", true );
			}
		}
	}

	/// <summary>
	/// Update the icons to ensure they're representative of the items in the
	/// inventory.
	/// </summary>
	private static void UpdateIcon(Entity ent, InventoryIcon inventoryIcon, int i)
	{
		var player = Local.Pawn as Player;
		if (player == null)
			return;
		// If the entity is null then clear the icon out.
		if (ent == null) {
			inventoryIcon.Clear();
			return;
		}
		// Set the icon label and target entity.
		var displayInfo = DisplayInfo.For( ent );
		inventoryIcon.TargetEnt = ent;
		inventoryIcon.Label.Text = displayInfo.Name;
		// Set this item to active if it's currently equipped.
		inventoryIcon.SetClass( "active", player.ActiveChild == ent );
	}

	/// <summary>
	/// Called when client makes any inputs.
	/// </summary>
	[Event("buildinput")]
	public void ProcessClientInput(InputBuilder input)
	{
		var player = Local.Pawn as Player;
		if (player == null)
			return;

		var inventory = player.Inventory;
		if (inventory == null)
			return;

		// Loop through all of the slot input buttons and check for inputs.
		foreach (var (button, slotNum) in slotInputs) {
			if (input.Pressed(button)) {
				SetActive();
				SetActiveSlot( input, inventory, slotNum );
			}
		}

		// Save the mousewheel as a variable because for some reason input.MouseWheel
		// changes value from the condition to the statement.
		var mwheel = input.MouseWheel;
		if (mwheel != 0) {
			SetActive();
			SwitchActiveSlot( input, inventory, -mwheel );
		}
	}

	private void SetActive()
	{
		var player = Local.Pawn as Player;
		if (player == null)
			return;
		// Set the inventory hud to active
		TimeSinceActive = 0;
		IsActive = true;
		// "Unfade" all of the inventory slot icons.
		for ( int i = 0; i < player.Inventory.Count(); i++) {
			Slots[i].SetClass( "fade", false );
		}
	}

	/// <summary>
	/// SetActiveSlot swaps the active slot to the provided slot number.
	///
	/// If the provided index is already equipped then nothing happens.
	/// </summary>
	private static void SetActiveSlot(InputBuilder input, IBaseInventory inventory, int i)
	{
		var player = Local.Pawn as Player;
		if (player == null)
			return;
		// Get the item at the wanted slot.
		var ent = inventory.GetSlot( i );
		// If the current active child is the same then return.
		if (player.ActiveChild == ent)
			return;
		// If the entity is null then return.
		if (ent == null)
			return;
		// Swap the active item.
		input.ActiveChild = ent;
	}

	/// <summary>
	/// SwitchActiveSlot change active slot by the provided idelta.
	/// </summary>
	private static void SwitchActiveSlot(InputBuilder input, IBaseInventory inventory, int idelta)
	{
		var count = inventory.Count();
		if (count == 0)
			return;
		// Get the next slot by the delta.
		var slot = inventory.GetActiveSlot();
		var nextSlot = slot + idelta;

		while (nextSlot < 0)
			nextSlot += count;

		while (nextSlot >= count)
			nextSlot -= count;

		SetActiveSlot( input, inventory, nextSlot );
	}
}