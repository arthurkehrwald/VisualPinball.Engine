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

using System;
using System.Collections.Generic;
using UnityEngine;
using VisualPinball.Engine.Game.Engines;
using VisualPinball.Engine.Math;
using Color = UnityEngine.Color;
using NLog;
using Logger = NLog.Logger;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace VisualPinball.Unity
{
	public class LampPlayer
	{
		/// <summary>
		/// List of all registered lamp APIs.
		/// </summary>
		private readonly Dictionary<ILampDeviceComponent, IApiLamp> _lamps = new Dictionary<ILampDeviceComponent, IApiLamp>();

		/// <summary>
		/// Links the GLE's IDs to the lamps.
		/// </summary>
		private readonly Dictionary<string, List<ILampDeviceComponent>> _lampAssignments = new Dictionary<string, List<ILampDeviceComponent>>();

		public List<ILampDeviceComponent> LampDevice(string id) => _lampAssignments[id];

		/// <summary>
		/// Links the GLE's IDs to the mappings.
		/// </summary>
		private readonly Dictionary<string, Dictionary<ILampDeviceComponent, LampMapping>> _lampMappings = new Dictionary<string, Dictionary<ILampDeviceComponent, LampMapping>>();

		private Player _player;
		private TableComponent _tableComponent;
		private IGamelogicEngine _gamelogicEngine;

		private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		internal IApiLamp Lamp(ILampDeviceComponent component)
			=> _lamps.ContainsKey(component) ? _lamps[component] : null;

		internal Dictionary<string, LampState> LampStates { get; } = new Dictionary<string, LampState>();
		internal void RegisterLamp(ILampDeviceComponent component, IApiLamp lampApi) => _lamps[component] = lampApi;

		public void Awake(Player player, TableComponent tableComponent, IGamelogicEngine gamelogicEngine)
		{
			_player = player;
			_tableComponent = tableComponent;
			_gamelogicEngine = gamelogicEngine;
		}

		public void OnStart()
		{
			if (_gamelogicEngine != null) {
				var config = _tableComponent.MappingConfig;
				_lampAssignments.Clear();
				_lampMappings.Clear();
				foreach (var lampMapping in config.Lamps) {

					if (lampMapping.Device == null) {
						Logger.Warn($"Ignoring unassigned lamp \"{lampMapping.Id}\".");
						continue;
					}

					AssignLampMapping(lampMapping);

					// turn it off
					if (_lamps.ContainsKey(lampMapping.Device)) {
						HandleLampEvent(lampMapping.Id, LampStatus.Off);
					}
				}

				if (_lampAssignments.Count > 0) {
					_gamelogicEngine.OnLampChanged += HandleLampEvent;
					_gamelogicEngine.OnLampsChanged += HandleLampsEvent;
				}
			}
		}

		private void HandleLampsEvent(object sender, LampsEventArgs lampsEvent)
		{
			foreach (var lampEvent in lampsEvent.LampsChanged) {
				Apply(lampEvent.Id, lampEvent.Source, lampEvent.IsCoil, (mapping, lamp, state) => ApplyValue(mapping, lamp, state, lampEvent.Id, lampEvent.Value));
			}
		}

		private void HandleLampEvent(object sender, LampEventArgs lampEvent)
		{
			Apply(lampEvent.Id, lampEvent.Source, lampEvent.IsCoil, (mapping, lamp, state) => ApplyValue(mapping, lamp, state, lampEvent.Id, lampEvent.Value));
		}

		public void HandleLampEvent(string id, float value)
		{
			Apply(id, LampSource.Lamp, false, (mapping, lamp, state) => ApplyValue(mapping, lamp, state, id, value));
		}

		public void HandleLampEvent(string id, LampStatus status)
		{
			Apply(id, LampSource.Lamp, false, (_, lamp, state) => ApplyStatus(lamp, state, id, status));
		}

		public void HandleLampEvent(string id, VisualPinball.Engine.Math.Color color)
		{
			Apply(id, LampSource.Lamp, false, (_, lamp, state) => ApplyColor(lamp, state, id, color));
		}

		public void HandleCoilEvent(string id, bool isEnabled)
		{
			Apply(id, LampSource.Lamp, true, (_, lamp, state) => ApplyStatus(lamp, state, id, isEnabled ? LampStatus.On : LampStatus.Off));
		}

		private void Apply(string id, LampSource lampSource, bool isCoil, Action<LampMapping, IApiLamp, LampState> action)
		{
			if (_lampAssignments.ContainsKey(id)) {
				foreach (var component in _lampAssignments[id]) {
					var mapping = _lampMappings[id][component];
					if (mapping.Source != lampSource || mapping.IsCoil != isCoil) {
						// so, if we have a coil here that happens to have the same name as a lamp,
						// or a GI light with the same name as an other lamp, skip.
						continue;
					}
					if (_lamps.ContainsKey(component)) {
						var lamp = _lamps[component];
						var state = LampStates[id];
						action(mapping, lamp, state);
					}
				}

				#if UNITY_EDITOR
				RefreshUI();
				#endif
			}
		}

		private void ApplyStatus(IApiLamp lamp, LampState state, string id, LampStatus status)
		{
			state.Status = status;
			LampStates[id] = state;
			lamp.OnLamp(status);
		}

		private void ApplyColor(IApiLamp lamp, LampState state, string id, VisualPinball.Engine.Math.Color color)
		{
			state.Color.SetColorWithoutAlpha(color);
			LampStates[id] = state;
			lamp.OnLamp(state.Color.ToUnityColor());
		}

		private void ApplyValue(LampMapping mapping, IApiLamp lamp, LampState state, string id, float value)
		{
			switch (mapping.Type) {
				case LampType.SingleOnOff:
					state.IsOn = value > 0;
					LampStates[id] = state;
					lamp.OnLamp(state.Status);
					break;

				case LampType.Rgb:
					state.Color.Alpha = (int)value;
					LampStates[id] = state;
					lamp.OnLamp(state.Intensity);
					break;

				case LampType.RgbMulti:
					state.SetChannel(mapping.Channel, value / 255f); // todo test
					LampStates[id] = state;
					lamp.OnLamp(state.Color.ToUnityColor());
					break;

				case LampType.SingleFading:
					state.Intensity = value / mapping.FadingSteps;
					LampStates[id] = state;
					lamp.OnLamp(state.Intensity);
					break;

				default:
					Logger.Error($"Unknown mapping type \"{mapping.Type}\" of lamp ID {id} for light {lamp}.");
					break;
			}
		}

		public void OnDestroy()
		{
			if (_lampAssignments.Count > 0 && _gamelogicEngine != null) {
				_gamelogicEngine.OnLampChanged -= HandleLampEvent;
				_gamelogicEngine.OnLampsChanged -= HandleLampsEvent;
			}
		}

		private void AssignLampMapping(LampMapping lampMapping)
		{
			var id = lampMapping.Id;
			if (!_lampAssignments.ContainsKey(id)) {
				_lampAssignments[id] = new List<ILampDeviceComponent>();
			}
			if (!_lampMappings.ContainsKey(id)) {
				_lampMappings[id] = new Dictionary<ILampDeviceComponent, LampMapping>();
			}
			_lampAssignments[id].Add(lampMapping.Device);
			_lampMappings[id][lampMapping.Device] = lampMapping;
			LampStates[id] = new LampState(lampMapping.Device.LampStatus, lampMapping.Device.LampColor.ToEngineColor());
		}

#if UNITY_EDITOR
		private void RefreshUI()
		{
			if (!_player.UpdateDuringGamplay) {
				return;
			}

			foreach (var manager in (EditorWindow[])Resources.FindObjectsOfTypeAll(Type.GetType("VisualPinball.Unity.Editor.LampManager, VisualPinball.Unity.Editor"))) {
				manager.Repaint();
			}
		}
#endif
	}
}
