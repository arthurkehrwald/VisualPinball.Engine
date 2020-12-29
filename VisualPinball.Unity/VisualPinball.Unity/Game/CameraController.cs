// Visual Pinball Engine
// Copyright (C) 2020 freezy and VPE Team
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
using Unity.Mathematics;
using UnityEngine;

namespace VisualPinball.Unity
{
	[ExecuteAlways]
	public class CameraController : MonoBehaviour
	{
		public int presetIndex = -1;
		public CameraPreset[] cameraPresets;

		[NonSerialized]
		public Camera Camera;

		private void OnEnable()
		{
			Camera = GetComponentInChildren<Camera>();
		}

		public void ApplyPreset()
		{
			if (presetIndex < 0 || presetIndex > cameraPresets.Length - 1) {
				return;
			}

			if (!TableSelector.Instance.HasSelectedTable) {
				return;
			}

			var preset = cameraPresets[presetIndex];
			var table = TableSelector.Instance.SelectedTable;
			var trans = Camera.transform;
			var altitude = trans.transform.parent;
			var azimuth = altitude.parent;
			var offset = azimuth.parent;

			trans.localPosition = new Vector3(0, 0, -preset.distance);
			altitude.localRotation = Quaternion.Euler(new Vector3(preset.angle, 0, 0));
			azimuth.localRotation = Quaternion.Euler(new Vector3(0, preset.orbit, 0));
			offset.localPosition = table.GetTableCenter() + preset.offset;
			Camera.fieldOfView = preset.fov;

			var tb = table.GetTableBounds();
			var p = trans.position;
			var nearPoint = tb.ClosestPoint(p);
			var deltaN = Vector3.Magnitude(p - nearPoint);
			var deltaF = math.max(Vector3.Distance(p, tb.max), Vector3.Distance(p, tb.min));

			//TODO: Replace this with proper frustum distances.
			var nearPlane = math.max(0.001f, math.abs(deltaN * 0.9f));
			var farPlane = math.max(1f, math.abs(deltaF*1.1f));

			Camera.nearClipPlane = nearPlane;
			Camera.farClipPlane = farPlane;
		}
	}
}
