using System;
using System.Collections.Generic;
using Sandbox;

namespace FearfulCry.player;

partial class FearfulCryPlayer
{
    // indices are represented by the integer values of the AmmoType enum.
    [Net, Local]
    public List<int> Ammo { get; set; }

    /// <summary>
	/// Clear out all the ammunition on this player.
	/// </summary>
    public void ClearAmmo()
    {
		Ammo.Clear();
	}

    /// <summary>
	/// Returns the amount of ammo this player has of the given type.
	/// </summary>
    public int AmmoCount(AmmoType type)
    {
        // Convert the enum to an integer representing the index.
		var iType = (int)type;
        if (Ammo == null)
			return 0;
        if (Ammo.Count <= iType)
			return 0;

		return Ammo[iType];
	}

    /// <summary>
	/// Sets the ammunition amount to the given type.
	/// </summary>
	/// <returns>true if ammo was set successfully.</returns>
    public bool SetAmmo(AmmoType type, int amount)
    {
        // Convert the enum to an integer representing the index.
		var iType = (int)type;
		if (!Host.IsServer)
			return false;
        if (Ammo == null)
			return false;

        // Keep adding new empty elements until the desired AmmoType index is
        // valid in the Ammo List.
		while (Ammo.Count <= iType) {
			Ammo.Add( 0 );
		}

		Ammo[iType] = amount;

		return true;
	}

    /// <summary>
	/// Attempts to give an amount of ammunition to the player.
	/// 
	/// This method will prevent from going over the maximum amount of held
	/// ammunition and returns the remaining ammo that wasn't taken.
	/// </summary>
	/// <returns>amount of ammo added</returns>
    public int GiveAmmo(AmmoType type, int amount)
    {
        if (!Host.IsServer)
			return 0;
        if (Ammo == null)
			return 0;
        if (type == AmmoType.None)
			return 0;

		var total = AmmoCount( type ) + amount;
		var max = MaxAmmo( type );

        if (total > max)
			total = max;

		var taken = total - AmmoCount( type );
		SetAmmo( type, total );

		return taken;
	}

    /// <summary>
	/// Removes ammunition from the player of the given type.
	/// </summary>
	/// <returns>The amount of ammo removed</returns>
    public int TakeAmmo(AmmoType type, int amount)
    {
        if (Ammo == null)
			return 0;

		var available = AmmoCount( type );
		amount = Math.Min( available, amount );

		SetAmmo( type, available - amount );
		return amount;
	}

    /// <summary>
	/// Returns the maximum amount of ammo that can be held of the given type.
	/// </summary>
    public static int MaxAmmo(AmmoType type) => type switch
    {
        AmmoType.Pistol => 120,
        AmmoType.Shotgun => 25,
        _ => 0,
    };
}