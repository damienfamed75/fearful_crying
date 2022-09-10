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
		
		if ( (player as Player).ActiveChild is AmmoWeapon aw ) {
			Style.Opacity = 1f;
			Style.BackgroundColor = Color.Parse("#333").Value.WithAlpha(.5f);
			Label.Text = $"🔥 {aw.CurrentBulletCount} / {aw.TotalBulletCount}";
		} else {
			Style.Opacity = 0f;
		}
	}
}