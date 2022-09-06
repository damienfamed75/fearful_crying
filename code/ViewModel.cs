using Sandbox;

// ViewModel is the model of an item. (first person)
public class ViewModel : BaseViewModel
{
	// variables that alter how much swing and bobbing there is on this item.
	protected static float SwingInfluence => 0.05f;
	protected static float ReturnSpeed => 5.0f;
	protected static float MaxOffsetLength => 10.0f;
	protected static float BobCycleTime => 7.0f;
	protected static Vector3 BobDirection => new( 0.0f, 1.0f, 0.5f );

	// variables used for calculating the amount of swing and bobbing.
	private Vector3 swingOffset;
	private float lastPitch;
	private float lastYaw;
	private float bobAnim;

	// activated dictates whether this model should be shown or not.
	private bool activated = false;

	// EnableSwingAndBob can be altered depending on the preference of the children classes.
	public bool EnableSwingAndBob = true;

	// The inertia based on the owner's velocity.
	public float YawInertia { get; private set; }
	public float PitchInertia { get; private set; }

	/// <summary>
	/// PostCameraSetup sets up the headbob and swinging of the viewmodel in your hands.
	/// </summary>
	/// <param name="camSetup">the built camera (see addons/base/code/Game.cs)</param>
	public override void PostCameraSetup( ref CameraSetup camSetup )
	{
		base.PostCameraSetup( ref camSetup );

		// If the local pawn isn't valid then return.
		if ( !Local.Pawn.IsValid() )
			return;

		if ( !activated )
		{
			lastPitch = camSetup.Rotation.Pitch();
			lastYaw = camSetup.Rotation.Yaw();

			YawInertia = 0;
			PitchInertia = 0;

			activated = true;
		}

		Position = camSetup.Position;
		Rotation = camSetup.Rotation;

		var cameraBoneIndex = GetBoneIndex( "camera" );
		if ( cameraBoneIndex != -1 )
		{
			camSetup.Rotation *= Rotation.Inverse * GetBoneTransform( cameraBoneIndex ).Rotation;
		}

		var newPitch = Rotation.Pitch();
		var newYaw = Rotation.Yaw();

		PitchInertia = Angles.NormalizeAngle( newPitch - lastPitch );
		YawInertia = Angles.NormalizeAngle( lastYaw - newYaw );

		// If headbob is enabled.
		if (EnableSwingAndBob) {
			SetSwingAndBob(newPitch);
		} else {
			SetAnimParameter( "aim_yaw_inertia", YawInertia );
			SetAnimParameter( "aim_pitch_inertia", PitchInertia );
		}

		lastPitch = newPitch;
		lastYaw = newYaw;
	}

	/// <summary>
	/// Based on the player's velocity and viewpoint pitch, set the viewmodel's
	/// offset by the amount of expected swing and bobbing.
	/// 
	/// If the player is noclipping then bobbing doesn't get applied, only inertia.
	/// </summary>
	/// <param name="newPitch"></param>
	protected void SetSwingAndBob(float newPitch)
	{
		// Store the local client's pawn velocity.
		var playerVelocity = Local.Pawn.Velocity;
		// And if the local client's Pawn is a Player type.
		// (which it should be all the time.)
		if (Local.Pawn is Player player) {
			// Get the player controller.
			var controller = player.GetActiveController();
			// If the player is noclipping, then don't apply bobbing.
			if (controller != null && controller.HasTag( "noclip" )) {
				playerVelocity = Vector3.Zero;
			}
		}

		var verticalDelta = playerVelocity.z * Time.Delta;
		var viewDown = Rotation.FromPitch( newPitch ).Up * -1.0f;
		verticalDelta *= 1.0f - System.MathF.Abs( viewDown.Cross( Vector3.Down ).y );
		var pitchDelta = PitchInertia - verticalDelta * 1;
		var yawDelta = YawInertia;

		var offset = CalcSwingOffset( pitchDelta, yawDelta );
		offset += CalcBobbingOffset( playerVelocity );

		Position += Rotation * offset;
	}

	/// <summary>
	/// Calculates the amount of swinging the weapon does based on the player's
	/// pitch and yaw delta.
	/// </summary>
	/// <param name="pitchDelta">the delta of the player's up and down rotation</param>
	/// <param name="yawDelta">the delta of the player's sideways rotation</param>
	/// <returns>an offset vector3 to be multiplied to a rotation and added to the position of the weapon</returns>
	protected Vector3 CalcSwingOffset( float pitchDelta, float yawDelta )
	{
		Vector3 swingVelocity = new( 0, yawDelta, pitchDelta );

		swingOffset -= swingOffset * ReturnSpeed * Time.Delta;
		swingOffset += (swingVelocity * SwingInfluence);

		if ( swingOffset.Length > MaxOffsetLength ) {
			swingOffset = swingOffset.Normal * MaxOffsetLength;
		}

		return swingOffset;
	}

	/// <summary>
	/// Given the pawn velocity, and the BobCycleTime, create an offset for the
	/// player's headbobbing.
	/// </summary>
	/// <param name="velocity">the local client's pawn velocity</param>
	/// <returns>an offset vector3 to be multiplied to a rotation and added to the position of the weapon</returns>
	protected Vector3 CalcBobbingOffset( Vector3 velocity )
	{
		bobAnim += Time.Delta * BobCycleTime;

		var twoPi = System.MathF.PI * 2.0f;
		if ( bobAnim > twoPi ) {
			bobAnim -= twoPi;
		}

		var speed = new Vector2( velocity.x, velocity.y ).Length;
		// If the speed is less than 10, then zero out speed.
		speed = speed > 10.0 ? speed : 0.0f;

		var offset = BobDirection * (speed * 0.005f) * System.MathF.Cos( bobAnim );
		offset = offset.WithZ( -System.MathF.Abs( offset.z ) );

		return offset;
	}
}
