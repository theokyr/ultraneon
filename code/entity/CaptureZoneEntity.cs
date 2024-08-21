using System;

[Category( "Ultraneon" )]
[Icon( "place" )]
public sealed class CaptureZoneEntity : Component, Component.IDamageable, Component.INetworkListener
{
	[Property]
	public string PointName { get; set; } = "Capture Zone";

	[Property]
	public float CaptureRadius { get; set; } = 5f;

	[Property]
	public float CaptureTime { get; set; } = 10f;

	[Property, HostSync]
	public string ControllingTeam { get; set; } = "Neutral";

	[Property, HostSync]
	public float CaptureProgress { get; set; } = 0f;

	[Property, ReadOnly]
	public float MaxHealth { get; set; } = 100f;

	[Property, HostSync, Change( nameof(OnHealthChanged) )]
	public float Health { get; set; }

	public float RadarX { get; set; }
	public float RadarY { get; set; }

	private TimeSince timeSinceLastCapture;

	protected override void OnStart()
	{
		base.OnStart();
		Health = MaxHealth;
		timeSinceLastCapture = 0f;
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
		{
			return;
		}

		var nearbyPlayers = Scene.Components.GetAll<Entity>()
			.Where( e => e.GameObject.Tags.Has( "player" ) && e.Transform.Position.Distance( Transform.Position ) <= CaptureRadius )
			.ToList();

		if ( nearbyPlayers.Any() )
		{
			var dominantTeam = nearbyPlayers.GroupBy( p => p.GameObject.Tags.First( t => t == "team1" || t == "team2" ) )
				.OrderByDescending( g => g.Count() )
				.First().Key;

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
		else if ( timeSinceLastCapture > 5f && ControllingTeam != "Neutral" )
		{
			CaptureProgress += Time.Delta / CaptureTime;
			if ( CaptureProgress >= 1f )
			{
				ControllingTeam = "Neutral";
				CaptureProgress = 0f;
				OnPointNeutralized();
			}
		}
	}

	public void OnDamage( in DamageInfo damage )
	{
		if ( Health <= 0 ) return;

		Health = Math.Clamp( Health - damage.Damage, 0f, MaxHealth );

		if ( Health <= 0 )
		{
			OnPointDestroyed();
		}
	}

	private void OnHealthChanged()
	{
	}

	private void OnPointCaptured()
	{
		Log.Info( $"{PointName} has been captured by {ControllingTeam}!" );
	}

	private void OnPointNeutralized()
	{
		Log.Info( $"{PointName} has been neutralized!" );
	}

	private void OnPointDestroyed()
	{
		Log.Info( $"{PointName} has been destroyed!" );
		GameObject.Destroy();
	}
}
