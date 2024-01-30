﻿// Visual Pinball Engine
// Copyright (C) 2023 freezy and VPE Team
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

// ReSharper disable InconsistentNaming

using System;
using UnityEngine;

namespace VisualPinball.Unity
{
	[Serializable]
	public class MechSound
	{
		[SerializeReference]
		public SoundAsset Sound;

		public string TriggerId;

		[Range(0.0001f, 1)]
		// this initialization doesnt work in inspector https://www.reddit.com/r/Unity3D/comments/j5i6cj/inspector_struct_default_values/
		public float Volume = 1;
		
		public MechSoundAction Action = MechSoundAction.Play;

		[Tooltip("Increments of 1000")]
		[Min(0)]
		[Unit("ms")]
		public float Fade;
	}
	
	public enum MechSoundAction { Play, Stop };
}
