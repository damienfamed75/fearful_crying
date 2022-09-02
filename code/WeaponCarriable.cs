using Sandbox;

public partial class Weapon : BaseWeapon, IUse
{
    /// <summary>
	/// When the player equips this weapon.
	/// </summary>
	public override void ActiveStart( Entity ent )
	{
		base.ActiveStart( ent );

		TimeSinceDeployed = 0;
	}

    /// <summary>
	/// Takes the ViewModelPath and creates a new "carrying" weapon model.
	/// </summary>
	public override void CreateViewModel()
	{
		// Assert that this is being called by a client.
		Host.AssertClient();
		// Check if the viewmodel path is invalid.
		if ( string.IsNullOrEmpty( ViewModelPath ) )
			return;

		// Create a new empty viewmodel and set the owner.
		ViewModelEntity = new ViewModel
		{
			Position = Position,
			Owner = Owner,
			EnableViewmodelRendering = true
		};
		// Set the viewmodel's model.
		ViewModelEntity.SetModel( ViewModelPath );
	}
}