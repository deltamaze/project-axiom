using PlayFab;
using PlayFab.ClientModels;

// Test file to see what's available in PlayFab
namespace TestPlayFab
{
    public class PlayFabTest
    {
        public void TestMethods()
        {
            // Try to find the correct method names
            // PlayFabClientAPI.LoginWithEmailAddress
            // PlayFabClientAPI.RegisterPlayFabUser
            // Let's try some variations:
            
            var loginRequest = new LoginWithEmailAddressRequest();
            var registerRequest = new RegisterPlayFabUserRequest();
            
            // Comment out actual calls for now
            // PlayFabClientAPI.LoginWithEmailAddress(loginRequest, null, null);
            // PlayFabClientAPI.RegisterPlayFabUser(registerRequest, null, null);
        }
    }
}
