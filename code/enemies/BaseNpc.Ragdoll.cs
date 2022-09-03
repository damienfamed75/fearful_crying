using Sandbox;

namespace FearfulCry.Enemies;

public partial class BaseNpc
{
    // Maximum total number of ragdolls.
	static EntityLimit RagdollLimit = new EntityLimit { MaxTotal = 25 };

	private float secondsToDelete => 20.0f;

	private float ragdollForce => 1000.0f;

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
		var ent = new ModelEntity();

		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.PhysicsEnabled = true;
		ent.UsePhysicsCollision = true;
		ent.Tags.Add( "ragdoll", "gib", "debris" );

		ent.CopyFrom( this );
		ent.CopyBonesFrom( this );
		ent.SetRagdollVelocityFrom( this );
		ent.DeleteAsync(secondsToDelete);
        ent.RenderColor = RenderColor;

        // Copy the clothes over
        foreach (var child in Children) {
            if (!child.Tags.Has("clothes"))
				continue;
            
            if (child is ModelEntity e) {
				var clothing = new ModelEntity();
				clothing.CopyFrom( e );
				clothing.SetParent( ent, true );
				clothing.RenderColor = RenderColor;
			}
		}

		ent.PhysicsGroup.AddVelocity( force );

        // If force bone was specified.
        if (forceBone >= 0) {
			var body = ent.GetBonePhysicsBody( forceBone );
            if (body != null) {
                //! TODO remove magic number
				body.ApplyForce( force * ragdollForce );
			} else {
				ent.PhysicsGroup.AddVelocity( force );
			}
		}

		RagdollLimit.Watch( ent );
	}
}