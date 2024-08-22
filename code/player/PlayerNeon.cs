using Sandbox;
using System;
using System.Threading.Tasks;

namespace Ultraneon
{
	public class PlayerNeon : Entity
	{
		[Property]
		public float MaxHealth { get; set; } = 100f;

		[Property, HostSync, Change( nameof(OnDamage) )]
		public float Health { get; set; }

		[Sync]
		public Team CurrentTeam { get; set; } = Team.Player;

		[Sync]
		public int Score { get; private set; } = 0;

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

		private void HandleInput()
		{
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
			AddScore( 100 ); // Score for capturing a grey zone
		}

		public void TakeDamage( DamageInfo info )
		{
			if ( IsProxy || IsDead ) return;

			float damageAmount = CalculateDamage( info );
			Health -= damageAmount;

			if ( Health <= 0 )
			{
				// TODO: Kill me
				// Die( info.Attacker.Components.FirstOrDefault<Entity>( x => x ) );
			}
		}

		private float CalculateDamage( DamageInfo info )
		{
			float damage = info.Damage;

			// TODO: Handle wallbangs here with DamageFlags
			// if ( info.Flags.HasFlag( DamageFlags.Wallbang ) )
			// {
			// 	damage *= 0.5f; // 50% damage reduction for wallbangs
			// }

			return damage;
		}

		private void Die( PlayerNeon killer )
		{
			if ( HasCapturedZone() )
			{
				TimeSinceDeath = 0;
				EnableRespawnState();
			}
			else
			{
				HandlePermanentDeath();
			}

			if ( killer != null )
			{
				int scoreAward = 10; // Base score for a kill
				if ( killer.IsStylishKill( this ) )
				{
					scoreAward = 40; // Score for a stylish kill
				}

				killer.AddScore( scoreAward );
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

		private bool IsStylishKill( PlayerNeon victim )
		{
			// TODO: Implement logic for determining if it's a stylish kill (airborne, wallbang)
			return false;
		}

		private Vector3 GetRespawnPosition()
		{
			// TODO: Implement logic to get the position of a captured zone or a default spawn point
			return Vector3.Zero;
		}

		private void EnablePlayerControls()
		{
			// TODO: Re-enable player controls after respawn
		}

		public void AddScore( int amount )
		{
			Score = Math.Max( 0, Score + amount );
		}
	}
}
