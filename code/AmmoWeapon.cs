using Sandbox;

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
    [Net, Predicted]
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

		// Get the new current bullet count.
		var newCurrent = TotalBulletCount + CurrentBulletCount;
		// Subtract from the total ammo.
		TotalBulletCount -= MagSize - CurrentBulletCount;
		// If the new current is larger than the magazine size, then use the
		// magazine size instead.
		CurrentBulletCount = newCurrent > MagSize ? MagSize : newCurrent;

        base.Reload();
    }
}