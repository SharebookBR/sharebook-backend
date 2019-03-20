using LightBDD.Framework;
using LightBDD.XUnit2;
using ShareBook.Tests.BDD.Services;

namespace ShareBook.Tests.BDD.Features
{
	public partial class RegisterUserFeature: FeatureFixture
	{
		private void Given_new_user_want_to_join_with_this_datas()
		{
            UserService service = new UserService();
            var  result = service.RegisterAsync().Result;

		}

		private void When_the_new_user_registers()
		{
			StepExecution.Current.IgnoreScenario("Not implemented yet");
		}

		private void Then_will_receive_a_success_message()
		{
			StepExecution.Current.IgnoreScenario("Not implemented yet");
		}
	}
}