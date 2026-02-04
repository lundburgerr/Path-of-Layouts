using fireMCG.PathOfLayouts.Messaging;
using UnityEngine;

public class LayoutBrowserUiController : MonoBehaviour
{
    public void OpenMainMenu()
    {
        OnAppStateChangeRequest message = new OnAppStateChangeRequest(StateController.AppState.MainMenu);
        MessageBusManager.Resolve.Publish(message);
    }
}