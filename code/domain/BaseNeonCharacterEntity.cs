﻿using System;
using Sandbox.Events;
using Ultraneon.Domain.Events;

namespace Ultraneon.Domain;

[Category( "Ultraneon" )]
[Icon( "settings_accessibility" )]
public class BaseNeonCharacterEntity : Entity, Component.INetworkListener
{
	[Property, ReadOnly]
	public float MaxHealth { get; set; } = 100f;

	[Property, HostSync, Change( nameof(OnDamage) )]
	public float Health { get; set; }

	[Property, ReadOnly]
	public bool IsAlive { get; protected set; } = true;

	[Property]
	public Team CurrentTeam { get; set; } = Team.Neutral;

	public override void OnDamage( in DamageInfo damage )
	{
		if ( !IsAlive ) return;
		Health = Math.Clamp( Health - damage.Damage, 0f, MaxHealth );

		if ( Health <= 0 ) KillCharacter( damage.Attacker );
		Log.Info( $"{EntityName} took {damage.Damage} damage from {damage.Attacker?.Name ?? "unknown"}" );
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();
		Health = MaxHealth;
	}

	[Button( "Kill Character" )]
	public void KillCharacter( GameObject attacker = null )
	{
		Health = 0f;
		IsAlive = false;
		BecomeRagdoll();

		var baseNeonCharacterEntity = Components.Get<Entity>();
		if ( baseNeonCharacterEntity != null )
		{
			var killer = attacker?.Components.Get<Entity>();
			// bool isStylishKill = IsStylishKill( killer ); todo
			GameObject.Dispatch( new CharacterDeathEvent( this, killer, false ) );
		}
	}

	void BecomeRagdoll()
	{
		var collider = GameObject.Components.Get<BoxCollider>();
		if ( collider == null )
		{
			return;
		}

		collider.Enabled = false;
		var ragdoll = GameObject.Components.Get<ModelPhysics>( true );
		if ( ragdoll == null )
		{
			return;
		}

		ragdoll.Enabled = true;
	}

	private bool IsStylishKill( BaseNeonCharacterEntity killer )
	{
		// TODO: Implement logic for determining if it's a stylish kill (airborne, wallbang)
		return false;
	}
}
