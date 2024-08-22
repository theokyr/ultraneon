using Sandbox;
using Sandbox.Citizen;
using Ultraneon;

public sealed class BotAi : BaseNeonCharacterEntity
{
	[Property]
	public NavMeshAgent Agent { get; set; }

	[Property]
	public CitizenAnimationHelper AnimationHelper { get; set; }

	[Property]
	public float StopDistance { get; set; } = 100f;

	[Property]
	public float AttackRange { get; set; } = 200f;

	private PlayerNeon CurrentTarget { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( Agent == null )
		{
			Agent = Components.GetOrCreate<NavMeshAgent>();
		}

		if ( AnimationHelper == null )
		{
			AnimationHelper = Components.GetOrCreate<CitizenAnimationHelper>();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !isAlive ) return;

		if ( CurrentTarget != null && CurrentTarget.IsDead )
		{
			CurrentTarget = null;
		}

		if ( CurrentTarget != null )
		{
			UpdateMovement();
			UpdateCombat();
		}
		else
		{
			Agent.Stop();
		}

		UpdateAnimation();
	}

	private void UpdateMovement()
	{
		float distanceToTarget = Vector3.DistanceBetween( Transform.Position, CurrentTarget.Transform.Position );

		if ( distanceToTarget > StopDistance )
		{
			Agent.MoveTo( CurrentTarget.Transform.Position );
		}
		else
		{
			Agent.Stop();
		}
	}

	private void UpdateCombat()
	{
		float distanceToTarget = Vector3.DistanceBetween( Transform.Position, CurrentTarget.Transform.Position );

		// if ( distanceToTarget <= AttackRange )
		// {
		// 	Log.Info( $"BotAi {EntityName} is attacking {CurrentTarget.EntityName}" );
		// }
	}

	private void UpdateAnimation()
	{
		if ( AnimationHelper != null )
		{
			AnimationHelper.WithVelocity( Agent.Velocity );

			if ( CurrentTarget != null )
			{
				AnimationHelper.WithLook( CurrentTarget.Transform.Position - Transform.Position );
			}
		}
	}

	public void SetTarget( PlayerNeon newTarget )
	{
		if ( newTarget != null && newTarget != CurrentTarget && newTarget.isAlive )
		{
			CurrentTarget = newTarget;
			Log.Info( $"BotAi {EntityName} is now targeting {CurrentTarget.EntityName}" );
		}
	}
}
