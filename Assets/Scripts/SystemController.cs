using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts
{
	public partial class SystemController : MonoBehaviourExtended
	{
		private const string MARKER_NAME = "marker";
		private const float DISTANCE_THRESHIOLD_F = 3f;

		private SourceController[] _sources;
		private DistanceWidgetController _text;
		private bool _isHidden;
		private int _hashMode;

		public void OnSourceFound(SourceController source)
		{
			Array.ForEach(_sources, _ => _.AddTarget(source));
		}

		public void OnSourceLost(SourceController source)
		{
			Array.ForEach(_sources, _ => _.RemoveTarget(source));
		}

		public void OnSourceDestroy()
		{
			ResetController();
		}

		private void ResetController()
		{
			_sources = new List<SourceController>(GetComponentsInChildren<SourceController>(true)).ToArray();
			Array.ForEach(_sources, _ => _.SetController(this));
		}

		// ReSharper disable once UnusedMember.Local
		protected override void Awake()
		{
			base.Awake();

			ResetController();
			_hashMode = Shader.PropertyToID("_Mode");
			_text = FindObjectsOfType<DistanceWidgetController>().FirstOrDefault();

			var markers = GetComponentsInChildren<Transform>(true)
				.Aggregate(new List<GameObject>(), (list, component) =>
				{
					var check =
						!list.Contains(component.gameObject) &&
						component.gameObject.name.Contains(MARKER_NAME) &&
						component.GetComponent<MeshRenderer>() != null;
					if(check)
						list.Add(component.gameObject);
					return list;
				})
				.Select(@object => @object.GetComponent<MeshRenderer>())
				.ToArray();
			Array.ForEach(markers, _ => _.material.SetFloat(_hashMode, 1f));
		}

		// ReSharper disable once UnusedMember.Local
		private void LateUpdate()
		{
			if(_sources.Length > 1)
			{
				var distance = Vector3.Distance(_sources[0].Transform.position, _sources[1].Transform.position);
				_text.SetDistance(distance);
				if(distance > DISTANCE_THRESHIOLD_F && !_isHidden)
				{
					Array.ForEach(_sources, _ => _.HideSpecial());
					_isHidden = true;
				}
				if(distance < DISTANCE_THRESHIOLD_F && _isHidden)
				{
					Array.ForEach(_sources, _ => _.ShowSpecial());
					_isHidden = false;
				}
			}
		}
	}
}
