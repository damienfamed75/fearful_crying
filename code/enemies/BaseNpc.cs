using Sandbox;

namespace FearfulCry.Enemies;

/// <summary>
/// BaseNpc is based on gvarados1's BaseNpc class made for Zombie Horde.
/// see: https://github.com/gvarados1/sbox-npc-zombie-horde/blob/main/code/Zombies/BaseNpc.cs
/// </summary>
[Category("NPC")]
public partial class BaseNpc : AnimatedEntity
{
	private DamageInfo lastDamage;
    /// <summary>
	/// CriticalHitboxGroup represents where the player can hit this NPC to get
	/// a critical hit. For instance, for the Citizen model the head is 1.
	/// </summary>
	protected static int CriticalHitboxGroup => 1;
    /// <summary>
	/// If a critical hitbox group is hit, then apply this critical hit modifier
	/// to the incoming damage.
	/// </summary>
	protected static float CriticalHitModifier => 2.0f;
	/// <summary>
	/// path to the particles used for when this Npc dies from a blast explosion.
	/// </summary>
	protected static string BlastParticles => "particles/impact.flesh-big.vpcf";

	public override void Spawn()
    {
		base.Spawn();
		Tags.Add( "npc" );
	}

	public override void TakeDamage( DamageInfo info )
	{
        // Save the last damage information.
		lastDamage = info;

        // if the critical hitbox group is damaged, then multiply the damage
        // by the critical hit modifier.
        if (GetHitboxGroup(info.HitboxIndex) == CriticalHitboxGroup ) {
			info.Damage *= CriticalHitModifier;
		}

        // Procedurally affects the animgraph to twitch according to where this
        // entity was hit.
		this.ProceduralHitReaction( info );

		base.TakeDamage( info );
	}

	public override void OnKilled()
	{
		base.OnKilled();

        if (lastDamage.Flags.HasFlag(DamageFlags.Blast)) {
            // Turn off prediction.
            using (Prediction.Off()) {
				// Bloody explosion.
				var particles = Particles.Create( BlastParticles );
                if (particles != null) {
                    //! TODO remove magic number.
					particles.SetPosition( 0, Position + Vector3.Up * 40 );
				}
			}
			// Become Ragdoll
            BecomeRagdollOnClient(
                (lastDamage.Force / 4) + Vector3.Up*300, // Dampen and send up.
                GetHitboxBone( lastDamage.HitboxIndex )
            );
        } else {
            // Become Ragdoll
            BecomeRagdollOnClient(
                lastDamage.Force,
                GetHitboxBone( lastDamage.HitboxIndex )
            );
        }
	}

    [ClientRpc]
    public virtual void PlaySoundOnClient(string sound)
    {
		Sound.FromWorld( sound, Position + Vector3.Up * 60 );
	}

    public virtual void DamagedEffects()
    {
		Velocity *= 0.1f;
        if (Health > 0) {
            //! TODO create sounds.
			// PlaySoundOnClient( "zombie.hurt" );
		}
	}
}