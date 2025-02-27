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

using System;
using System.Linq;

namespace VisualPinball.Unity
{
	public class SoundPackable
	{
		public MultiPlayMode MultiPlayMode;
		public float Volume;
		public SoundPriority Priority;
		public float CalloutMaxQueueTime;

		public static byte[] Pack(SoundComponent comp) => PackageApi.Packer.Pack(new SoundPackable {
			MultiPlayMode = comp.MultiPlayMode,
			Volume = comp.Volume,
			Priority = comp.Priority,
			CalloutMaxQueueTime = comp.CalloutMaxQueueTime,
		});

		public static void Unpack(byte[] bytes, SoundComponent comp)
		{
			var data = PackageApi.Packer.Unpack<SoundPackable>(bytes);
			comp.MultiPlayMode = data.MultiPlayMode;
			comp.Volume = data.Volume;
			comp.Priority = data.Priority;
			comp.CalloutMaxQueueTime = data.CalloutMaxQueueTime;
		}
	}

	public class BinaryEventSoundPackable : SoundPackable
	{
		public StartWhen StartWhen;
		public StopWhen StopWhen;
	}

	public class SwitchSoundPackable : BinaryEventSoundPackable
	{
		public string SwitchName;

		public static byte[] Pack(SwitchSoundComponent comp)
			=> PackageApi.Packer.Pack(new SwitchSoundPackable {
				MultiPlayMode = comp.MultiPlayMode,
				Volume = comp.Volume,
				Priority = comp.Priority,
				CalloutMaxQueueTime = comp.CalloutMaxQueueTime,
				StartWhen = comp.StartWhen,
				StopWhen = comp.StopWhen,
				SwitchName = comp.SwitchName
			});

		public static void Unpack(byte[] bytes, SwitchSoundComponent comp)
		{
			var data = PackageApi.Packer.Unpack<SwitchSoundPackable>(bytes);
			comp.MultiPlayMode = data.MultiPlayMode;
			comp.Volume = data.Volume;
			comp.Priority = data.Priority;
			comp.CalloutMaxQueueTime = data.CalloutMaxQueueTime;
			comp.StartWhen = data.StartWhen;
			comp.StopWhen = data.StopWhen;
			comp.SwitchName = data.SwitchName;
		}
	}

	public class CoilSoundPackable : BinaryEventSoundPackable
	{
		public string CoilName;

		public static byte[] Pack(CoilSoundComponent comp)
			=> PackageApi.Packer.Pack(new CoilSoundPackable {
				MultiPlayMode = comp.MultiPlayMode,
				Volume = comp.Volume,
				Priority = comp.Priority,
				CalloutMaxQueueTime = comp.CalloutMaxQueueTime,
				StartWhen = comp.StartWhen,
				StopWhen = comp.StopWhen,
				CoilName = comp.CoilName
			});

		public static void Unpack(byte[] bytes, CoilSoundComponent comp)
		{
			var data = PackageApi.Packer.Unpack<CoilSoundPackable>(bytes);
			comp.MultiPlayMode = data.MultiPlayMode;
			comp.Volume = data.Volume;
			comp.Priority = data.Priority;
			comp.CalloutMaxQueueTime = data.CalloutMaxQueueTime;
			comp.StartWhen = data.StartWhen;
			comp.StopWhen = data.StopWhen;
			comp.CoilName = data.CoilName;
		}
	}

	public struct SoundReferencesPackable {

		public int SoundAssetRef;
		public string[]	ClipRefs;

		public static byte[] PackReferences(SoundComponent comp, PackagedFiles files)
		{
			if (!comp.SoundAsset) {
				return Array.Empty<byte>();
			}

			// pack asset
			var assetRef = files.AddAsset(comp.SoundAsset);

			// pack sound files
			var clipRefs = comp.SoundAsset.Clips != null
				? comp.SoundAsset.Clips.Select(files.Add).ToArray()
				: Array.Empty<string>();

			return PackageApi.Packer.Pack(new SoundReferencesPackable {
				SoundAssetRef = assetRef,
				ClipRefs = clipRefs
			});
		}

		public static void Unpack(byte[] bytes, SoundComponent comp, PackagedFiles files)
		{
			var data = PackageApi.Packer.Unpack<SoundReferencesPackable>(bytes);
			comp.SoundAsset = files.GetAsset<SoundAsset>(data.SoundAssetRef);
			comp.SoundAsset.Clips = data.ClipRefs.Select(files.GetAudioClip).ToArray();
		}
	}

	public struct SoundMetaPackable
	{
		public string Guid;
		// will probably get more data in here
	}

	public class CalloutCoordinatorPackable
	{
		public float PauseDuration;

		public static byte[] Pack(CalloutCoordinator comp)
			=> PackageApi.Packer.Pack(new CalloutCoordinatorPackable {
				PauseDuration = comp.PauseDuration
			});
		
		public static void Unpack(byte[] bytes, CalloutCoordinator comp)
		{
			var data = PackageApi.Packer.Unpack<CalloutCoordinatorPackable>(bytes);
			comp.PauseDuration = data.PauseDuration;
		}
	}
	
	public class MusicCoordinatorPackable
	{
		public float FadeDuration;

		public static byte[] Pack(MusicCoordinator comp)
			=> PackageApi.Packer.Pack(new MusicCoordinatorPackable {
				FadeDuration = comp.FadeDuration
			});
		
		public static void Unpack(byte[] bytes, MusicCoordinator comp)
		{
			var data = PackageApi.Packer.Unpack<MusicCoordinatorPackable>(bytes);
			comp.FadeDuration = data.FadeDuration;
		}
	}
}
