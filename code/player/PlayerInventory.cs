using Sandbox;
using System;
using System.Linq;

namespace Ultraneon
{
	[Category( "Ultraneon" )]
	[Icon( "backpack" )]
	public sealed class PlayerInventory : Component
	{
		[Property, Sync]
		public WeaponBaseNeon ActiveWeapon { get; private set; }

		[Property, Sync]
		public WeaponBaseNeon[] Weapons { get; private set; } = new WeaponBaseNeon[MaxWeapons];

		[Property, Sync]
		public int SelectedSlot { get; private set; } = 0;

		private const int MaxWeapons = 4;

		protected override void OnFixedUpdate()
		{
			if ( IsProxy ) return;

			HandleWeaponSelection();
			HandleWeaponFiring();
		}

		private void HandleWeaponSelection()
		{
			for ( int i = 0; i < MaxWeapons; i++ )
			{
				if ( Input.Pressed( $"Slot{i + 1}" ) )
				{
					SetActive( Weapons[i] );
					return;
				}
			}

			if ( Input.Pressed( "Slot5" ) ) SetActive( null );

			HandleWeaponScroll();
		}

		private void HandleWeaponScroll()
		{
			if ( Input.MouseWheel.Length == 0 || Weapons.Count( x => x != null ) <= 1 ) return;

			int delta = Math.Sign( Input.MouseWheel.y );
			int newSlot = SelectedSlot;

			do
			{
				newSlot = (newSlot - delta + MaxWeapons) % MaxWeapons;
			} while ( Weapons[newSlot] == null && newSlot != SelectedSlot );

			if ( newSlot != SelectedSlot )
			{
				SetActive( Weapons[newSlot] );
			}
		}

		private void HandleWeaponFiring()
		{
			if ( Input.Pressed( "attack1" ) )
			{
				ActiveWeapon?.Shoot();
			}
			else if ( Input.Down( "attack1" ) && ActiveWeapon != null && !ActiveWeapon.IsSemiAuto )
			{
				ActiveWeapon.Shoot();
			}
		}

		public void SetActive( WeaponBaseNeon weapon )
		{
			if ( weapon == ActiveWeapon ) return;

			ActiveWeapon?.Holster();
			ActiveWeapon = weapon;

			if ( ActiveWeapon != null )
			{
				SelectedSlot = Array.IndexOf( Weapons, ActiveWeapon );
				ActiveWeapon.Equip();
			}
		}

		public bool AddWeapon( WeaponBaseNeon weapon )
		{
			int slot = (int)weapon.WeaponType;
			if ( Weapons[slot] == null )
			{
				Weapons[slot] = weapon;
				return true;
			}

			return false;
		}

		public void RemoveWeapon( WeaponBaseNeon weapon )
		{
			int index = Array.IndexOf( Weapons, weapon );
			if ( index != -1 )
			{
				Weapons[index] = null;
				if ( ActiveWeapon == weapon )
				{
					SetActive( Weapons.FirstOrDefault( w => w != null ) );
				}
			}
		}
	}
}
