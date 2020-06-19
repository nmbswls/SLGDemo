
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;

public delegate void SimpleSceneEventProxy(GameObject go, Vector3 pos);
public delegate void SceneEventProxy(SceneClickData data);
public delegate void SceneSimpleDragProxy(Vector2 delta);

public interface ISceneClickable
{
	void onClick(SceneClickData data);

	void onRightClick(SceneClickData data);

	void onLongClick(Vector3 posInScreenView);

	bool hasClickEvent();

	bool hasLongClickEvent();
}

public interface ISceneDragble
{
	void onDrag(Vector3 posInScreenView);

	void startDrag(Vector3 posDeltaInScreenView);

	void endDrag(Vector3 posInScreenView);
}

public struct SceneClickData
{
	public GameObject Go;
	public float Distance;
	public Vector3 PosInWorld;
	public Vector2 PosInScreen;
}

public class InputManager : MonoBehaviour
{

	
	public enum eSceneEventResult
	{
		Block,
		Continue,
	}

	#region quanjudrag

	private event SceneSimpleDragProxy GlobalDragEvent;

	public void AddGlobalDragCB(SceneSimpleDragProxy cb)
	{
		GlobalDragEvent += cb;
	}

	public void RemoveGlobalDragCB(SceneSimpleDragProxy cb)
	{
		GlobalDragEvent -= cb;
	}
	#endregion



	public static eSceneEventResult FieldEventResult;

	private Camera mCamera;
	

	public static eSceneEventResult s_EventResult;

	public static void BindClickEvent(GameObject target, SceneEventProxy Func)
	{
		SceneClickable listener = target.GetComponent<SceneClickable>();
		if (listener == null)
		{
			listener = target.AddComponent<SceneClickable>();
			listener.ClickEvent += Func;
		}
	}
	public void Start()
	{
		mCamera = Camera.main;
	}

	public void Update()
	{
		CheckClickDown();
		CheckClicking();
		CheckClickUp();
	}
	//拖拽阈值
	public static float dragThreshold = 0.1f;
	public static float longClickThreshold = 0.7f;

	//鼠标按下位置 屏幕坐标系
	private Vector3 mMouseDownPos;

	bool longClickTriggered;
	float mMouseDownTime;
	//上次拖拽时位置
	Vector3 lastDragPos;
	//被点击物体列表
	List<SceneClickData> nowClickedList;

	//鼠标状态
	enum MouseState
	{
		NONE,
		CLICK,
		LONGCLICK,
		DRAG,
		RIGHTCLICK,
	}
	MouseState nowMode = MouseState.NONE;


	//GameObject[] clickedObjs;

	private bool isOnUI()
	{
#if IPHONE || ANDROID
		if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)){
			return true;
		}
		return false;
#else
		if (EventSystem.current.IsPointerOverGameObject())
		{
			return true;
		}
		return false;
#endif
	}

	private void CheckClickDown()
	{	

		//未点击
		if(!Input.GetMouseButtonDown(0) && !((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)))
		{
			return;
		}

		if(isOnUI()){
			return;
		}

		if(nowMode != MouseState.NONE)
		{
			return;
		}

		Ray ray = mCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);

		mMouseDownPos = Input.mousePosition;
		mMouseDownTime = Time.time;
		nowMode = MouseState.CLICK;

		longClickTriggered = false;

		nowClickedList = new List<SceneClickData>();

		if (hits.Length > 1){

			for(int i = 0; i < hits.Length; i++)
			{
				SceneClickData newWrap = new SceneClickData();
				newWrap.Go = hits[i].collider.gameObject;
				newWrap.Distance = hits[i].distance;
				newWrap.PosInWorld = hits[i].point;
				newWrap.PosInScreen = Input.mousePosition;
				nowClickedList.Add(newWrap);
			}

			//从前往后排序
			nowClickedList.Sort((x, y) => {
				return x.Distance.CompareTo(y.Distance);
			});
		}
		else if (hits.Length > 0)
		{
			
			SceneClickData newWrap = new SceneClickData();
			newWrap.Go = hits[0].collider.gameObject;
			newWrap.Distance = hits[0].distance;
			newWrap.PosInWorld = hits[0].point;
			newWrap.PosInScreen = Input.mousePosition;
			nowClickedList.Add(newWrap);
		}
	}

	private void CheckClicking()
	{
		if (!Input.GetMouseButton(0) && !((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)))
		{
			return;
		}
		if (nowMode == MouseState.NONE)
		{
			return;
		}

		
		Vector3 nowPos = Input.mousePosition;
		Vector2 mouseMove = (nowPos - mMouseDownPos);
		if (nowMode == MouseState.CLICK){
			
			if (!longClickTriggered && Time.time - mMouseDownTime > longClickThreshold)
			{
				longClickTriggered = true;
				//当任意原因无法触发回调时，不转化为长按点击
				if (nowClickedList == null || nowClickedList.Count == 0)
				{
					return;
				}
				ISceneClickable cp = nowClickedList[0].Go.GetComponentInParent<ISceneClickable>();
				if(cp == null)
				{
					return;
				}

				if (!cp.hasLongClickEvent())
				{
					return;
				}
				nowClickedList[0].Go.GetComponentInParent<ISceneClickable>().onLongClick(Input.mousePosition);
				nowMode = MouseState.LONGCLICK;
				return;
			}
			//超过阀值 将变为拖动
			if (mouseMove.magnitude < dragThreshold)
				return;

			Debug.Log("switch to drag");
			
			nowMode = MouseState.DRAG;
			lastDragPos = nowPos;

			if (nowClickedList == null || nowClickedList.Count == 0 || nowClickedList[0].Go.GetComponentInParent<ISceneDragble>() == null)
				return;
			//拖拽事件不能传递
			nowClickedList[0].Go.GetComponentInParent<ISceneDragble>().startDrag(lastDragPos);
		}else if (nowMode == MouseState.DRAG){
			Vector3 delta = nowPos - lastDragPos;
			lastDragPos = nowPos;


			if (nowClickedList == null || nowClickedList.Count == 0 || nowClickedList[0].Go.GetComponentInParent<ISceneDragble>() == null)
			{
				if (GlobalDragEvent != null)
				{
					GlobalDragEvent(delta);
				}
				return;
			}
			//拖拽事件不能传递
			nowClickedList[0].Go.GetComponentInParent<ISceneDragble>().onDrag(delta);
		}
	}

	private void CheckClickUp()
	{
		//To do 检测触摸
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}

		
		if (nowMode == MouseState.NONE || nowMode == MouseState.LONGCLICK)
		{
			nowMode = MouseState.NONE;
			return;
		}
		
		if (nowMode == MouseState.CLICK)
		{
			if (nowClickedList == null || nowClickedList.Count == 0 || nowClickedList[0].Go.GetComponentInParent<ISceneClickable>() == null)
			{
				nowMode = MouseState.NONE;
				return;
			}
			for (int i = 0; i < nowClickedList.Count; i++)
			{
				ISceneClickable cp = nowClickedList[i].Go.GetComponentInParent<ISceneClickable>();
				if (cp == null || !cp.hasClickEvent())
				{
					continue;
				}
				if (cp != null && cp.hasClickEvent())
				{
					cp.onClick(nowClickedList[i]);
					if (FieldEventResult == eSceneEventResult.Block)
					{
						break;
					}
				}
			}
			FieldEventResult = eSceneEventResult.Block;
		}
		else if (nowMode == MouseState.DRAG)
		{
			if (nowClickedList == null || nowClickedList.Count == 0 || nowClickedList[0].Go.GetComponentInParent<ISceneDragble>() == null)
			{
				nowMode = MouseState.NONE;
				return;
			}
			nowClickedList[0].Go.GetComponentInParent<ISceneDragble>().endDrag(Input.mousePosition);
		}
		nowMode = MouseState.NONE;
	}
}
