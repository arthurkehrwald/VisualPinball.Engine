﻿// Visual Pinball Engine
// Copyright (C) 2022 freezy and VPE Team
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
	public class SwitchMapping
	{
		public string Id = string.Empty;

		public int InternalId;

		public bool IsNormallyClosed;

		public string Description = string.Empty;

		public SwitchSource Source = SwitchSource.Playfield;

		public string InputActionMap = string.Empty;

		public string InputAction = string.Empty;

		public SwitchConstant Constant;

		[SerializeReference]
		public MonoBehaviour _device;
		public ISwitchDeviceComponent Device { get => _device as ISwitchDeviceComponent; set => _device = value as MonoBehaviour; }

		public string DeviceItem = string.Empty;

		public int PulseDelay = 250;
	}
}
