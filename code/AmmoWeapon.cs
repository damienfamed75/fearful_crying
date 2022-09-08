using System;
using Sandbox;
using Sandbox.UI;

public partial class AmmoWeapon : Weapon
{
    /// <summary>
	/// MagSize represents the maximum clip/magazine size.
	/// </summary>
    [Net, Predicted]
    public int MagSize { get; set; }
    
    /// <summary>
	/// Current number of bullets in the weapon.
	/// </summary>
    [Net, Predicted]
	public int CurrentBulletCount { get; set; }

    /// <summary>
	/// Total number of bullets that the weapon has.
	/// </summary>
    [Net]
	public int TotalBulletCount { get; set; }

	/// <summary>
	/// Does this weapon have ammo?
	/// </summary>
	public bool HasAmmo => CurrentBulletCount != 0;

    /// <summary>
	/// Subtracts the ammo from the current clip.
	/// </summary>
	public override void AttackPrimary() => CurrentBulletCount--;

	/// <summary>
	/// Whether this weapon reloads one bullet at a time.
	/// For example a pump action shotgun would be a single bullet reloading weapon.
	/// </summary>
	public virtual bool SingleBulletReloading => false;

	/// <summary>
	/// SingleBulletReloadTime should be EQUAL OR UNDER ReloadTime.
	/// 
	/// This represents the amount of time to load a single bullet.
	/// </summary>
	public virtual float SingleBulletReloadTime => 0.6f;

	public virtual AmmoType AmmoType { get; set; }

	/// <summary>
	/// The amount of time since a single bullet was loaded.
	/// Used for SingleBulletReloading weapons.
	/// </summary>
	[Net, Predicted]
	protected TimeSince TimeSinceReloadSingleBullet { get; set; }

	/// <summary>
	/// Handles reloading ammunition numbers and that's about it.
	/// </summary>
	public override void Reload()
    {
		// If there's no more ammo left, then don't reload.
		if (TotalBulletCount == 0)
			return;

		// Don't reload if current magazine is already full.
		if (CurrentBulletCount == MagSize)
			return;

		if (SingleBulletReloading) {
			SingleBulletReload();
		} else {
			// Get the new current bullet count.
			var newCurrent = TotalBulletCount + CurrentBulletCount;
			// Subtract from the total ammo.
			TotalBulletCount -= (MagSize - CurrentBulletCount).Clamp(0, TotalBulletCount);
			// If the new current is larger than the magazine size, then use the
			// magazine size instead.
			CurrentBulletCount = newCurrent > MagSize ? MagSize : newCurrent;
		}


        base.Reload();
    }

	/// <summary>
	/// Handles reloading a single bullet at a time.
	/// </summary>
	private void SingleBulletReload()
	{
		// If there's no ammo to reload then return.
		if (TotalBulletCount <= 0)
			return;
		// If the current magazine is already full.
		if (CurrentBulletCount >= MagSize)
			return;

		// Reset the reload times.
		TimeSinceReload = 0;
		TimeSinceReloadSingleBullet = 0;

		// Reload the single bullet.
		CurrentBulletCount++;
		TotalBulletCount--;

		// Plays the reload animation again (animgraph handles replays)
		StartReloadEffects();
	}

	public override void Simulate( Client player )
	{
		if (IsReloading && SingleBulletReloading && (TimeSinceReloadSingleBullet > SingleBulletReloadTime)) {
			// Capture any primary attack input that would interrupt the
			// single bullet reloading.
			bool interruptAttack = Input.Pressed( InputButton.PrimaryAttack )
				|| Input.Released( InputButton.PrimaryAttack )
				|| Input.Down( InputButton.PrimaryAttack );

			if ( interruptAttack ) {
				// Fix the time since reload to finish the reload.
				TimeSinceReload = ReloadTime+1;
			} else {
				SingleBulletReload();
			}
		}

		// Call after checking for single reloads because they should be treated
		// at a higher priority.
		base.Simulate(player);
	}
}