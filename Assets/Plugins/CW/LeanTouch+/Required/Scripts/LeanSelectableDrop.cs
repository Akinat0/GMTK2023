using UnityEngine;
using UnityEngine.Events;
using Lean.Common;
using CW.Common;

namespace Lean.Touch
{
	/// <summary>This script allows you to change the color of the SpriteRenderer attached to the current GameObject.</summary>
	[HelpURL(LeanTouch.PlusHelpUrlPrefix + "LeanSelectableDrop")]
	[AddComponentMenu(LeanTouch.ComponentPathPrefix + "Selectable Drop")]
	public class LeanSelectableDrop : LeanSelectableByFingerBehaviour
	{
		public enum IgnoreType
		{
			None,
			ThisGameObject,
			ThisGameObjectAndAncestors
		}

		[System.Serializable] public class GameObjectEvent : UnityEvent<GameObject> {}
		[System.Serializable] public class IDropHandlerEvent : UnityEvent<IDropHandler> {}

		/// <summary>When this object is dropped, should its GameObject layer be temporarily switched to IgnoreRaycast, so the drop can pass through it?
		/// None = Make no changes.
		/// ThisGameObject = Change this GameObject's layer only.
		/// ThisGameObjectAndAncestors = Change this GameObject and its ancestors's layers.</summary>
		public IgnoreType Ignore { set { ignore = value; } get { return ignore; } } [SerializeField] private IgnoreType ignore = IgnoreType.ThisGameObjectAndAncestors;

		public LeanScreenQuery ScreenQuery = new LeanScreenQuery(LeanScreenQuery.MethodType.Raycast);

		/// <summary>Called on the first frame the conditions are met.
		/// GameObject = The GameObject instance this was dropped on.</summary>
		public GameObjectEvent OnGameObject { get { if (onGameObject == null) onGameObject = new GameObjectEvent(); return onGameObject; } } [SerializeField] private GameObjectEvent onGameObject;
		
		/// <summary>Called on the first frame the conditions are met.
		/// GameObject = The GameObject instance this was dropped on.</summary>
		public UnityEvent OnEmpty { get { if (onEmpty == null) onEmpty = new UnityEvent(); return onEmpty; } } [SerializeField] private UnityEvent onEmpty;

		/// <summary>Called on the first frame the conditions are met.
		/// IDropHandler = The IDropHandler instance this was dropped on.</summary>
		public IDropHandlerEvent OnDropHandler { get { if (onDropHandler == null) onDropHandler = new IDropHandlerEvent(); return onDropHandler; } } [SerializeField] private IDropHandlerEvent onDropHandler;

		protected override void OnSelectedSelectFingerUp(LeanSelectByFinger select, LeanFinger finger)
		{
			if (ignore != IgnoreType.None)
			{
				LeanScreenQuery.ChangeLayers(gameObject, ignore == IgnoreType.ThisGameObjectAndAncestors, false);
			}

			var dropHandler   = default(IDropHandler);
			var root          = default(Component);
			var worldPosition = default(Vector3);
			var query         = ScreenQuery.TryQuery(gameObject, finger.ScreenPosition, ref dropHandler, ref root, ref worldPosition);

			LeanScreenQuery.RevertLayers();

			if (query == true)
			{
				dropHandler.HandleDrop(gameObject, finger);

				if (onGameObject != null)
				{
					onGameObject.Invoke(root.gameObject);
				}

				if (onDropHandler != null)
				{
					onDropHandler.Invoke(dropHandler);
				}
			}
			else
			{
				if (OnEmpty != null)
				{
					OnEmpty.Invoke();
				}
			}
		}

		
	}
}

#if UNITY_EDITOR
namespace Lean.Touch.Editor
{
	using UnityEditor;
	using TARGET = LeanSelectableDrop;

	[CanEditMultipleObjects]
	[CustomEditor(typeof(TARGET))]
	public class LeanSelectableDrop_Editor : CwEditor
	{
		protected override void OnInspector()
		{
			TARGET tgt; TARGET[] tgts; GetTargets(out tgt, out tgts);

			Draw("ignore", "When this object is dropped, should its GameObject layer be temporarily switched to IgnoreRaycast, so the drop can pass through it?\n\nNone = Make no changes.\n\nThisGameObject = Change this GameObject's layer only.\n\nThisGameObjectAndAncestors = Change this GameObject and its ancestors's layers.");
			Draw("ScreenQuery");

			Separator();

			var usedA = Any(tgts, t => t.OnGameObject.GetPersistentEventCount() > 0);
			var usedB = Any(tgts, t => t.OnDropHandler.GetPersistentEventCount() > 0);
			var usedC = Any(tgts, t => t.OnEmpty.GetPersistentEventCount() > 0);

			var showUnusedEvents = DrawFoldout("Show Unused Events", "Show all events?");

			if (usedA == true || showUnusedEvents == true)
			{
				Draw("onGameObject");
			}

			if (usedB == true || showUnusedEvents == true)
			{
				Draw("onDropHandler");
			}
			
			if (usedC == true || showUnusedEvents == true)
			{
				Draw("onEmpty");
			}
		}
	}
}
#endif