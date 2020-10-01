﻿using Sdl.LC.AddonBlueprint.Enums;

namespace Sdl.LC.AddonBlueprint.Models
{
	public class AccountLifecycleEvent<T> : AccountLifecycleEvent where T: class
	{
		/// <summary>
		/// The data object.
		/// </summary>
		public T Data { get; set; }
	}

	public class AccountLifecycleEvent
	{
		/// <summary>
		/// The account lifecycle event id.
		/// </summary>
		public AccountLifecycleEventEnum Id { get; set; }

		/// <summary>
		/// The timestamp.
		/// </summary>
		public string Timestamp { get; set; }
	}
}
