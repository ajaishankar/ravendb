﻿using System;
using System.Collections.Generic;

namespace Raven.Database.Storage.StorageActions
{
	public interface IQueueStorageActions
	{
		void EnqueueToQueue(string name, byte[] data);
		IEnumerable<Tuple<byte[], long>> PeekFromQueue(string name);
		void DeleteFromQueue(string name, long id);
	}
}