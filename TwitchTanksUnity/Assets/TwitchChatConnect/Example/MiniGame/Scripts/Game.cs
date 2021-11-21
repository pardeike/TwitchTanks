namespace TwitchChatConnect.Example.MiniGame
{
	/*
    public class Game : MonoBehaviour
    {
        private static string START_COMMAND = "!start";
        private static string MOVE_COMMAND = "!move";

        private Dictionary<string, Vector3> directions;

        void Start()
        {
            directions = new Dictionary<string, Vector3>();
            directions.Add("up", Vector3.forward);
            directions.Add("down", Vector3.back);
            directions.Add("right", Vector3.right);
            directions.Add("left", Vector3.left);
            
            TwitchChatClient.Instance.Init(() =>
                {
                    TwitchChatClient.Instance.onChatMessageReceived += OnChatMessageReceived;
                    TwitchChatClient.Instance.onChatCommandReceived += OnChatCommandReceived;
                    TwitchChatClient.Instance.onChatRewardReceived += OnChatRewardReceived;

                    MatchManager.instance.onMatchEnd += OnMatchEnd;
                    MatchManager.instance.onMatchBegin += OnMatchBegin;
                },
                message =>
                {
                    // Error when initializing.
                    Debug.LogError(message);
                });
        }

        void OnChatCommandReceived(TwitchChatCommand chatCommand)
        {
            if (chatCommand.Command == START_COMMAND)
            {
                if (MatchManager.instance.HasStarted) return;
                MatchManager.instance.Begin();
                return;
            }

            if (!MatchManager.instance.HasStarted) return;

            if (chatCommand.Command == MOVE_COMMAND)
            {
                string parameter = chatCommand.Parameters[0];
                if (directions.ContainsKey(parameter))
                {
                    Vector3 direction = directions[parameter];
                    GameUI.instance.UpdateUser(chatCommand.User);
                    MatchManager.instance.Move(direction);
                }
                return;
            }

            Debug.Log($"Unknown Command received: {chatCommand.Command}");
        }

        void OnChatRewardReceived(TwitchChatReward chatReward)
        {
        }

        void OnChatMessageReceived(TwitchChatMessage chatMessage)
        {
        }

        void OnMatchBegin()
        {
            TwitchChatClient.Instance.SendChatMessage("A new game has started");
        }

        void OnMatchEnd(float secondsElapsed)
        {
            TwitchChatClient.Instance.SendChatMessage("---------------");
            TwitchChatClient.Instance.SendChatMessage($"The game has ended, it took {secondsElapsed} seconds.");
            foreach (KeyValuePair<TwitchUser,UserInfo> user in GameUI.instance.Users)
            {
                TwitchChatClient.Instance.SendChatMessage(user.Value.GetText());
            }
            TwitchChatClient.Instance.SendChatMessage("---------------");
            GameUI.instance.Reset();
        }
    }
   */
}
