using System;
using Machine.Specifications;

namespace InitialStateClient.Specs
{
	public class when_building_a_client_with_valid_configuration
	{
		private static IInitialStateEventsClient _client;
		private static Exception _actualException;

		private Establish context = () => { };

		private Because of = () =>
		{
			try
			{
				_client = new InitialStateEventsClient(new InitialStateConfig
				{
					AccessKey = "validaccesskey"
				});
			}
			catch (Exception ex)
			{
				_actualException = ex;
			}
		};

		private It should_not_throw_any_exception = () => _actualException.ShouldBeNull();
	}
}