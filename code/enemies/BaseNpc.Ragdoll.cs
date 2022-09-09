using Sandbox;

namespace FearfulCry.Enemies;

public partial class BaseNpc
{
    // Maximum total number of ragdolls.
	static readonly EntityLimit RagdollLimit = new(){ MaxTotal = 25 };

	private static float SecondsToDelete => 20.0f;

	private static float RagdollForce => 1500.0f; // 1000.0f

	/// <summary>
	/// Turns the entity into a ragdoll and applies force. The more force
	/// the further the entity gets thrown.
	/// </summary>
	/// <param name="force">amount of force</param>
	/// <param name="forceBone">which bone to apply the force to</param>
	[ClientRpc]
    protected void BecomeRagdollOnClient(Vector3 force, int forceBone)
    {
		// This is a lot like the player ragdoll.
		var ent = new ModelEntity {
			Position = Position,
			Rotation = Rotation,
			PhysicsEnabled = true,
			UsePhysicsCollision = true
		};

		ent.Tags.Add( "ragdoll", "gib", "debris" );
		ent.CopyFrom( this );
		ent.CopyBonesFrom( this );
		ent.SetRagdollVelocityFrom( this );
		ent.DeleteAsync( SecondsToDelete );
        ent.RenderColor = RenderColor;

        // Copy the clothes over
        foreach (var child in Children) {
            if (!child.Tags.Has("clothes"))
				continue;
            
            if (child is ModelEntity e) {
				var clothing = new ModelEntity();
				clothing.CopyFrom( e );
				clothing.SetParent( ent, true );
				clothing.RenderColor = e.RenderColor;
			}
		}

        // If force bone was specified.
        if (forceBone >= 0) {
			var body = ent.GetBonePhysicsBody( forceBone );
            if (body != null) {
                //! TODO remove magic number
				body.ApplyForce( force * RagdollForce );
			} else {
				ent.PhysicsGroup.AddVelocity( force );
			}
		}

		RagdollLimit.Watch( ent );
	}
}