using System;
using Studio;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRGIN.Core;

namespace KKSCharaStudioVR
{
	public class VRCameraMoveHelper : MonoBehaviour
	{
		public bool showGUI = true;

		public RectTransform menuRect;

		private static VRCameraMoveHelper _instance;

		public bool keepY = true;

		public bool moveAlong;

		public Vector3 moveAlongBasePos;

		public Quaternion moveAlongBaseRot;

		private float DEFAULT_DISTANCE = 3f;

		private float DISTANCE_RATIO = 1f;

		private global::Studio.Studio studio;

		private GameObject moveDummy;

		private int windowID = 8752;

		private const int panelWidth = 200;

		private const int panelHeight = 100;

		private Rect windowRect = new Rect(-1f, -1f, 0f, 0f);

		private string windowTitle = "VR Move";

		public static VRCameraMoveHelper Instance => _instance;

		public static void Install(GameObject container)
		{
			if (_instance == null)
			{
				_instance = container.AddComponent<VRCameraMoveHelper>();
			}
		}

		private void Awake()
		{
			SceneManager.sceneLoaded += OnSceneWasLoaded;
			DISTANCE_RATIO = VR.Context.Settings.IPDScale;
		}

		private void Start()
		{
		}

		private void OnSceneWasLoaded(Scene scene, LoadSceneMode mode)
		{
			studio = Singleton<global::Studio.Studio>.Instance;
			if (!(studio == null))
			{
				Transform cameraMenuRootT = studio.transform.Find("Canvas System Menu/02_Camera");
				_instance.Init(cameraMenuRootT);
			}
		}

		private void OnGUI()
		{
			if (!showGUI || !(menuRect != null) || !menuRect.gameObject.activeInHierarchy)
			{
				return;
			}
			GUISkin skin = GUI.skin;
			try
			{
				if (windowRect.x == -1f && windowRect.y == -1f)
				{
					windowRect = new Rect(Screen.width / 2, 100f, 200f, 100f);
				}
				windowRect = GUI.Window(windowID, windowRect, FuncWindowGUI, windowTitle);
			}
			finally
			{
				GUI.skin = skin;
			}
		}

		private void FuncWindowGUI(int winID)
		{
			try
			{
				GUI.enabled = true;
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				GUILayoutOption[] options = new GUILayoutOption[2]
				{
					GUILayout.Width(80f),
					GUILayout.Height(35f)
				};
				if (GUILayout.Button("Jump", options))
				{
					MoveToSelectedObject(true);
				}
				GUILayout.EndHorizontal();
				GUILayout.EndVertical();
				GUI.DragWindow();
			}
			catch (Exception value)
			{
				Console.WriteLine(value);
			}
		}

		public void SaveCamera(int slot)
		{
			if (!(VR.Camera.Head == null))
			{
				CurrentToCameraCtrl();
				studio.sceneInfo.cameraData[slot] = studio.cameraCtrl.Export();
			}
		}

		public void CurrentToCameraCtrl()
		{
			GetCurrentLookDirAndRot(out var lookPoint, out var dir, out var rot);
			Studio.CameraControl.CameraData cameraData = new Studio.CameraControl.CameraData();
			VR.Camera.Head.TransformPoint(dir.normalized * DEFAULT_DISTANCE * DISTANCE_RATIO);
			Vector3 distance = new Vector3(0f, 0f, -1f * DEFAULT_DISTANCE * DISTANCE_RATIO);
			cameraData.Set(lookPoint, rot, distance, studio.cameraCtrl.fieldOfView);
			studio.cameraCtrl.Import(cameraData);
		}

		private void GetCurrentLookDirAndRot(out Vector3 lookPoint, out Vector3 dir, out Vector3 rot)
		{
			lookPoint = VR.Camera.Head.TransformPoint(Vector3.forward * DEFAULT_DISTANCE);
			Vector3 vector = lookPoint;
			vector.y = VR.Camera.Head.position.y;
			dir = vector - VR.Camera.Head.position;
			if (dir == Vector3.zero)
			{
				dir = Vector3.forward;
			}
			rot = Quaternion.LookRotation(dir).eulerAngles;
		}

		public void MoveToCamera(int slot)
		{
			Studio.CameraControl.CameraData src = studio.sceneInfo.cameraData[slot];
			studio.cameraCtrl.Import(src);
			MoveToCurrent();
		}

		public void MoveToCurrent()
		{
			Studio.CameraControl.CameraData cameraData = studio.cameraCtrl.Export();
			Vector3 tobeHeadPos = cameraData.pos + Quaternion.Euler(cameraData.rotate) * cameraData.distance;
			Quaternion tobeHeadRot = Quaternion.Euler(cameraData.rotate);
			MoveTo(tobeHeadPos, tobeHeadRot);
		}

		public void MoveTo(Vector3 tobeHeadPos, Quaternion tobeHeadRot)
		{
			Transform transform = null;
			GameObject vROrigin = GetVROrigin();
			if (!(vROrigin == null))
			{
				transform = vROrigin.transform.parent;
				moveDummy.transform.position = VR.Camera.Head.position;
				moveDummy.transform.rotation = GripMoveStudioNEOV2Tool.RemoveXZRot(VR.Camera.Head.rotation);
				vROrigin.transform.parent = moveDummy.transform;
				moveDummy.transform.position = tobeHeadPos;
				moveDummy.transform.rotation = tobeHeadRot;
				vROrigin.transform.parent = transform;
				vROrigin.transform.rotation = GripMoveStudioNEOV2Tool.RemoveXZRot(vROrigin.transform.rotation);
			}
		}

		private GameObject GetVROrigin()
		{
			if ((bool)VR.Camera && (bool)VR.Camera.SteamCam && (bool)VR.Camera.SteamCam.origin)
			{
				return VR.Camera.SteamCam.origin.gameObject;
			}
			return null;
		}

		public void MoveToSelectedObject(bool lockY)
		{
			ObjectCtrlInfo[] selectObjectCtrl = Singleton<global::Studio.Studio>.Instance.treeNodeCtrl.selectObjectCtrl;
			if (selectObjectCtrl != null && selectObjectCtrl.Length != 0)
			{
				ObjectCtrlInfo objectCtrlInfo = selectObjectCtrl[0];
				Vector3 position = objectCtrlInfo.guideObject.transformTarget.position;
				if (objectCtrlInfo is OCIChar)
				{
					position = (objectCtrlInfo as OCIChar).charInfo.objHead.transform.position;
				}
				MoveToPoint(position, lockY);
			}
		}

		public void MoveToPoint(Vector3 targetPos, bool lockY)
		{
			GetCurrentLookDirAndRot(out var lookPoint, out var dir, out var rot);
			Vector3 tobeHeadPos = targetPos - dir.normalized * 0.5f * DISTANCE_RATIO;
			if (lockY)
			{
				tobeHeadPos.y = VR.Camera.Head.position.y;
			}
			else
			{
				tobeHeadPos.y += VR.Camera.Head.position.y - lookPoint.y;
			}
			MoveTo(tobeHeadPos, Quaternion.Euler(rot));
		}

		public void MoveForwardBackward(float distance)
		{
			GetCurrentLookDirAndRot(out var _, out var dir, out var rot);
			Vector3 tobeHeadPos = VR.Camera.Head.position + dir * distance * DISTANCE_RATIO;
			tobeHeadPos.y = VR.Camera.Head.position.y;
			MoveTo(tobeHeadPos, Quaternion.Euler(rot));
		}

		private void Init(Transform cameraMenuRootT)
		{
			VRLog.Info("Initializing VRCameraMoveHelper");
			try
			{
				menuRect = cameraMenuRootT.GetComponent<RectTransform>();
				if (moveDummy == null)
				{
					moveDummy = new GameObject("MoveDummy");
					UnityEngine.Object.DontDestroyOnLoad(moveDummy);
					moveDummy.transform.parent = base.gameObject.transform;
				}
				for (int i = 0; i < menuRect.childCount; i++)
				{
					Transform child = menuRect.GetChild(i);
					int idx = -1;
					if (int.TryParse(child.name, out idx))
					{
						child.Find("Button Save").gameObject.GetComponent<Button>().onClick.AddListener(delegate
						{
							OnSaveButtonClick(idx);
						});
						child.Find("Button Load").gameObject.GetComponent<Button>().onClick.AddListener(delegate
						{
							OnLoadButtonClick(idx);
						});
					}
					else
					{
						VRLog.Info("Not Found. {0}", child.name);
					}
				}
			}
			catch (Exception obj)
			{
				VRLog.Error(obj);
			}
			VRLog.Info("VR Camera Helper installed.");
		}

		private void OnSaveButtonClick(int idx)
		{
			SaveCamera(idx);
		}

		private void OnLoadButtonClick(int idx)
		{
			MoveToCamera(idx);
		}
	}
}