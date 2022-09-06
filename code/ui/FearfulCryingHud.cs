using System.Linq;
using Sandbox;
using Sandbox.UI;

public partial class FearfulCryingHud : HudEntity<RootPanel>
{
	public FearfulCryingHud()
    {
        // Only call when client.
        if (!IsClient)
			return;

		RootPanel.StyleSheet.Load( "/ui/FearfulCryingHud.scss" );

		RootPanel.AddChild<Crosshair>();
		RootPanel.AddChild<Health>();
		RootPanel.AddChild<Ammo>();
	}
}