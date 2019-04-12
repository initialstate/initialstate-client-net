using System;
using Machine.Specifications;

namespace InitialStateClient.Specs
{
	public class when_building_a_client_without_an_access_key
	{
		private static Exception _actualException;
		private static IInitialStateEventsClient _client;

		private Establish context = () => { };

		private Because of = () =>
		{
			try
			{
				_client = new InitialStateEventsClient(new InitialStateConfig());
			}
			catch (Exception ex)
			{
				_actualException = ex;
			}
		};

		private It should_throw_an_exception = () => _actualException.ShouldNotBeNull();

		private It should_throw_the_appropriate_exception_type =
			() => _actualException.ShouldBeOfExactType(typeof(ConfigurationException));
	}
}