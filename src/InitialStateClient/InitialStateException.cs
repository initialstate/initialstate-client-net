using System;
using System.Net;

namespace InitialStateClient
{ 
	public class InitialStateException : Exception
	{
		public InitialStateException(HttpStatusCode statusCode, string requestBody, string responseContent)
		{
			StatusCode = statusCode;
			RequestBody = requestBody;
			ResponseContent = responseContent;
		}

		public HttpStatusCode StatusCode { get; set; }
		public string RequestBody { get; set; }
		public string ResponseContent { get; set; }
	}

	public class ConfigurationException : Exception
	{
		public ConfigurationException(string message) : base(message)
		{
			
		}
	}
}