
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface SceneClickableActor
{
	void onClick(Vector3 posInScreenView);

	void onDrag(Vector3 posInScreenView);

	void startDrag(Vector3 posDeltaInScreenView);

	void endDrag(Vector3 posInScreenView);
}

public class InputManager
{

	public enum eSceneEventResult
	{
		Block,
		Continue,
	}

	

	private Camera mCamera;
	

	public static eSceneEventResult s_EventResult;
	public Camera m_camera;

	public static void BindClickEvent(GameObject target, ClickableEventlistener2D.FieldEventProxy Func)
	{
		ClickableEventlistener2D listener = target.GetComponent<ClickableEventlistener2D>();
		if (listener == null)
		{
			listener = target.AddComponent<ClickableEventlistener2D>();
			listener.ClickEvent += Func;
		}
	}
	public InputManager(Camera camera)
	{
		mCamera = camera;
	}

	public void Update()
	{
		CheckClickDown();
		CheckClicking();
		CheckClickUp();
	}
	//拖拽阈值
	public static float dragThreshold = 0.1f;

	//鼠标按下位置 屏幕坐标系
	private Vector3 mMouseDownPos;
	//上次拖拽时位置
	Vector3 lastDragPos;
	//被点击物体列表
	List<GameObject> nowClickedList;

	//鼠标状态
	enum MouseState
	{
		NONE,
		CLICK,
		DRAG,
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
		//Debug.Log(Stage.inst.touchTarget.gameObject.name);	
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

		if(!isOnUI()){
			return;
		}

		if(nowMode != MouseState.NONE)
		{
			return;
		}

		Ray ray = mCamera.ScreenPointToRay(Input.mousePosition);
		RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
		if (hits.Length > 1){
			mMouseDownPos = Input.mousePosition;
			nowClickedList = new List<GameObject>();
			List<RaycastHit> hitsList = new List<RaycastHit>(hits);
			//从前往后排序
			hitsList.Sort((x, y) => {
				return x.distance.CompareTo(y.distance);
			});
			for (int i = 0; i < hits.Length; i++)
			{
				nowClickedList.Add(hits[i].collider.gameObject);
			}
			nowMode = MouseState.CLICK;
		}
		else if (hits.Length > 0)
		{
			mMouseDownPos = Input.mousePosition;
			nowClickedList = new List<GameObject>() { hits[0].collider.gameObject };
			nowMode = MouseState.CLICK;
		}
	}

	private void CheckClicking()
	{
		if (!Input.GetMouseButtonDown(0) && !((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)))
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
			//超过阀值 将变为拖动
			if (mouseMove.magnitude < dragThreshold)
				return;
			nowMode = MouseState.DRAG;
			lastDragPos = nowPos;

			if (nowClickedList == null || nowClickedList.Count == 0 || nowClickedList[0].GetComponentInParent<ClickableEventlistener2D>() == null)
				return;

			//拖拽事件不能传递
			nowClickedList[0].GetComponentInParent<SceneClickableActor>().startDrag(lastDragPos);
		}else if (nowMode == MouseState.DRAG){
			Vector3 delta = nowPos - lastDragPos;
			lastDragPos = nowPos;
			if (nowClickedList == null || nowClickedList.Count == 0 || nowClickedList[0].GetComponentInParent<SceneClickableActor>() == null)
				return;
			//拖拽事件不能传递
			nowClickedList[0].GetComponentInParent<ClickableEventlistener2D>().onDrag(delta);
		}
	}

	private void CheckClickUp()
	{
		//To do 检测触摸
		if (!Input.GetMouseButtonUp(0))
		{
			return;
		}

		//if (Input.GetMouseButtonUp(0))
		//{
		//	if (nowMode == MouseState.NONE)
		//	{

		//	}
		//	else if (nowMode == MouseState.CLICK)
		//	{
		//		if (nowClickedList == null || nowClickedList.Count == 0 || nowClickedList[0].GetComponentInParent<ClickableSprite>() == null)
		//		{
		//			//无法回调

		//		}
		//		else
		//		{

		//			{
		//				for (int i = 0; i < nowClickedList.Count; i++)
		//				{
		//					ClickableEventlistener2D cp = nowClickedList[i].GetComponentInParent<ClickableEventlistener2D>();
		//					if (cp == null || !cp.hasClickEvent())
		//					{
		//						continue;
		//					}
		//					if (cp != null && cp.hasClickEvent())
		//					{
		//						cp.onClick(Input.mousePosition);
		//						if (FieldEventResult == eFieldEventResult.Block)
		//						{
		//							break;
		//						}
		//					}
		//				}

		//			}
		//			FieldEventResult = eFieldEventResult.Block;


		//		}
		//	}
		//	else if (nowMode == MouseState.DRAG)
		//	{
		//		if (nowClickedList == null || nowClickedList.Count == 0 || nowClickedList[0].GetComponentInParent<SceneClickableActor>() == null)
		//		{
		//			//无法回调
		//		}
		//		else
		//		{
		//			nowClickedList[0].GetComponentInParent<SceneClickableActor>().endDrag(Input.mousePosition);
		//		}
		//	}
		//	nowMode = MouseState.NONE;
		//}
	}
}
public class ClickableEventlistener2D : MonoBehaviour, SceneClickableActor
{
	// 定义事件代理
	public delegate void FieldEventProxy(GameObject go, Vector3 pos);

	// 鼠标点击事件
	public event FieldEventProxy ClickEvent;
	//拖动
	public event FieldEventProxy BeginDragEvent;

	public event FieldEventProxy OnDragEvent;

	public event FieldEventProxy EndDragEvent;

	public void onClick(Vector3 pos)
	{
		if (interval > 0)
			return;
		if (ClickEvent != null)
			ClickEvent(this.gameObject, pos);
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
}