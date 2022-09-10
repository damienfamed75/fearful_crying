
/// <summary>
/// Used to get an ammo type for a weapon.
/// </summary>
public interface IAmmoTypeWeapon
{
	AmmoType GetAmmoType();
}

public partial class Weapon
{
    /// <summary>
	/// Override this if the weapon has an ammo type.
	/// </summary>
    public virtual AmmoType GetAmmoType() => AmmoType.None;
}