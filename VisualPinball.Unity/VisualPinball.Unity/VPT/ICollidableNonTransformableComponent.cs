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

using Unity.Mathematics;

namespace VisualPinball.Unity
{
	public interface ICollidableNonTransformableComponent : ICollidableComponent
	{
		/// <summary>
		/// The translation matrix, that will be applied in reverse to the ball
		/// for hit testing and collision.
		/// </summary>
		/// <param name="worldToPlayfield">The playfield's worldToLocal matrix.</param>
		/// <returns></returns>
		internal float4x4 TranslateWithinPlayfieldMatrix(float4x4 worldToPlayfield);

		internal void GetColliders(Player player, ref ColliderReference colliders, ref ColliderReference kinematicColliders, float4x4 translateWithinPlayfieldMatrix, float margin);
	}
}
