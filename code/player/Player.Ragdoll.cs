using Sandbox;

namespace FearfulCry.player;

partial class FearfulCryPlayer
{
    [ClientRpc]
    private void BecomeRagdollOnClient(Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone)
    {
		var ent = new ModelEntity();

        // Setup the ragdoll's physics, position, and velocity.
		ent.Tags.Add( "ragdoll", "solid", "debris" );
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.Scale = Scale;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.SetModel( GetModelName() );
		ent.CopyBonesFrom( this );
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.CopyMaterialOverrides( this );
		ent.TakeDecalsFrom( this );
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColor = RenderColor;
		ent.PhysicsGroup.Velocity = velocity;
		ent.PhysicsEnabled = true;

        // Set the clothes for the ragdoll.
        foreach (var child in Children) {
            if (!child.Tags.Has("clothes"))
                continue;

			if (child is ModelEntity e) {
				var model = e.GetModelName();

				var clothing = new ModelEntity();
				clothing.SetModel( model );
				clothing.SetParent( ent, true );
				clothing.RenderColor = e.RenderColor;
				clothing.CopyBodyGroups( e );
				clothing.CopyMaterialGroup( e );
			}
		}

		Corpse = ent;

		ent.DeleteAsync( 10.0f );
	}
}