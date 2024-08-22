using Sandbox;
using Ultraneon;

public sealed class SpotAi : Component
{
	[Property]
	public float DetectionRadius { get; set; } = 500f;

	[Property]
	public float DetectionInterval { get; set; } = 0.5f;

	private BotAi ParentBot { get; set; }
	private TimeSince TimeSinceLastDetection { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		ParentBot = GameObject.Parent?.Components.Get<BotAi>();

		if ( ParentBot == null )
		{
			Log.Warning( $"SpotAi on {GameObject.Name} could not find a parent BotAi component." );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( ParentBot == null || !ParentBot.isAlive ) return;

		if ( TimeSinceLastDetection >= DetectionInterval )
		{
			DetectPlayers();
			TimeSinceLastDetection = 0;
		}
	}

	private void DetectPlayers()
	{
		var nearbyEntities = Scene.GetAllComponents<PlayerNeon>();

		foreach ( var player in nearbyEntities )
		{
			if ( player.isAlive && player.CurrentTeam != ParentBot.CurrentTeam )
			{
				float distance = Vector3.DistanceBetween( Transform.Position, player.Transform.Position );

				if ( distance <= DetectionRadius )
				{
					if ( IsPlayerVisible( player ) )
					{
						ParentBot.SetTarget( player );
						return;
					}
				}
			}
		}
	}

	private bool IsPlayerVisible( PlayerNeon player )
	{
		var trace = Scene.Trace.Ray( Transform.Position, player.Transform.Position )
			.WithoutTags( "bot" )
			.Run();

		DebugOverlay.Trace( trace );

		return trace.Hit && trace.GameObject.Components.Get<PlayerNeon>() == player;
	}
}
