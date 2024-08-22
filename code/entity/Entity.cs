using Sandbox;
using Sandbox.Citizen;
using Sandbox.Diagnostics;
using System;
using System.Threading.Channels;

[Category("Ultraneon")]
[Icon( "settings_accessibility" )]
public class Entity : Component, Component.IDamageable, Component.INetworkListener
{
	[Property, ReadOnly] private float maxHealth { get; set; } = 100f;
	[Property,ReadOnly] public bool isAlive { get;private set; } = true;
	[Property,HostSync, Change( nameof( OnDamage ) )] public float health { get; set; }

	public void OnDamage( in DamageInfo damage )
	{
		if ( !isAlive ) return;



		health = Math.Clamp(health - damage.Damage, 0f, maxHealth);

		if ( health <= 0 ) killEntity();
		Log.Info( damage.Attacker );

	}

	protected override void OnEnabled()
	{
		health = maxHealth;
	}
	[Button( "kill player" )]
	public void killEntity()
	{
		health = 0f;
		isAlive = false;
		becomeRagdoll();
	}

	void becomeRagdoll()
	{
		var collider = GameObject.Components.Get<BoxCollider>();
		collider.Enabled = false;
		var ragdoll = GameObject.Components.Get<ModelPhysics>(true);
		ragdoll.Enabled = true;

		if ( GameObject.Tags.Has( "bot" ) )
		{
			GameObject.Components.Get<NavMeshAgent>().Enabled = false;
			GameObject.Tags.Add( "debris" );
		}
	}

}
