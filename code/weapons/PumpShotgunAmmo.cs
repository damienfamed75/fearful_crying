using System;
using Sandbox;
using SandboxEditor;

[Library("ammo_pumpshotgun"), HammerEntity]
[Title("Pump Shotgun Ammo"), Category("Ammo"), Icon("list")]
[EditorModel("models/rust_props/small_junk/cereal_box.vmdl")]
public class PumpShotgunAmmo : Ammunition
{
	protected override int AmmoAmount { get; set; } = 16;
	protected override string ModelPath { get; set; } = "models/rust_props/small_junk/cereal_box.vmdl";
	protected override string SoundPath { get; set; } = "pistol_ammo_pickup";
	protected override AmmoType AmmoType => AmmoType.Shotgun;
}