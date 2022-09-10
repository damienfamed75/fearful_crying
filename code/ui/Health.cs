using FearfulCry.player;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class Health : Panel
{
	public Label CurrentHealth;
	public Label MaxHealth;
	public Label Bar;
	public Label BarGrey;

	private Color HighHealth;
	private Color LowHealth;

	public static Health Current { get; private set; }

	public Health()
    {
		Current = this;

		HighHealth = (Color)Color.Parse( "#00ff5f" );
		LowHealth = (Color)Color.Parse( "#bf250f" );

		Bar = AddChild<Label>( "health-bar" );
		BarGrey = AddChild<Label>( "health-bar-grey" );
		CurrentHealth = AddChild<Label>( "health-current" );
		CurrentHealth.SetText( "🩸 " );
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
		BarGrey.Style.Width = width - barWidth;

		if (player.Health == player.MaxHealth) {
			Bar.Style.BorderBottomRightRadius = 5;
			Bar.Style.BorderTopRightRadius = 5;
		} else {
			Bar.Style.BorderBottomRightRadius = 0;
			Bar.Style.BorderTopRightRadius = 0;
		}

		// Make the color more red based on how little health the player has.
		Bar.Style.BackgroundColor = Color.Lerp(
			LowHealth,
			HighHealth,
			player.Health / player.MaxHealth
		);

		SetClass( "low", player.Health < 40.0f );
		SetClass("empty", player.Health <= 0.0f);
	}
}