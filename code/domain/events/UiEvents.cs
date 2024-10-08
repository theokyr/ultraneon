﻿using Sandbox.Events;

namespace Ultraneon.Domain.Events;

public enum MenuAction
{
	Play,
	Settings,
	Quit
}

public enum UiInfoFeedType
{
	Sticky,
	Normal,
	Warning,
	Success,
	Debug
}

public record UiInfoFeedEvent( string Message, UiInfoFeedType Type ) : IGameEvent;
