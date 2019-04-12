using System;

namespace InitialStateClient
{
	public interface IInitialStateEventsClient
	{
		string CreateBucket(string key = null, string name = null, string[] tags = null);
		void SendEvent(string key, string value, string bucketKey = null, DateTime? timestamp = null, bool sendAsync = true);
		void SendEvents<T>(T obj, string bucketKey = null, DateTime? timestamp = null, bool sendAsync = true);
	}
}
