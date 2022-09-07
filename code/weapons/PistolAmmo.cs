using System;
using Sandbox;
using SandboxEditor;

[Library("ammo_pistol"), HammerEntity]
[Title("Pistol Ammo"), Category("Ammo")]
[EditorModel("models/rust_props/small_junk/tea_box.vmdl")]
public class PistolAmmo : Ammunition
{
	protected override Type WeaponType { get; set; } = typeof( Pistol );
	protected override int AmmoAmount { get; set; } = 25;
	protected override string ModelPath { get; set; } = "models/rust_props/small_junk/tea_box.vmdl";
	protected override string SoundPath { get; set; } = "pistol_ammo_pickup";
}