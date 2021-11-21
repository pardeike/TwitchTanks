using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tank : MonoBehaviour
{
	public enum StepType
	{
		move,
		rotate,
		shoot
	}

	public class Step
	{
		public StepType type;
		public float[] parameters;
	}

	float desiredMove;
	float angle;
	float desiredAngle;
	float shootDelay;
	List<Step> steps = new List<Step>();
	internal int idx;

	internal float move = 0f;
	internal float rotate = 0f;
	internal string lastCommand = "";
	internal float shoot = 0f;

	public void Start()
	{
		angle = gameObject.transform.eulerAngles.y;
		desiredAngle = angle;
	}

	public void Update()
	{
		if (desiredMove != 0f)
		{
			var sign = Mathf.Sign(desiredMove);
			var direction = gameObject.transform.rotation * Vector3.forward;
			gameObject.transform.position += sign * direction / 5f;
			desiredMove -= sign * 0.5f;
			if (sign > 0 && desiredMove < 0) desiredMove = 0f;
			if (sign < 0 && desiredMove > 0) desiredMove = 0f;
		}

		if (angle > desiredAngle && angle - desiredAngle > 180)
			desiredAngle += 360;
		if (Mathf.Abs(angle - desiredAngle) < 1f)
			angle = desiredAngle;
		else
			angle += (desiredAngle - angle) / 20f;
		gameObject.transform.rotation = Quaternion.Euler(0f, angle, 0f);

		if (shootDelay > 0f) shootDelay -= 0.1f;
		if (shootDelay < 0f) shootDelay = 0f;
	}

	internal bool IsStatic() => desiredMove == 0f && angle == desiredAngle && shootDelay == 0f;

	internal void ExecuteCommands(Action completionCallback)
	{
		_ = StartCoroutine(Stepper(completionCallback));
	}

	IEnumerator Stepper(Action completionCallback)
	{
		yield return null;
		while (true)
		{
			var step = steps.PopFirst();
			if (step == null) break;
			switch (step.type)
			{
				case StepType.move:
					desiredMove = step.parameters[0];
					// Debug.LogWarning($"#{idx} Tank move {desiredMove}");
					yield return null;
					break;
				case StepType.rotate:
					desiredAngle = angle + step.parameters[0];
					// Debug.LogWarning($"#{idx} Tank turn {desiredAngle}");
					yield return null;
					break;
				case StepType.shoot:
					var delta = gameObject.transform.rotation * new Vector3(0f, 0f, 4f);
					var start = gameObject.transform.position + delta;
					start.y = 3.5f;
					var shootDistance = step.parameters[0];
					// Debug.LogWarning($"#{idx} Tank shoot {shootDistance}");
					Game.GetInstance().MakeBullet(idx, start, gameObject.transform.eulerAngles.y, shootDistance);
					shootDelay = 5;
					yield return null;
					break;
			}
			// Debug.LogWarning($"#{idx} waiting");
			while (IsStatic() == false)
				yield return null;
			// Debug.LogWarning($"#{idx} done");
		}
		completionCallback();
	}

	void SetStep(StepType type, params float[] parameters)
	{
		steps = steps.Where(step => step.type != type).ToList();
		steps.Add(new Step() { type = type, parameters = parameters });
	}

	internal void Move(float distance)
	{
		SetStep(StepType.move, distance);
	}

	internal void Rotate(float angle)
	{
		SetStep(StepType.rotate, angle);
	}

	internal void Shoot(float distance)
	{
		SetStep(StepType.shoot, distance);
	}
}
