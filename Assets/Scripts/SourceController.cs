using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vuforia;

namespace Assets.Scripts
{
	[RequireComponent(typeof(EllipsoidParticleEmitter))]
	[RequireComponent(typeof(ParticleAnimator))]
	[RequireComponent(typeof(ParticleRenderer))]
	public class SourceController : MonoBehaviourExtended
	{
		private const float PARTICLE_SPEED_F = 6f;

		private class SourceEventHandler : ITrackableEventHandler
		{
			private readonly SourceController _source;

			internal SourceEventHandler(SourceController source)
			{
				_source = source;
			}

			void ITrackableEventHandler.OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
			{
				if (_source != null)
				{
					switch (newStatus)
					{
						case TrackableBehaviour.Status.TRACKED:
							_source.OnSourceEnable();
							break;
						case TrackableBehaviour.Status.NOT_FOUND:
							_source.OnSourceDisable();
							break;
					}
				}
			}
		}

		[HideInInspector]
		public Color _color;

		private SystemController _controller;
		private ParticleEmitter _emitter;
		private ParticleAnimator _animator;
		private ImageTargetBehaviour _target;
		private MeshRenderer[] _special;
		private MeshRenderer _marker;
		private readonly SourceEventHandler _handler;
		private readonly List<SourceController> _targets = new List<SourceController>();
		private List<int> _existing;
		private readonly List<int> _links = new List<int>();

		public void SetController(SystemController controller)
		{
			_controller = controller;
		}

		public SourceController()
		{
			_handler = new SourceEventHandler(this);
			UpdateExisting();
		}

		public void AddTarget(SourceController target)
		{
			if(target == null || ReferenceEquals(this, target) || _targets.Contains(target))
				return;
			var firstEmpty = _targets.FindIndex(inspecting => inspecting == null);
			if(firstEmpty == -1)
				_targets.Add(target);
			else
				_targets[firstEmpty] = target;
			for(var counter = 0; counter < _links.Count; counter++)
				_links[counter] = _links[counter] != -1 ? _links[counter] : (firstEmpty == -1 ? _targets.Count - 1 : firstEmpty);
			UpdateExisting();
		}

		public void RemoveTarget(SourceController target)
		{
			var link = _targets.FindIndex(inspecting => inspecting == target);
			if(link != -1)
			{
				_targets[link] = null;
				UpdateExisting();
				for(var counter = 0; counter < _links.Count; counter++)
					_links[counter] =
						_links[counter] == link
							? (_existing.Count > 0 ? _existing[SelectRandomExistingTarget()] : -1)
							: _links[counter];
				UpdateExisting();
			}
		}

		public void HideSpecial()
		{
			Array.ForEach(_special, _ => _.enabled = false);
		}

		public void ShowSpecial()
		{
			Array.ForEach(_special, _ => _.enabled = true);
		}

		private void UpdateExisting()
		{
			var counter = 0;
			_existing = _targets.Aggregate(
				new List<int>(),
				(list, _) =>
				{
					if(_ != null)
						list.Add(counter);
					counter++;
					return list;
				});
		}

		private int SelectRandomExistingTarget()
		{
			return _existing.Count > 0 ? UnityEngine.Random.Range(0, _existing.Count) : -1;
		}

		private Vector3 GetTargetPositionForParticle(int pIndex)
		{
			while(pIndex >= _links.Count)
				_links.Add(SelectRandomExistingTarget());
			return
				_links[pIndex] != -1
					? _targets[_links[pIndex]].transform.position
					: transform.position;
		}

		private void OnSourceEnable()
		{
			_controller.OnSourceFound(this);
			if(_marker != null)
				_marker.enabled = true;
			if(_emitter != null)
				_emitter.emit = true;
		}

		private void OnSourceDisable()
		{
			if(_emitter != null)
			{
				_emitter.emit = false;
				_emitter.ClearParticles();
			}
			if(_marker != null)
				_marker.enabled = false;
			_controller.OnSourceLost(this);
		}

		// ReSharper disable once UnusedMember.Local
		protected override void Awake()
		{
			base.Awake();

			_special = GetComponentsInChildren<Component>(true)
				.Where(component => component.name.StartsWith("sphere_"))
				.Aggregate(new List<GameObject>(), (list, component) =>
				{
					var check =
						component.GetComponent<MeshRenderer>() != null &&
						!list.Contains(component.gameObject);
					if(check)
						list.Add(component.gameObject);
					return list;
				})
				.Select(@object => @object.GetComponent<MeshRenderer>())
				.ToArray();
		}

		// ReSharper disable once UnusedMember.Local
		private void OnEnable()
		{
			_emitter = GetComponent<EllipsoidParticleEmitter>();
			_emitter.emit = false;
			_animator = GetComponent<ParticleAnimator>();

			var parent = transform.parent;
			_marker = parent
				.GetComponentsInChildren<Transform>()
				.Where(component => component.name.Contains("marker") && component.GetComponent<MeshRenderer>() != null)
				.Select(component => component.GetComponent<MeshRenderer>())
				.FirstOrDefault();
			while(parent != null && parent.GetComponent<ImageTargetBehaviour>() == null)
				parent = parent.transform.parent;
			if(parent != null)
			{
				_target = parent.GetComponent<ImageTargetBehaviour>();
				_target.RegisterTrackableEventHandler(_handler);
			}
		}

		// ReSharper disable once UnusedMember.Local
		private void OnDisable()
		{
			if(_target != null)
			{
				_target.UnregisterTrackableEventHandler(_handler);
				_target = null;
			}
		}

		// ReSharper disable once UnusedMember.Local
		private void OnDestroy()
		{
			_controller.OnSourceDestroy();
		}

		// ReSharper disable once UnusedMember.Local
		private void LateUpdate()
		{
			_animator.force = (GetTargetPositionForParticle(0) - transform.position).normalized * PARTICLE_SPEED_F;

			//for(var counter = 0; counter < _emitter.particleCount; counter++) {
			//	var particles = _emitter.particles;
			//	particles[counter].velocity = (GetTargetPositionForParticle(counter) - particles[counter].position).normalized * PARTICLE_SPEED_F;
			//	_emitter.particles = particles;
			//}
		}
	}
}
