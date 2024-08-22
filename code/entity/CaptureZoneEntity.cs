using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Ultraneon;

[Category( "Ultraneon" )]
[Icon( "place" )]
public sealed class CaptureZoneEntity : Component, Component.ITriggerListener
{
	[Property]
	public string PointName { get; set; } = "Capture Zone";

	[Property]
	public float CaptureTime { get; set; } = 15f;

	[Property, HostSync]
	public Team ControllingTeam { get; set; } = Team.Neutral;

	[Property, HostSync]
	public float CaptureProgress { get; set; } = 0f;

	public float RadarX { get; set; }
	public float RadarY { get; set; }

	private TimeSince timeSinceLastCapture;
	private HashSet<Entity> playersInZone = new();

	protected override void OnStart()
	{
		base.OnStart();
		timeSinceLastCapture = 0f;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		UpdateCapture();
	}

	private void UpdateCapture()
	{
		if ( playersInZone.Any() )
		{
			var dominantTeamStr = playersInZone
				.GroupBy( p => p.GameObject.Tags.First( t => t is "player" or "bot" ) )
				.OrderByDescending( g => g.Count() )
				.First().Key;

			var dominantTeam = Teams.GetTeamFromTag( dominantTeamStr );

			if ( dominantTeam != ControllingTeam )
			{
				CaptureProgress += Time.Delta / CaptureTime;
				if ( CaptureProgress >= 1f )
				{
					ControllingTeam = dominantTeam;
					CaptureProgress = 0f;
					OnPointCaptured();
				}
			}
			else
			{
				CaptureProgress = Math.Max( 0f, CaptureProgress - Time.Delta / CaptureTime );
			}

			timeSinceLastCapture = 0f;
		}
		else if ( timeSinceLastCapture > 5f && ControllingTeam != Team.Neutral )
		{
			CaptureProgress += Time.Delta / CaptureTime;
			if ( CaptureProgress >= 1f )
			{
				ControllingTeam = Team.Neutral;
				CaptureProgress = 0f;
				OnPointNeutralized();
			}
		}
	}

	void Component.ITriggerListener.OnTriggerEnter( Collider other )
	{
		var entity = other.GameObject.Components.Get<Entity>();
		if ( entity != null && entity.GameObject.Tags.Has( "player" ) )
		{
			playersInZone.Add( entity );
			if ( playersInZone.Count == 1 )
			{
				OnStartCapture();
			}
		}
	}

	void Component.ITriggerListener.OnTriggerExit( Collider other )
	{
		var entity = other.GameObject.Components.Get<Entity>();
		if ( entity != null )
		{
			playersInZone.Remove( entity );
		}
	}

	private void OnStartCapture()
	{
		Log.Info( $"{PointName} is being captured!" );
		// TODO: Send a lightwave in radius to alert other players
	}

	private void OnPointCaptured()
	{
		Log.Info( $"{PointName} has been captured by {ControllingTeam}!" );
		// TODO: Set as spawn point for the capturing team
		// TODO: Update score (see scoring system in design document)
	}

	private void OnPointNeutralized()
	{
		Log.Info( $"{PointName} has been neutralized!" );
	}
}
