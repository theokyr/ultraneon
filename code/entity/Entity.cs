using Sandbox;
using System;

namespace Ultraneon
{
	public class Entity : Component, Component.IDamageable
	{
		[Property]
		public Guid Id { get; private set; }

		[Property]
		public string EntityName { get; set; }

		public bool IsActive { get; private set; } = true;

		protected override void OnAwake()
		{
			base.OnAwake();
			Id = Guid.NewGuid();
		}

		protected override void OnStart()
		{
			base.OnStart();
			if ( string.IsNullOrEmpty( EntityName ) )
			{
				EntityName = GetType().Name;
			}
		}

		public virtual void Activate()
		{
			IsActive = true;
		}

		public virtual void Deactivate()
		{
			IsActive = false;
		}

		public virtual void OnCollision( Entity other )
		{
		}

		public void OnDamage( in DamageInfo damageInfo )
		{
			throw new NotImplementedException();
		}

		public virtual void Destroy()
		{
			GameObject.Destroy();
		}

		public float DistanceTo( Entity other )
		{
			return Vector3.DistanceBetween( Transform.Position, other.Transform.Position );
		}

		public Vector3 DirectionTo( Entity other )
		{
			return (other.Transform.Position - Transform.Position).Normal;
		}
	}
}
