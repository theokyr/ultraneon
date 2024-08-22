using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Events;

namespace Ultraneon;

public class GameService : Component,
	IGameEventHandler<CaptureZoneEvent>,
	IGameEventHandler<PlayerSpawnEvent>,
	IGameEventHandler<CharacterDeathEvent>,
	IGameEventHandler<DamageEvent>
{
	[Property]
	public float RoundTime { get; set; } = 600f; // 10 min

	[Property]
	public int ScoreToWin { get; set; } = 1000;

	[Property]
	public List<CaptureZoneEntity> CaptureZones { get; set; } = new();

	private TimeSince roundStartTime;
	private Dictionary<Team, int> teamScores = new();

	protected override void OnStart()
	{
		if ( IsProxy ) return;

		InitializeGame();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy ) return;

		UpdateGameState();
	}

	private void InitializeGame()
	{
		roundStartTime = 0f;
		teamScores[Team.Player] = 0;
		teamScores[Team.Enemy] = 0;

		// Initialize capture zones
		CaptureZones = Scene.GetAllComponents<CaptureZoneEntity>().ToList();
		foreach ( var zone in CaptureZones )
		{
			zone.ControllingTeam = Team.Neutral;
			zone.CaptureProgress = 0f;
		}

		// Spawn players
		SpawnPlayers();
	}

	private void UpdateGameState()
	{
		if ( roundStartTime >= RoundTime )
		{
			EndGame();
			return;
		}

		UpdateScores();

		if ( teamScores[Team.Player] >= ScoreToWin || teamScores[Team.Enemy] >= ScoreToWin )
		{
			EndGame();
		}
	}

	private void UpdateScores()
	{
		foreach ( var zone in CaptureZones )
		{
			if ( zone.ControllingTeam != Team.Neutral )
			{
				teamScores[zone.ControllingTeam]++;
			}
		}
	}

	private void SpawnPlayers()
	{
		// TODO: Implement player spawning logic
		GameObject.Dispatch( new PlayerSpawnEvent( Team.Player ) );
		GameObject.Dispatch( new PlayerSpawnEvent( Team.Enemy ) );
	}

	private void EndGame()
	{
		var winningTeam = teamScores[Team.Player] > teamScores[Team.Enemy] ? Team.Player : Team.Enemy;
		Log.Info( $"Game Over! {winningTeam} wins with a score of {teamScores[winningTeam]}" );
		// TODO: Implement game end logic (show results, restart, etc.)
	}

	public void OnGameEvent( DamageEvent eventArgs )
	{
		if ( eventArgs.Target is BaseNeonCharacterEntity targetEntity )
		{
			targetEntity.Health -= eventArgs.Damage;
			Log.Info(
				$"{targetEntity.EntityName} took {eventArgs.Damage} damage from {eventArgs.Attacker?.EntityName ?? "unknown"}. Remaining health: {targetEntity.Health}" );

			if ( targetEntity.Health <= 0 )
			{
				var killerEntity = eventArgs.Attacker?.Components.Get<BaseNeonCharacterEntity>();
				GameObject.Dispatch( new CharacterDeathEvent( targetEntity, killerEntity, true ) );
			}
		}
	}

	public void OnGameEvent( CaptureZoneEvent eventArgs )
	{
		if ( eventArgs.PreviousTeam != Team.Neutral )
		{
			teamScores[eventArgs.PreviousTeam] -= 150; // Lose points for losing a zone
		}

		if ( eventArgs.NewTeam != Team.Neutral )
		{
			teamScores[eventArgs.NewTeam] += eventArgs.PreviousTeam == Team.Neutral ? 100 : 200; // Points for capturing
		}

		Log.Info(
			$"Zone {eventArgs.ZoneName} captured by {eventArgs.NewTeam}. New scores - Player: {teamScores[Team.Player]}, Enemy: {teamScores[Team.Enemy]}" );
	}

	public void OnGameEvent( PlayerSpawnEvent eventArgs )
	{
		// TODO: Implement player spawning logic
		Log.Info( $"Player spawned for team {eventArgs.Team}" );
	}

	public void OnGameEvent( CharacterDeathEvent eventArgs )
	{
		if ( eventArgs.Killer is not BaseNeonCharacterEntity killer )
		{
			return;
		}

		if ( killer.CurrentTeam != eventArgs.Victim.CurrentTeam )
		{
			var scoreAward = eventArgs.IsStylish ? 40 : 10;
			teamScores[killer.CurrentTeam] += scoreAward;
			Log.Info( $"{killer.CurrentTeam} scored {scoreAward} points for a kill" );
		}
	}
}

public record CaptureZoneEvent( string ZoneName, Team PreviousTeam, Team NewTeam ) : IGameEvent;

public record PlayerSpawnEvent( Team Team ) : IGameEvent;

public record DamageEvent( BaseNeonCharacterEntity Target, Entity Attacker, float Damage, Vector3 Position )
	: IGameEvent;

public record CharacterDeathEvent( BaseNeonCharacterEntity Victim, Entity Killer, bool IsStylish ) : IGameEvent;
