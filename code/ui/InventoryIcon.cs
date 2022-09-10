using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class InventoryIcon : Panel
{
	public Entity TargetEnt;
    public Label Label;
	public Label Number;

    public InventoryIcon(int i, Panel parent)
    {
		Parent = parent;
		Label = AddChild<Label>( "item-name" );
		Number = AddChild<Label>( "slot-number" );
		Number.SetText( $"{i}" );
	}

    public void Clear()
    {
		Label.Text = "";

		SetClass( "active", false );
	}
}