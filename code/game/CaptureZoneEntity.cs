using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Events;
using Ultraneon;
using Ultraneon.Events;

[Category( "Ultraneon" )]
[Icon( "place" )]
public sealed class CaptureZoneEntity : Component, Component.ITriggerListener
{
	[Property]
	public string PointName { get; set; } = "Capture Zone";
	
	[Property]
	public Color NeutralColor { get; set; }
	
	[Property]
	public Color PlayerColor { get; set; }
	
	[Property]
	public Color EnemyColor { get; set; }

	[Property]
	public float CaptureTime { get; set; } = 15f;

	[Property, HostSync]
	public Team ControllingTeam { get; set; } = Team.Neutral;

	[Property, HostSync]
	public float CaptureProgress { get; set; } = 0f;

	[Property]
	public ModelRenderer ZoneModel { get; set; }

	public float RadarX { get; set; }
	public float RadarY { get; set; }

	private TimeSince timeSinceLastCapture;
	private HashSet<BaseNeonCharacterEntity> charactersInZone = new();

	protected override void OnStart()
	{
		base.OnStart();
		timeSinceLastCapture = 0f;
		UpdateZoneVisuals();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
			return;

		UpdateCapture();
		UpdateZoneVisuals();
	}

	private void UpdateCapture()
	{
		if ( charactersInZone.Any() )
		{
			var dominantTeam = charactersInZone
				.GroupBy( p => p.CurrentTeam )
				.OrderByDescending( g => g.Count() )
				.First().Key;

			if ( dominantTeam != ControllingTeam )
			{
				CaptureProgress += Time.Delta / CaptureTime * charactersInZone.Count( p => p.CurrentTeam == dominantTeam );
				if ( CaptureProgress >= 1f )
				{
					var previousTeam = ControllingTeam;
					ControllingTeam = dominantTeam;
					CaptureProgress = 0f;
					OnPointCaptured( previousTeam );
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
				var previousTeam = ControllingTeam;
				ControllingTeam = Team.Neutral;
				CaptureProgress = 0f;
				OnPointNeutralized( previousTeam );
			}
		}
	}

	private void UpdateZoneVisuals()
	{
		if ( ZoneModel != null )
		{
			Color teamColor = ControllingTeam switch
			{
				Team.Player => PlayerColor,
				Team.Enemy => EnemyColor,
				_ => NeutralColor
			};

			ZoneModel.Tint = teamColor;

			// TODO: Add particle effects or other visual indicators for capture progress
		}
	}

	public void OnTriggerEnter( Collider other )
	{
		var character = other.GameObject.Components.Get<BaseNeonCharacterEntity>();
		if ( character != null )
		{
			charactersInZone.Add( character );
			if ( charactersInZone.Count == 1 )
			{
				OnStartCapture();
			}
		}
	}

	public void OnTriggerExit( Collider other )
	{
		var player = other.GameObject.Components.Get<PlayerNeon>();
		if ( player != null )
		{
			charactersInZone.Remove( player );
		}
	}

	private void OnStartCapture()
	{
		Log.Info( $"{PointName} is being captured!" );
		// TODO: Send a lightwave in radius to alert other players
	}

	private void OnPointCaptured( Team previousTeam )
	{
		Log.Info( $"{PointName} has been captured by {ControllingTeam}!" );
		GameObject.Dispatch( new CaptureZoneEvent( PointName, previousTeam, ControllingTeam ) );
	}

	private void OnPointNeutralized( Team previousTeam )
	{
		Log.Info( $"{PointName} has been neutralized!" );
		GameObject.Dispatch( new CaptureZoneEvent( PointName, previousTeam, Team.Neutral ) );
	}

	public bool IsPlayerInZone( PlayerNeon player )
	{
		return charactersInZone.Contains( player );
	}
}
