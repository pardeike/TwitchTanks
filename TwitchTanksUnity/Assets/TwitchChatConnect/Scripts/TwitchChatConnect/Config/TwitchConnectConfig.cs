using System;
using UnityEngine;

namespace TwitchChatConnect.Config
{
	[Serializable]
	public class TwitchConnectConnect
	{
		[SerializeField] private readonly string username;
		[SerializeField] private readonly string userToken;
		[SerializeField] private readonly string channelName;

		public string Username => username?.ToLower() ?? null;
		public string UserToken => userToken;
		public string ChannelName => channelName;

		public TwitchConnectConnect(string username, string userToken, string channelName)
		{
			this.username = username;
			this.userToken = userToken;
			this.channelName = channelName;
		}

		public bool IsValid()
		{
			return !String.IsNullOrEmpty(Username) &&
					 !String.IsNullOrEmpty(UserToken) &&
					 !String.IsNullOrEmpty(ChannelName);
		}
	}
}
