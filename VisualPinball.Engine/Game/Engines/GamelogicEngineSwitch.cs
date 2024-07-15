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

// ReSharper disable BuiltInTypeReferenceStyle
// ReSharper disable InconsistentNaming

using System;

namespace VisualPinball.Engine.Game.Engines
{
	/// <summary>
	/// A switch declaration.<p/>
	///
	/// Gamelogic engines and switch devices use this to announce which inputs
	/// they expect.
	/// </summary>
	///
	/// <remarks>
	/// This class isn't used during gameplay, but serves to declare the properties
	/// that will then used in the mapping.
	/// </remarks>
	[Serializable]
	public class GamelogicEngineSwitch : IGamelogicEngineDeviceItem
	{
		/// <summary>
		/// A unique identifier. This is what VPE uses to identify a switch.
		/// </summary>
		public virtual string Id { get => _id; set => _id = value; }

		/// <summary>
		/// If true, inverts the signal, i.e. disabled switches return "closed" (true),
		/// while enabled switched return "open" (false).
		/// </summary>
		public bool NormallyClosed;

		/// <summary>
		/// If true, only the "enabled" event is emitted by the element, and the engine
		/// handles disabling the switch after a configurable delay.
		/// </summary>
		public bool IsPulseSwitch;

		public virtual string Description { get => _description; set => _description = value; }
		public string InputActionHint;
		public string InputMapHint;

		public string DeviceHint { get => _deviceHint; set => _deviceHint = value; }
		public string DeviceItemHint { get => _deviceItemHint; set => _deviceItemHint = value; }
		public int NumMatches { get => _numMatches; set => _numMatches = value; }
		public SwitchConstantHint ConstantHint = SwitchConstantHint.None;

		protected string _description;
		protected string _id;
		protected string _deviceHint;
		protected string _deviceItemHint;
		protected int _numMatches = 1;

		public GamelogicEngineSwitch(string id)
		{
			Id = id;
		}

		public GamelogicEngineSwitch(int id)
		{
			Id = id.ToString();
		}
	}

	public enum SwitchConstantHint
	{
		None, AlwaysOpen, AlwaysClosed
	}
}
