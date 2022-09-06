using Sandbox;

public partial class AmmoWeapon : Weapon
{
    //
    // Ammunition.
    //

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
    [Net, Predicted]
	public int TotalBulletCount { get; set; }

	public virtual string DryFireSound => null;

	/// <summary>
	/// Does this weapon have ammo?
	/// </summary>
	/// <returns>Whether this weapon has ammo or not</returns>
	public bool HasAmmo() => CurrentBulletCount != 0;

    /// <summary>
	/// Subtracts the ammo from the current clip.
	/// </summary>
	public override void AttackPrimary() => CurrentBulletCount--;

    /// <summary>
	/// Handles reloading ammunition numbers and that's about it.
	/// </summary>
    public override void Reload()
    {
		// If there's no more ammo left, then don't reload.
		if (TotalBulletCount == 0)
			return;

		// Don't reload if there's nothing to reload.
		if (CurrentBulletCount == MagSize)
			return;

		var oldCurrent = CurrentBulletCount;
		var diff = MagSize - oldCurrent;
        
        if (TotalBulletCount > diff) {
			TotalBulletCount -= diff;
			CurrentBulletCount = MagSize;
		} else {
			CurrentBulletCount += TotalBulletCount;
			TotalBulletCount = 0;
		}

        base.Reload();
    }
}