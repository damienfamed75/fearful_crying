﻿using Sandbox;
using System;
using System.Linq;

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

	[ConCmd.Admin("im_a_cheater")]
	public static void FirstConsoleCommand()
	{
		var callingClient = ConsoleSystem.Caller;
		callingClient.Kick();
	}


	public MyGame()
	{
		if (IsServer) {
			// Create hud.
			_ = new FearfulCryingHud();
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
}
