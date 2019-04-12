using System.Configuration;

namespace InitialStateClient
{
	public class InitialStateConfig : ConfigurationSection
	{
		[ConfigurationProperty("accessKey", IsRequired = true)]
		public string AccessKey
		{
			get { return (string)this["accessKey"]; }
			set { this["accessKey"] = value; }
		}

		[ConfigurationProperty("apiBase", DefaultValue = "https://groker.init.st/api")]
		public string ApiBase
		{
			get { return (string)this["apiBase"]; }
			set { this["apiBase"] = value; }
		}

		[ConfigurationProperty("apiVersion", DefaultValue = "~0")]
		public string ApiVersion
		{
			get { return (string)this["apiVersion"]; }
			set { this["apiVersion"] = value; }
		}

		[ConfigurationProperty("defaultBucketKey")]
		public string DefaultBucketKey
		{
			get { return (string)this["defaultBucketKey"]; }
			set { this["defaultBucketKey"] = value; }
		}
	}
}