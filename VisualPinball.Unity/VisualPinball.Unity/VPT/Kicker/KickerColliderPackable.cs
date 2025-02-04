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

// ReSharper disable MemberCanBePrivate.Global

using MemoryPack;
using VisualPinball.Unity.Editor.Packaging;

namespace VisualPinball.Unity
{
	[MemoryPackable]
	public partial struct KickerColliderPackable
	{
		public float Scatter;
		public float HitAccuracy;
		public float HitHeight;
		public bool FallThrough;
		public bool FallIn;
		public bool LegacyMode;

		public static byte[] Pack(KickerColliderComponent comp)
		{
			return PackageApi.Packer.Pack(new KickerColliderPackable {
				Scatter = comp.Scatter,
				HitAccuracy = comp.HitAccuracy,
				HitHeight = comp.HitHeight,
				FallThrough = comp.FallThrough,
				FallIn = comp.FallIn,
				LegacyMode = comp.LegacyMode
			});
		}

		public static void Unpack(byte[] bytes, KickerColliderComponent comp)
		{
			var data = PackageApi.Packer.Unpack<KickerColliderPackable>(bytes);
			comp.Scatter = data.Scatter;
			comp.HitAccuracy = data.HitAccuracy;
			comp.HitHeight = data.HitHeight;
			comp.FallThrough = data.FallThrough;
			comp.FallIn = data.FallIn;
			comp.LegacyMode = data.LegacyMode;
		}
	}
}
