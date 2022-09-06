using FearfulCry.player;
using Sandbox;
using System;
using System.Linq;
using System.Security;

//
// You don't need to put things in a namespace, but it doesn't hurt.
//
namespace FearfulCry;

/// <summary>
/// This is your game class. This is an entity that is created serverside when
/// the game starts, and is replicated to the client. 
/// 
/// You can use this to create things like HUDs and declare which player class
/// to use for spawned players.
/// </summary>
public partial class MyGame : Sandbox.Game
{
	StandardPostProcess postProcess;

	[Net, Change]
	public int NumClients { get; set; }
	/// <summary>
	/// Called automatically when NumClients is changed because of the attribute.
	/// See: https://wiki.facepunch.com/sbox/Network_Basics#howwilliknowwhenmyvariablegetsupdated
	/// </summary>
	private void OnNumClientsChanged(int oldVal, int newVal)
	{
		Log.Info( $"Number of clients changed. before({oldVal}) after({newVal})" );
	}

	/// <summary>
	/// Console Commands...
	/// </summary>

	[ConCmd.Admin("heal")]
	public static void Heal()
	{
		var callingClient = ConsoleSystem.Caller;
		(callingClient.Pawn as Player).Health += 10;
	}


	public MyGame()
	{
		if (IsServer) {
			// Create hud.
			_ = new FearfulCryingHud();
		}

		if (IsClient) {
			postProcess = new StandardPostProcess();
			PostProcess.Add( postProcess );
		}

	}

	/// <summary>
	/// A client has joined the server. Make them a pawn to play with
	/// </summary>
	public override void ClientJoined( Client client )
	{
		base.ClientJoined( client );

		// Create a pawn for this client to play with
		var pawn = new player.FearfulCryPlayer( client );
		pawn.Respawn();
		client.Pawn = pawn;

		// Get all of the spawnpoints
		var spawnpoints = Entity.All.OfType<SpawnPoint>();

		// chose a random one
		var randomSpawnPoint = spawnpoints.OrderBy( x => Guid.NewGuid() ).FirstOrDefault();

		// if it exists, place the pawn there
		if ( randomSpawnPoint != null )
		{
			var tx = randomSpawnPoint.Transform;
			tx.Position = tx.Position + Vector3.Up * 20.0f; // raise it up
			pawn.Transform = tx;
		}

		NumClients++;
	}

	public override void FrameSimulate( Client cl )
	{
		base.FrameSimulate( cl );

		postProcess.Sharpen.Enabled = true;
		postProcess.Sharpen.Strength = 0.5f;

		postProcess.Saturate.Enabled = true;
		postProcess.Saturate.Amount = 1f;

		postProcess.Vignette.Enabled = true;
		postProcess.Vignette.Intensity = 0.2f;
		postProcess.Vignette.Roundness = 1.5f;
		postProcess.Vignette.Smoothness = .5f;
		postProcess.Vignette.Color = Color.Black;

		postProcess.FilmGrain.Enabled = true;
		postProcess.FilmGrain.Intensity = 0.2f;
		postProcess.FilmGrain.Response = .3f;

		postProcess.ChromaticAberration.Enabled = true;
		postProcess.ChromaticAberration.Offset = 0.004f;

		postProcess.Blur.Enabled = false;

		if (Local.Pawn is FearfulCryPlayer player) {
			var timeSinceDamage = player.TimeSinceDamage.Relative / 2;
			var damageUI = timeSinceDamage.LerpInverse( 0.25f, 0.0f, true ) * 0.2f; // 0.2f
			if (damageUI > 0) {
				postProcess.Saturate.Amount -= damageUI;

				postProcess.Vignette.Intensity += damageUI*5;
				postProcess.Vignette.Color = Color.Lerp( postProcess.Vignette.Color, Color.Red, damageUI );
				postProcess.Vignette.Smoothness += damageUI;
				postProcess.Vignette.Roundness += damageUI;

				postProcess.Blur.Enabled = true;
				postProcess.Blur.Strength = damageUI * 0.5f;

				postProcess.ChromaticAberration.Offset += damageUI / 100;
			}

			var lowHealthUI = player.Health.LerpInverse( 50.0f, 0.0f, true );
			if (player.LifeState == LifeState.Dead)
				lowHealthUI = 0;

			if (lowHealthUI > 0) {
				postProcess.Saturate.Amount -= lowHealthUI;

				postProcess.FilmGrain.Intensity += lowHealthUI * 0.25f;

				postProcess.Blur.Enabled = true;
				postProcess.Blur.Strength = lowHealthUI * 0.08f;

				Audio.SetEffect( "core.player.death.muffle1", lowHealthUI * 0.8f, 2.0f );
			}
		}
	}
}
