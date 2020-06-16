using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneClickable : MonoBehaviour, SceneClickableActor
{
	// 定义事件代理

	public delegate void SimpleSceneEventProxy(GameObject go, Vector3 pos);
	public delegate void SceneEventProxy(SceneClickData data);

	// 鼠标点击事件
	public event SceneEventProxy ClickEvent;

	public event SimpleSceneEventProxy LongClickEvent;
	//拖动
	public event SimpleSceneEventProxy BeginDragEvent;

	public event SimpleSceneEventProxy OnDragEvent;

	public event SimpleSceneEventProxy EndDragEvent;

	public void onClick(SceneClickData data)
	{
		if (interval > 0)
			return;
		if (ClickEvent != null)
			ClickEvent(data);
	}

	public void onLongClick(Vector3 pos)
	{
		if (interval > 0)
			return;
		if (LongClickEvent != null)
			LongClickEvent(this.gameObject, pos);
	}

	public void onRightClick(SceneClickData data)
	{
		if (interval > 0)
			return;
		if (ClickEvent != null)
			ClickEvent(data);
	}

	public void startDrag(Vector3 pos)
	{
		if (interval > 0)
			return;
		if (BeginDragEvent != null)
			BeginDragEvent(this.gameObject, pos);
	}

	public void onDrag(Vector3 pos)
	{
		if (interval > 0)
			return;
		if (OnDragEvent != null)
			OnDragEvent(this.gameObject, pos);
	}

	public void endDrag(Vector3 pos)
	{
		if (interval > 0)
			return;
		if (EndDragEvent != null)
		{
			EndDragEvent(this.gameObject, pos);
		}
	}

	public float interval = 0.1f;
	void Update()
	{
		if (interval > 0)
		{
			interval -= Time.unscaledDeltaTime;
		}
	}

	public bool hasClickEvent()
	{
		if (ClickEvent != null)
		{
			return ClickEvent.GetInvocationList().Length > 0;
		}
		else
		{
			return false;
		}
	}
	public bool hasLongClickEvent()
	{
		if (LongClickEvent != null)
		{
			return LongClickEvent.GetInvocationList().Length > 0;
		}
		else
		{
			return false;
		}
	}


}