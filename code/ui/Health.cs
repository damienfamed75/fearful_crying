using FearfulCry.player;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Health : Panel
{
	// public Label Label;

	public Label CurrentHealth;
	public Label MaxHealth;
	public Label Bar;
	public Label BarGrey;
	
	public static Health Current { get; private set; }

	public Health()
    {
		Current = this;

		CurrentHealth = Add.Label( "🩸 ", "health-current" );
		// MaxHealth = Add.Label( "100", "health-max" );
		Bar = Add.Label( "", "health-bar" );
		BarGrey = Add.Label( "", "health-bar-grey" );
		// Label = Add.Label( "100", "value" );
	}

    public override void Tick()
    {
		var player = Local.Pawn as FearfulCryPlayer;
        if (player == null)
			return;

		// CurrentHealth.Text = $"🩸 "; // alternates ➕🫀🩸❤️
		// Width maximum
		var width = 200;
		var barWidth = width * (player.Health / player.MaxHealth).Clamp( 0, 1 );
		
		Bar.Style.Width = barWidth;
		// BarGrey.Style.Width = barWidth;
		BarGrey.Style.Width = width - barWidth;

		BarGrey.Style.BackgroundColor = (Color)Color.Parse("#111");
		// BarGrey.Style.

		if (player.Health == player.MaxHealth) {
			Bar.Style.BorderBottomRightRadius = 5;
			Bar.Style.BorderTopRightRadius = 5;
		} else {
			Bar.Style.BorderBottomRightRadius = 0;
			Bar.Style.BorderTopRightRadius = 0;
		}
		
		// var right = 150;
		// var left = 150;

		// CurrentHealth.Style.Right = right;
		// CurrentHealth.Style.FontColor = (Color)Color.Parse( "#ffffff" );
		// MaxHealth.Style.Left = left;

		var color = Color.Parse("#00ff5f");

		// todo change color based on amount of health left.

		Bar.Style.BackgroundColor = color;

		SetClass( "low", player.Health < 40.0f );
		SetClass("empty", player.Health <= 0.0f);
	}
}