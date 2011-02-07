namespace MediaServer.SSDP.Messages
{
	public enum SearchType
	{
		Unknown = -1,
		All = 0,
		RootDevice = 1,
		DeviceId = 2,
		DeviceType = 3,
		ServiceType = 4
	}

	public enum NotificationType
	{
		Unknown = -1,
		RootDevice = 1,
		DeviceId = 2,
		DeviceType = 3,
		ServiceType = 4
	}

	public enum NotificationSubType
	{
		Alive,
		ByeBye
	}
}
