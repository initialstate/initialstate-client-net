using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using log4net;
using RestSharp;
using RestSharp.Serialization.Json;

namespace InitialStateClient
{
	public class InitialStateEventsClient : IInitialStateEventsClient
	{
		private readonly InitialStateConfig _config;
		private readonly ILog _log = log4net.LogManager.GetLogger("is_event_sender");
		private readonly string _version;
		private readonly DateTime epochDateTime = new DateTime(1970, 1, 1);
		private RestClient _restClient;
		private string _defaultBucketKey;

		/// <summary>
		/// Creates an API Client for creating Initial State Buckets and sending events to the bucket.
		/// </summary>
		/// <param name="config">Optional configuration injection in lieu of using external file config</param>
		/// <exception cref="ConfigurationException">Thrown when there isn't an Access Key in the injected configuration or the discovered file configuration.</exception>
		public InitialStateEventsClient(InitialStateConfig config = null)
		{
			_version =
				FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
			_config = config ?? (InitialStateConfig) ConfigurationManager.GetSection("initialstate");
			_defaultBucketKey = _config.DefaultBucketKey;

			if (string.IsNullOrEmpty(_config.AccessKey))
				throw new ConfigurationException("access key is required");

			_restClient = new RestClient(_config.ApiBase);
			_restClient.UserAgent = "initialstate_net/" + _version;
		}

		/// <summary>
		/// Create a bucket to associate events with. This is an idempotent action unless a bucket has been deleted
		/// </summary>
		/// <param name="key">Bucket Key, should be unique in the scope of an Access Key and is generated by a caller; if not provided, a bucket key is generated and associated with the instantiation</param>
		/// <param name="name">Optional name of the bucket. If no name is provided, the bucket key is used</param>
		/// <param name="tags">Optional array of string tags to attach to the bucket upon creation. These do not get updated on subsequent creations of the same Bucket Key/Access Key</param>
		/// <returns>The Bucket Key for this bucket.</returns>
		/// <exception cref="InitialStateException">If the API responds with a non-successful HTTP Status Code, this exception is thrown</exception>
		public string CreateBucket(string key = null, string name = null, string[] tags = null)
		{
			if (string.IsNullOrEmpty(key))
				key = _defaultBucketKey;

			if (string.IsNullOrEmpty(key))
			{
				key = Guid.NewGuid().ToString("N");
				_defaultBucketKey = key;
			}

			if (string.IsNullOrEmpty(name))
				name = key;

			var request = new RestRequest("/buckets", Method.POST);
			request.AddHeader("Content-Type", "application/json");
			request.AddHeader("Accept-Version", _config.ApiVersion);
			request.JsonSerializer = new JsonSerializer();

			request.AddJsonBody(
				new
				{
					bucketName = name,
					bucketKey = key,
					tags
				});

			IRestResponse response = _restClient.Execute(request);

			Parameter bodyRequest = request.Parameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
			string body = "<empty>";
			if (bodyRequest != null)
			{
				body = bodyRequest.Value.ToString();
			}

			if ((int)response.StatusCode > 299 || (int)response.StatusCode < 200)
			{
				_log.Error($"Error creating bucket {response.ResponseUri}... {response.StatusCode}");

				throw new InitialStateException(response.StatusCode, body, response.Content);
			}

			return key;
		}

		/// <summary>
		/// A simple method to immediately send a single event
		/// </summary>
		/// <param name="key">key of the event to associate the value to.</param>
		/// <param name="value">value of the event</param>
		/// <param name="bucketKey">optional override of the bucket key associated with this instantiation</param>
		/// <param name="timestamp">optional timestamp override, if this isn't provided, one is automatically generated from the system clock</param>
		/// <param name="sendAsync">optional send event without waiting for confirmed response or wait for confirmed response</param>
		/// <exception cref="InitialStateException">If the API responds with a non-successful HTTP Status Code, this exception is thrown</exception>
		/// <exception cref="ConfigurationException">Thrown if there is no bucket key provided as a param, in the constructor configuration or in the file config</exception>
		public void SendEvent(string key, string value, string bucketKey = null, DateTime? timestamp = null,
			bool sendAsync = true)
		{
			IDictionary<string, string> dict = new Dictionary<string, string>
			{
				{key, value}
			};

			SendEvents(dict, bucketKey, timestamp);
		}

		/// <summary>
		/// Primary method for sending events to a Bucket in Initial State
		/// </summary>
		/// <typeparam name="T">Type of object to parse; expected to be a key-value type object</typeparam>
		/// <param name="obj">object containing keys and values to send to an initial state bucket</param>
		/// <param name="bucketKey">optional override of the bucket key associated with this instantiation</param>
		/// <param name="timestamp">optional timestamp override, if this isn't provided, one is automatically generated from the system clock</param>
		/// <param name="sendAsync">optional send event without waiting for confirmed response or wait for confirmed response</param>
		/// <exception cref="InitialStateException">If the API responds with a non-successful HTTP Status Code, this exception is thrown</exception>
		/// <exception cref="ConfigurationException">Thrown if there is no bucket key provided as a param, in the constructor configuration or in the file config</exception>
		public void SendEvents<T>(T obj, string bucketKey = null, DateTime? timestamp = null, bool sendAsync = true)
		{
			if (string.IsNullOrEmpty(bucketKey))
				bucketKey = _config.DefaultBucketKey;

			if (string.IsNullOrEmpty(bucketKey))
				throw new ConfigurationException("bucket key is required");

			double epoch = GetEpoch(timestamp);

			var request = new RestRequest("/events", Method.POST);
			request.AddHeader("X-IS-AccessKey", _config.AccessKey);
			request.AddHeader("X-IS-BucketKey", bucketKey);
			request.AddHeader("Accept-Version", _config.ApiVersion);
			request.JsonSerializer = new JsonSerializer();


			if (typeof(T).IsAssignableFrom(typeof(IDictionary<string, string>)))
			{
				List<Event> events = ((IDictionary<string, string>)obj).Select(kvp => new Event
				{
					Epoch = epoch,
					Key = kvp.Key,
					Value = kvp.Value
				}).ToList();
				request.AddJsonBody(events);
			}
			else
			{
				PropertyInfo[] properties = typeof(T).GetProperties();
				List<Event> events = properties.Select(prop => new Event
				{
					Epoch = epoch,
					Key = prop.Name,
					Value = prop.GetValue(obj).ToString()
				}).ToList();
				request.AddJsonBody(events);
			}

			Parameter bodyRequest = request.Parameters.FirstOrDefault(x => x.Type == ParameterType.RequestBody);
			string body = "<empty>";
			if (bodyRequest != null)
			{
				body = bodyRequest.Value.ToString();
			}

			if (sendAsync)
			{
				_restClient.ExecuteAsync(request, response =>
				{
					if ((int)response.StatusCode > 299 || (int)response.StatusCode < 200)
					{
						_log.Error(
							$"Unsuccessfully submitted events to {response.ResponseUri} ({_config.AccessKey}:{bucketKey})... {response.StatusCode} {body}");
					}
				});
			}
			else
			{
				IRestResponse response = _restClient.Execute(request);

				if ((int)response.StatusCode > 299 || (int)response.StatusCode < 200)
				{
					_log.Error($"Unsuccessfully submitted events to {response.ResponseUri}... {response.StatusCode} {body}");

					throw new InitialStateException(response.StatusCode, body, response.Content);
				}
			}
		}

		private double GetEpoch(DateTime? timestamp)
		{
			if (timestamp == null)
				timestamp = DateTime.UtcNow;

			return timestamp.Value.Subtract(epochDateTime).TotalMilliseconds / 1000;
		}
	}
}