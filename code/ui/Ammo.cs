using FearfulCry.player;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Ammo : Panel
{
	public Label Label;

	public Ammo()
    {
		Label = AddChild<Label>("value");
	}

    public override void Tick()
    {
		var player = Local.Pawn;
        if (player == null)
			return;

		var fplayer = player as FearfulCryPlayer;

		if ( fplayer.ActiveChild is AmmoWeapon aw ) {
			Style.Opacity = 1f;
			Style.BackgroundColor = Color.Parse("#333").Value.WithAlpha(.5f);
			var total = fplayer.AmmoCount( aw.AmmoType );
			Label.Text = $"🔥 {aw.CurrentBulletCount} / {total}";
		} else {
			Style.Opacity = 0f;
		}
	}
}