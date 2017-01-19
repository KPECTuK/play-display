using UnityEngine;

namespace Assets.Scripts
{
	public abstract class MonoBehaviourExtended : MonoBehaviour
	{
		/// <summary>
		/// [Deprecated]
		/// </summary>
		// ReSharper disable once InconsistentNaming
		public new Transform transform { get; private set; }
		public Transform Transform { get; private set; }

		// ReSharper disable once UnusedMemberHiearchy.Global
		protected virtual void Awake()
		{
			Transform = base.transform;
			transform = base.transform;
		}
	}
}