using Sandbox;
using System;
using System.Linq;
using Sandbox.Events;
using Ultraneon.Events;

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
		public int SelectedSlot { get; private set; } = 5;

		private const int MaxWeapons = 4;

		protected override void OnFixedUpdate()
		{
			if ( IsProxy ) return;

			HandleWeaponSelection();
			HandleWeaponFiring();
			HandleWeaponReload();
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

		private void HandleWeaponReload()
		{
			if ( Input.Pressed( "reload" ) )
			{
				ActiveWeapon?.StartReload();
			}
		}

		public void SetActive( WeaponBaseNeon weapon )
		{
			if ( weapon == ActiveWeapon ) return;

			Log.Info( $"Switching weapon from {ActiveWeapon?.GameObject.Name ?? "None"} to {weapon?.GameObject.Name ?? "None"}" );

			var oldWeapon = ActiveWeapon;
			oldWeapon?.Holster();
			ActiveWeapon = weapon;

			GameObject.Dispatch( new ActiveWeaponChangedEvent( oldWeapon, ActiveWeapon ) );

			if ( ActiveWeapon == null )
			{
				SelectedSlot = -1;
				Log.Info( "No active weapon. SelectedSlot set to -1" );
				return;
			}

			SelectedSlot = Array.IndexOf( Weapons, ActiveWeapon );
			ActiveWeapon.Equip();

			Log.Info(
				$"New active weapon: {ActiveWeapon.GameObject.Name}, SelectedSlot: {SelectedSlot}, Ammo: {ActiveWeapon.CurrentAmmo}/{ActiveWeapon.ClipSize}" );
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
