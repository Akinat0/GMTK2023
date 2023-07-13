using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Lean.Common;
using CW.Common;

namespace Lean.Touch
{
	/// <summary>This component will draw a selection box.</summary>
	public class SelectionBox : MonoBehaviour
	{
		// This class will store an association between a Finger and a RectTransform instance
		[System.Serializable]
		public class FingerData : LeanFingerData
		{
			public RectTransform Box; // The RectTransform instance associated with this link
		}

		/// <summary>The camera this component will calculate using.
		/// None/null = MainCamera.</summary>
		public Camera Camera { set { _camera = value; } get { return _camera; } } [SerializeField] private Camera _camera;

		/// <summary>Ignore fingers with StartedOverGui?</summary>
		public bool IgnoreIfStartedOverGui { set { ignoreIfStartedOverGui = value; } get { return ignoreIfStartedOverGui; } } [SerializeField] private bool ignoreIfStartedOverGui = true;

		/// <summary>The selection box prefab.</summary>
		public RectTransform Prefab { set { prefab = value; } get { return prefab; } } [SerializeField] private RectTransform prefab;

		/// <summary>The transform the prefabs will be spawned on.
		/// NOTE: This RectTransform must fill the whole screen, like the main canvas.</summary>
		public RectTransform Root { set { root = value; } get { return root; } } [SerializeField] private RectTransform root;

		/// <summary>The selected objects will be selected by this component.</summary>
		public LeanSelectByFinger Select { set { select = value; } get { return select; } } [SerializeField] private LeanSelectByFinger select;

		/// <summary>For an object to be selected, it must be in one of these layers.</summary>
		public LayerMask RequiredLayers { set { requiredLayers = value; } get { return requiredLayers; } } [SerializeField] private LayerMask requiredLayers = -1;

		/// <summary>For an object to be selected, it must have one of these tags.</summary>
		public List<string> RequiredTags { get { if (requiredTags == null) requiredTags = new List<string>(); return requiredTags; } } [SerializeField] private List<string> requiredTags;

		// This stores all the links between Fingers and RectTransform instances
		private List<FingerData> fingerDatas = new List<FingerData>();

		// Temporary currentSelectables inside box
		private static HashSet<LeanSelectable> oldSelectables = new HashSet<LeanSelectable>();
		private static HashSet<LeanSelectable> prevSelectables = new HashSet<LeanSelectable>();
		private static HashSet<LeanSelectable> currentSelectables = new HashSet<LeanSelectable>();
		// private static List<LeanSelectable> newSelectables = new List<LeanSelectable>();

		protected virtual void OnEnable()
		{
			LeanTouch.OnFingerDown   += HandleFingerDown;
			LeanTouch.OnFingerUpdate += HandleFingerSet;
			LeanTouch.OnFingerUp     += HandleFingerUp;
		}

		protected virtual void OnDisable()
		{
			LeanTouch.OnFingerDown   -= HandleFingerDown;
			LeanTouch.OnFingerUpdate -= HandleFingerSet;
			LeanTouch.OnFingerUp     -= HandleFingerUp;
		}

		private void HandleFingerDown(LeanFinger finger)
		{
			oldSelectables.Clear();

			foreach (var selectable in select.Selectables)
				oldSelectables.Add(selectable);

			// Limit to one selection box
			if (fingerDatas.Count > 0)
			{
				return;
			}

			// Only use fingers clear of the GUI
			if (ignoreIfStartedOverGui == true && finger.StartedOverGui == true)
			{
				return;
			}

			if (finger.Index == LeanTouch.HOVER_FINGER_INDEX)
			{
				return;
			}

			// Make new link
			var fingerData = LeanFingerData.FindOrCreate(ref fingerDatas, finger);

			// Assign this finger to this link
			fingerData.Finger = finger;

			// Create LineRenderer instance for this link
			fingerData.Box = Instantiate(prefab);

			fingerData.Box.gameObject.SetActive(true);

			// Move box to root
			fingerData.Box.transform.SetParent(root, false);
		}

		private void HandleFingerSet(LeanFinger finger)
		{
			// Try and find the link for this finger
			var fingerData = LeanFingerData.Find(fingerDatas, finger);

			// Link exists?
			if (fingerData != null && fingerData.Box != null)
			{
				WriteTransform(fingerData.Box, fingerData.Finger);
			}
		}

		private void HandleFingerUp(LeanFinger finger)
		{
			// Try and find the link for this finger
			var fingerData = LeanFingerData.Find(fingerDatas, finger);

			// Link exists?
			if (fingerData != null)
			{
				// Remove link from list
				fingerDatas.Remove(fingerData);

				// Destroy box GameObject
				if (fingerData.Box != null)
				{
					Destroy(fingerData.Box.gameObject);
				}
			}
		}

		// Transform rect based on finger data
		private void WriteTransform(RectTransform rect, LeanFinger finger)
		{
			// Make sure the camera exists
			var camera = CwHelper.GetCamera(_camera, gameObject);

			if (camera != null)
			{
				var min = camera.ScreenToViewportPoint(finger.StartScreenPosition);
				var max = camera.ScreenToViewportPoint(finger.     ScreenPosition);
				
				// Fix any inverted values
				if (min.x > max.x)
				{
					var t = min.x;

					min.x = max.x;
					max.x = t;
				}

				if (min.y > max.y)
				{
					var t = min.y;

					min.y = max.y;
					max.y = t;
				}

				// Reset pivot in case you forgot
				rect.pivot = Vector2.zero;

				// Set anchors
				rect.anchorMin = min;
				rect.anchorMax = max;

				// Make rect we check against
				var viewportRect = new Rect();

				viewportRect.min = min;
				viewportRect.max = max;

				// Rebuild list of all currentSelectables within rect
				currentSelectables.Clear();


				foreach (var selectable in LeanSelectableByFinger.Instances)
				{
					var selectableMask = 1 << selectable.gameObject.layer;

					if ((selectableMask & requiredLayers) != 0 && HasRequiredTag(selectable.tag) == true)
					{
						var viewportPoint = camera.WorldToViewportPoint(selectable.transform.position);

						if (viewportRect.Contains(viewportPoint))
						{
							if(!oldSelectables.Contains(selectable))
								currentSelectables.Add(selectable);
						}
					}
				}

				// print("Select: ");
				// foreach (LeanSelectable toDeselect in prevSelectables.Where(selectable => !currentSelectables.Contains(selectable)))
				// {
				// 	print(toDeselect.);
				// 	select.Deselect(toDeselect);
				// }
				//
				// foreach (LeanSelectable toSelect in currentSelectables.Where(selectable => !prevSelectables.Contains(selectable)))
				// 	select.Select(toSelect);
				
				//
				// foreach (LeanSelectable selectable in select.Selectables)
				// {
				// 	if(!currentSelectables.Contains(selectable))
				// 		currentSelectables.Add(selectable);
				// }

				foreach (var oldSelectable in oldSelectables)
					currentSelectables.Add(oldSelectable);

				// Select them
				select.ReplaceSelection(currentSelectables.ToList(), finger);
			}
			else
			{
				Debug.LogError("Failed to find camera. Either tag your cameras MainCamera, or set one in this component.", this);
			}
		}

		private bool HasRequiredTag(string tag)
		{
			var count = 0;

			if (requiredTags != null)
			{
				foreach (var requiredTag in requiredTags)
				{
					if (string.IsNullOrEmpty(requiredTag) == false)
					{
						count += 1;

						if (requiredTag == tag)
						{
							return true;
						}
					}
				}
			}

			return count == 0;
		}
	}
}

#if UNITY_EDITOR
namespace Lean.Touch.Editor
{
	using UnityEditor;
	using TARGET = SelectionBox;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class SelectionBox_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("_camera", "The camera the translation will be calculated using.\n\nNone/null = MainCamera.");
			Draw("ignoreIfStartedOverGui", "Ignore fingers with StartedOverGui?");
			BeginError(Any(tgts, t => t.Prefab == null));
				Draw("prefab", "The selection box prefab.");
			EndError();
			BeginError(Any(tgts, t => t.Root == null));
				Draw("root", "The transform the prefabs will be spawned on.\n\nNOTE: This RectTransform must fill the whole screen, like the main canvas.");
			EndError();
			BeginError(Any(tgts, t => t.Select == null));
				Draw("select", "The selected objects will be selected by this component.");
			EndError();
			
			Separator();

			BeginError(Any(tgts, t => t.RequiredLayers == 0));
				Draw("requiredLayers", "For an object to be selected, it must be in one of these layers.");
			EndError();
			Draw("requiredTags", "For an object to be selected, it must have one of these tags.");
		}
	}
}
#endif