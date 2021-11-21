using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TwitchChatConnect.Client;
using TwitchChatConnect.Config;
using TwitchChatConnect.Data;
using TwitchChatConnect.Manager;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour
{
	enum State
	{
		initializing,
		started,
		preparing,
		executing
	}

	TwitchChatClient twitchChat;

	public GameObject[] tankPrefabs;
	public GameObject[] bulletPrefabs;
	public GameObject shot;
	public GameObject explosion;
	public Color[] colors;

	State state = State.initializing;
	DateTime nextStep = DateTime.MinValue;

	public float preparingSeconds;
	public float explosionDistance;

	public GameObject timerBox;
	public GameObject[] playerBox;
	RectTransform timer;

	readonly ConcurrentQueue<TwitchUser> waiting = new ConcurrentQueue<TwitchUser>();
	readonly ConcurrentDictionary<TwitchUser, Tank> tanks = new ConcurrentDictionary<TwitchUser, Tank>();

	internal static Game GetInstance()
	{
		return GameObject.Find("Game").GetComponent<Game>();
	}

	public void Start()
	{
		var authFilePath = Application.dataPath + Path.DirectorySeparatorChar + "twitch-auth.txt";
		Debug.LogWarning($"Loading twitch authentication from {authFilePath}");

		try
		{
			var (username, token, (channel, _)) = File.ReadAllLines(authFilePath);
			twitchChat = GetComponent<TwitchChatClient>();
			Debug.LogWarning($"Twitch connecting to {channel} as {username}...");
			twitchChat.Init(new TwitchConnectConnect(username, token, channel), () =>
			{
				Debug.LogWarning($"Twitch connected");
				_ = twitchChat.SendChatMessage($"Twitch Tanks ready!", 1);
				twitchChat.onChatCommandReceived += TwitchCommand;
				state = State.started;
			},
			error => Debug.LogError($"Twitch connected error: {error}"));
		}
		catch (Exception ex)
		{
			Debug.LogException(ex);
		}

		timer = timerBox.transform.Find("ProgressBackground").Find("Progress").GetComponent<RectTransform>();
		timerBox.SetActive(false);

		Debug.LogWarning("TwitchTansk started");
	}

	void TwitchCommand(TwitchChatCommand command)
	{
		var user = command.User;
		var args = new List<string>(command.Message.Split(' '));
		var msg = args[0].ToLower();
		args.RemoveAt(0);
		var tank = GetTank(user);
		// Debug.LogWarning($"[{msg}] [{command.Message}]");
		switch (msg)
		{
			case "!about":
				_ = twitchChat.SendChatMessage("Instructions: Write \"!join\" to enter the battle (or the queue if area has already 4 tanks). When in control of a tank write \"!move +/-N\" to move your tank N units forwards/backwards or \"!turn +/-D\" to rotate your tank D degrees. Write \"!shoot N\" to shoot at distance N. Tanks execute in turns and only ever with max 1 move and max 1 turn command", 1);
				break;
			case "!join":
				if (tank != null)
				{
					_ = twitchChat.SendChatMessage($"You are already in the game", 1);
					break;
				}
				if (waiting.Contains(user))
				{
					var waitingUsers = waiting.ToArray();
					for (var i = 0; i < waitingUsers.Length; i++)
						if (waitingUsers[i] == user)
							_ = twitchChat.SendChatMessage($"@{user.Username} You are already #{i + 1} in the tank queue", 1);
				}
				else
				{
					if (waiting.Count < 4)
						tanks[user] = MakeTank(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
					else
					{
						waiting.Enqueue(user);
						_ = twitchChat.SendChatMessage($"@{user.Username} You are now #{waiting.Count} in the tank queue", 1);
					}
				}
				break;
			case "!move":
				if (tank == null)
				{
					_ = twitchChat.SendChatMessage($"Please !join first and wait for your turn", 1);
					break;
				}
				if (args.Count != 1 || float.TryParse(args[0], out var moveDistance) == false)
				{
					_ = twitchChat.SendChatMessage($"Use \"!move N\" where N is a number", 1);
					break;
				}
				tank.Move(moveDistance);
				break;
			case "!turn":
				if (tank == null)
				{
					_ = twitchChat.SendChatMessage($"Please !join first and wait for your turn", 1);
					break;
				}
				if (args.Count != 1 || float.TryParse(args[0], out var angle) == false)
				{
					_ = twitchChat.SendChatMessage($"Use \"!turn D\" where D is in degrees", 1);
					break;
				}
				tank.Rotate(angle);
				break;
			case "!shoot":
				if (tank == null)
				{
					_ = twitchChat.SendChatMessage($"Please !join first and wait for your turn", 1);
					break;
				}
				if (args.Count != 1 || float.TryParse(args[0], out var shootDistance) == false)
				{
					_ = twitchChat.SendChatMessage($"Use \"!shoot N\" where N is a number", 1);
					break;
				}
				tank.Shoot(shootDistance);
				break;
			default:
				_ = twitchChat.SendChatMessage($"Unknown command \"{msg}\". Valid: !about !join !move !turn !shoot", 1);
				break;
		}
	}

	public void Update()
	{
		if (state != State.initializing && twitchChat.IsConnected() == false)
		{
			Debug.LogWarning("Reconnecting...");
			twitchChat.Login();
		}

		tanks
			.Where(pair => TwitchUserManager.HasUser(pair.Key.Username) == false)
			.Select(pair => pair.Value).ToList()
			.ForEach(tank => tanks.RemoveByValue(tank));

		while (tanks.Count < 4 && waiting.Count > 0)
		{
			if (waiting.TryDequeue(out var user))
			{
				_ = twitchChat.SendChatMessage($"@{user.Username} You have entered the tank battle!", 1);
				tanks[user] = MakeTank(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
			}
		}

		var pairs = tanks.ToArray();
		for (var i = 0; i < 4; i++)
		{
			var box = playerBox[i];
			if (i >= pairs.Length)
				box.SetActive(false);
			else
			{
				box.GetComponent<Image>().color = colors[pairs[i].Value.idx];
				box.transform.Find("Name").GetComponent<Text>().text = pairs[i].Key.Username;
				box.SetActive(true);
			}
		}

		switch (state)
		{
			case State.started:
				if (tanks.Count > 0)
					StartNextRound();
				break;
			case State.preparing:
				var delta = nextStep.Subtract(DateTime.Now);
				if (delta > TimeSpan.Zero)
				{
					var f = Mathf.Clamp01((float)delta.TotalSeconds / preparingSeconds);
					timer.sizeDelta = new Vector2(f, 1f);
				}
				else
				{
					timer.sizeDelta = new Vector2(0f, 1f);
					_ = StartCoroutine(ExecuteTankCommands());
					state = State.executing;
				}
				break;
			case State.executing:
				break;
		}

		// debugging
		if (tanks.Count > 0)
		{
			if (Input.GetKeyDown(KeyCode.W)) tanks.Take(1).First().Value.Move(5);
			if (Input.GetKeyDown(KeyCode.S)) tanks.Take(1).First().Value.Move(-5);
			if (Input.GetKeyDown(KeyCode.A)) tanks.Take(1).First().Value.Rotate(-45);
			if (Input.GetKeyDown(KeyCode.D)) tanks.Take(1).First().Value.Rotate(45);
			if (Input.GetKeyDown(KeyCode.Space)) tanks.Take(1).First().Value.Shoot(50);
		}
	}

	void StartNextRound()
	{
		if (tanks.Count == 0)
		{
			nextStep = DateTime.MinValue;
			state = State.started;
			timerBox.SetActive(false);
			return;
		}

		timerBox.SetActive(true);
		nextStep = DateTime.Now.AddSeconds(preparingSeconds);
		state = State.preparing;
	}

	IEnumerator ExecuteTankCommands()
	{
		var n = 0;
		foreach (var pairs in tanks)
			pairs.Value.ExecuteCommands(() => n++);
		while (n < tanks.Count)
			yield return null;
		StartNextRound();
	}

	Tank GetTank(TwitchUser user)
	{
		if (tanks.TryGetValue(user, out var tank))
			return tank;
		return null;
	}

	Tank MakeTank(float x, float z)
	{
		var availIndices = new List<int>() { 0, 1, 2, 3 };
		foreach (var pair in tanks) availIndices.RemoveByValue(pair.Value.idx);
		var idx = availIndices.RandomElement();
		var angle = Random.Range(0f, 360f);
		var obj = Instantiate(tankPrefabs[idx], new Vector3(x * 77f, 1.45f, z * 41f), Quaternion.Euler(0f, angle, 0f));
		obj.transform.localScale = new Vector3(2, 2, 2);
		var tank = obj.GetComponent<Tank>();
		tank.idx = idx;
		return tank;
	}

	internal void MakeBullet(int idx, Vector3 start, float angle, float distance)
	{
		var shotInstance = Instantiate(shot, start, Quaternion.Euler(0, Random.Range(0, 360), 0));
		shotInstance.transform.localScale = new Vector3(2, 2, 2);

		var quat = Quaternion.Euler(0f, angle, 0f);
		var obj = Instantiate(bulletPrefabs[idx], start, quat);
		obj.transform.localScale = new Vector3(2, 2, 2);
		var bullet = obj.GetComponent<Bullet>();
		bullet.idx = idx;
		distance /= 3f;
		bullet.destination = start + quat * (Vector3.forward * distance);
		bullet.distance = distance;
	}

	internal void Explode(Vector3 position)
	{
		var obj = Instantiate(explosion, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
		obj.transform.localScale = new Vector3(3, 3, 3);
		var tanksCopy = new List<Tank>(tanks.Values);
		foreach (var tank in tanksCopy)
		{
			var tankPos = tank.gameObject.transform.position;
			if ((tankPos - position).magnitude <= explosionDistance)
			{
				var user = tanks.First(pair => pair.Value == tank).Key;
				tanks.RemoveByValue(tank);
				Destroy(tank.gameObject);
				_ = Instantiate(explosion, tankPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
				_ = twitchChat.SendChatMessage($"@{user.Username} You have been destroyed!", 1);
			}
		}
	}
}
