namespace Ultraneon;

public enum Team
{
	Neutral = 0,
	Player = 1,
	Enemy = 2
}

public static class Teams
{
	public static Team GetTeamFromTag( string tag = null )
	{
		if ( tag == null )
		{
			return Team.Neutral;
		}

		return tag switch
		{
			"player" => Team.Player,
			"bot" => Team.Enemy,
			_ => Team.Neutral
		};
	}
}
