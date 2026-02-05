using fireMCG.PathOfLayouts.System;

namespace fireMCG.PathOfLayouts.Messaging
{
    public sealed class OnAppStateChangeRequest : IMessage
    {
        public readonly StateController.AppState TargetState;

        public OnAppStateChangeRequest(StateController.AppState targetState)
        {
            TargetState = targetState;
        }
    }
}