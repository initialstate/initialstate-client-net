using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;

namespace InitialStateClient.Specs
{
	public class given_an_initial_state_client_sandbox_client
	{
		protected static IInitialStateEventsClient InitialStateEventsClient;

		private Establish context = () =>
		{
			InitialStateEventsClient = new InitialStateEventsClient(new InitialStateConfig
			{
				AccessKey = "someaccesskey",
				ApiBase = "https://groker.init.st/api",
				ApiVersion = "~0",
				DefaultBucketKey = "defaultbucketkey"
			});
		};
	}
}
