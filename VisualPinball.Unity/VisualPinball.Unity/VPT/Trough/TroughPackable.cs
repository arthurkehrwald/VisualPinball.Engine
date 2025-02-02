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

using MemoryPack;

namespace VisualPinball.Unity
{
	[MemoryPackable]
	public readonly partial struct TroughPackable
	{
		public readonly int Type;
		public readonly string PlayfieldEntrySwitchRef;
		public readonly string PlayfieldExitKickerRef;
		public readonly int BallCount;
		public readonly int SwitchCount;
		public readonly bool JamSwitch;
		public readonly int RollTime;
		public readonly int TransitionTime;
		public readonly int KickTime;

		public TroughPackable(int type, string playfieldEntrySwitchRef, string playfieldExitKickerRef, int ballCount, int switchCount, bool jamSwitch, int rollTime, int transitionTime, int kickTime)
		{
			Type = type;
			PlayfieldEntrySwitchRef = playfieldEntrySwitchRef;
			PlayfieldExitKickerRef = playfieldExitKickerRef;
			BallCount = ballCount;
			SwitchCount = switchCount;
			JamSwitch = jamSwitch;
			RollTime = rollTime;
			TransitionTime = transitionTime;
			KickTime = kickTime;
		}

		public static TroughPackable Unpack(byte[] data) => MemoryPackSerializer.Deserialize<TroughPackable>(data);

		public byte[] Pack() => MemoryPackSerializer.Serialize(this);
	}
}
