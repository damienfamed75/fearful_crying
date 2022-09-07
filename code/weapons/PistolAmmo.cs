using System;
using System.Xml.Schema;
using Sandbox;
using SandboxEditor;

[Library("ammo_pistol"), HammerEntity]
[Title("Pistol Ammo"), Category("Ammo")]
[EditorModel("models/sbox_props/burger_box/burger_box.vmdl")]
public class PistolAmmo : Ammunition
{
	protected override Type WeaponType { get; set; } = typeof( Pistol );
	protected override int AmmoAmount { get; set; } = 25;
	protected override string ModelPath { get; set; } = "models/sbox_props/burger_box/burger_box.vmdl";
	protected override string SoundPath { get; set; } = "pistol_ammo_pickup";
}