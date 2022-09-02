using Sandbox;
using Sandbox.UI;

public partial class FearfulCryingHud : HudEntity<RootPanel>
{
    public FearfulCryingHud()
    {
        // Only call when client.
        if (!IsClient)
			return;

		RootPanel.StyleSheet.Load( "FearfulCryingHud.scss" );

		RootPanel.AddChild<Crosshair>();
	}
}