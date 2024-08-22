using Sandbox.Events;

namespace Ultraneon.Events;

// Weapons (used to update ui)
public record WeaponStateChangedEvent( WeaponBaseNeon Weapon ) : IGameEvent;

public record ActiveWeaponChangedEvent( WeaponBaseNeon OldWeapon, WeaponBaseNeon NewWeapon ) : IGameEvent;

// Zone Capture Stuff
public record CaptureZoneEvent( string ZoneName, Team PreviousTeam, Team NewTeam ) : IGameEvent;

// Game Events
public record PlayerSpawnEvent( Team Team ) : IGameEvent;

public record DamageEvent( BaseNeonCharacterEntity Target, Entity Attacker, float Damage, Vector3 Position ) : IGameEvent;

public record CharacterDeathEvent( BaseNeonCharacterEntity Victim, Entity Killer, bool IsStylish ) : IGameEvent;
