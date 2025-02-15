// Visual Pinball Engine
// Copyright (C) 2025 freezy and VPE Team
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using UnityEngine;
using Logger = NLog.Logger;

namespace VisualPinball.Unity
{
	/// <summary>
	/// Base component for playing a <c>SoundAsset</c> using the public methods <c>Play</c> and <c>Stop</c>.
	/// </summary>
	[AddComponentMenu("Visual Pinball/Sound/Sound")]
	public class SoundComponent : EnableAfterAwakeComponent
	{
		private enum MultiPlayMode
		{
			PlayInParallel,
			DoNotPlay,
			FadeOutPrevious,
			StopPrevious,
		}

		[SerializeReference]
		protected SoundAsset _soundAsset;

		[SerializeField]
		private MultiPlayMode _multiPlayMode;

		[SerializeField, Range(0f, 1f)]
		private float _volume = 1f;

		[SerializeField]
		private SoundPriority _priority = SoundPriority.Medium;

		[SerializeField]
		private float _maxQueueTime = -1;

		private CalloutCoordinator _calloutCoordinator;
		private MusicCoordinator _musicCoordinator;
		private readonly List<ISoundCommponentSoundPlayer> _soundPlayers = new();

		protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public bool IsPlayingOrRequestingSound()
		{
			return _soundPlayers.Any(x => x.IsPlayingOrRequestingSound());
		}

		protected override void OnEnableAfterAfterAwake()
		{
			base.OnEnableAfterAfterAwake();
			_calloutCoordinator = GetComponentInParent<CalloutCoordinator>();
			_musicCoordinator = GetComponentInParent<MusicCoordinator>();
		}

		protected void Update()
		{
			for (int i = _soundPlayers.Count - 1; i >= 0; i--)
			{
				if (!_soundPlayers[i].IsPlayingOrRequestingSound())
				{
					_soundPlayers[i].Dispose();
					_soundPlayers.RemoveAt(i);
				}
			}
		}

		protected virtual void OnDisable()
		{
			StopAllSounds(allowFade: true);
		}

		protected virtual void OnDestroy()
		{
			StopAllSounds(allowFade: false);
			_soundPlayers.ForEach(x => x.Dispose());
		}

		protected void StartSound()
		{
			if (!isActiveAndEnabled)
			{
				Logger.Warn("Cannot play a disabled sound component.");
				return;
			}

			if (_soundAsset == null)
			{
				Logger.Warn("Cannot play without sound asset. Assign it in the inspector.");
				return;
			}

			if (IsPlayingOrRequestingSound())
			{
				if (_soundAsset is MusicAsset)
				{
					// We never want to have multiple active music requests. Makes no sense.
					StopAllSounds(allowFade: true);
				}
				else
				{
					switch (_multiPlayMode)
					{
						case MultiPlayMode.PlayInParallel:
							// Don't need to do anything.
							break;
						case MultiPlayMode.DoNotPlay:
							return;
						case MultiPlayMode.FadeOutPrevious:
							StopAllSounds(allowFade: true);
							break;
						case MultiPlayMode.StopPrevious:
							StopAllSounds(allowFade: false);
							break;
					}
				}
			}

			var player = CreateSoundPlayer();
			player.StartSound(_volume);
			_soundPlayers.Add(player);
		}

		protected void StopAllSounds(bool allowFade)
		{
			_soundPlayers.ForEach(x => x.StopSound(allowFade));
		}

		private ISoundCommponentSoundPlayer CreateSoundPlayer()
		{
			if (_soundAsset is SoundEffectAsset)
			{
				return new SoundComponentSoundEffectPlayer(
					(SoundEffectAsset)_soundAsset,
					gameObject
				);
			}

			if (_soundAsset is CalloutAsset)
			{
				var request = new CalloutRequest(
					(CalloutAsset)_soundAsset,
					_priority,
					_maxQueueTime,
					_volume
				);
				return new SoundComponentCalloutPlayer(request, _calloutCoordinator);
			}

			if (_soundAsset is MusicAsset)
			{
				var request = new MusicRequest((MusicAsset)_soundAsset, _priority, _volume);
				return new SoundComponentMusicPlayer(request, _musicCoordinator);
			}

			throw new NotImplementedException(
				$"Unknown type of sound asset '{_soundAsset.GetType()}'"
			);
		}

		public virtual bool SupportsLoopingSoundAssets() => true;

		public virtual Type GetRequiredType() => null;
	}
}
