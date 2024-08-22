using Sandbox;
using System;
using System.Threading.Tasks;
using Sandbox.Events;

namespace Ultraneon
{
	public class PlayerNeon : BaseNeonCharacterEntity
	{
		[Sync]
		public Team CurrentTeam { get; set; } = Team.Player;

		[Property]
		public float CaptureTime { get; set; } = 15f;

		[Sync]
		public float CurrentCaptureProgress { get; private set; } = 0f;

		[Property]
		public float HealInterval { get; set; } = 5f;

		[Property]
		public float HealAmount { get; set; } = 10f;

		[Property]
		public float RespawnDelay { get; set; } = 5f;

		private TimeSince TimeSinceLastHeal { get; set; }
		private TimeSince TimeSinceDeath { get; set; }

		[RequireComponent]
		public PlayerInventory Inventory { get; private set; }

		public bool IsDead => Health <= 0;

		protected override void OnStart()
		{
			base.OnStart();
			if ( IsProxy ) return;

			Health = MaxHealth;
			Inventory = Components.Get<PlayerInventory>();
		}

		protected override void OnUpdate()
		{
			if ( IsProxy || IsDead ) return;

			HandleInput();
			TryHeal();
			UpdateCapture();
		}

		public new void OnDamage( DamageInfo info )
		{
			if ( IsProxy || IsDead ) return;

			if ( info.Attacker.Components.Get<Entity>() is { } entity )
			{
				GameObject.Dispatch( new DamageEvent( this, entity, info.Damage, info.Position ) );
			}
		}

		private void HandleInput()
		{
			// TODO: Implement input handling
		}

		private void TryHeal()
		{
			if ( TimeSinceLastHeal >= HealInterval && IsInCapturedZone() )
			{
				Health = Math.Min( Health + HealAmount, MaxHealth );
				TimeSinceLastHeal = 0;
			}
		}

		private void UpdateCapture()
		{
			if ( IsInCaptureZone() )
			{
				CurrentCaptureProgress += Time.Delta;
				if ( CurrentCaptureProgress >= CaptureTime )
				{
					CompleteCaptureZone();
				}
			}
			else
			{
				CurrentCaptureProgress = Math.Max( 0, CurrentCaptureProgress - Time.Delta );
			}
		}

		private bool IsInCapturedZone()
		{
			// TODO: Implement logic to check if the player is in a captured zone
			return false;
		}

		private bool IsInCaptureZone()
		{
			// TODO: Implement logic to check if the player is in a capturable zone
			return false;
		}

		private void CompleteCaptureZone()
		{
			// TODO: Implement zone capture logic
		}

		public void TakeDamage( DamageInfo info )
		{
			if ( IsProxy || IsDead ) return;

			float damageAmount = CalculateDamage( info );
			Health -= damageAmount;

			if ( Health <= 0 )
			{
				Die( info.Attacker.Components.Get<PlayerNeon>() );
			}
		}

		private float CalculateDamage( DamageInfo info )
		{
			float damage = info.Damage;

			// TODO: Wallbang hit reduction
			// if (info.Flags.HasFlag(DamageFlags.Wallbang))
			// {
			//     damage *= 0.5f;
			// }

			return damage;
		}

		private void Die( PlayerNeon killer )
		{
			bool isStylishKill = IsStylishKill( killer );
			GameObject.Dispatch( new CharacterDeathEvent( this, killer, isStylishKill ) );

			if ( HasCapturedZone() )
			{
				TimeSinceDeath = 0;
				EnableRespawnState();
			}
			else
			{
				HandlePermanentDeath();
			}
		}

		private bool HasCapturedZone()
		{
			// TODO: Implement logic to check if the player has any captured zones
			return false;
		}

		private void EnableRespawnState()
		{
			// TODO: Disable player controls and show respawn UI
		}

		private void HandlePermanentDeath()
		{
			// TODO: Implement permanent death logic and show game over UI
		}

		private bool IsStylishKill( PlayerNeon killer )
		{
			// TODO: Implement logic for determining if it's a stylish kill (airborne, wallbang)
			return false;
		}

		public void Respawn( Vector3 position )
		{
			Health = MaxHealth;
			Transform.Position = position;
			EnablePlayerControls();
		}

		private void EnablePlayerControls()
		{
			// TODO: Re-enable player controls after respawn
		}
	}
}
