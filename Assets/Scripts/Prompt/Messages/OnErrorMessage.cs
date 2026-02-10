using fireMCG.PathOfLayouts.Messaging;

namespace fireMCG.PathOfLayouts.Prompt
{
    public class OnErrorMessage : IMessage
    {
        public readonly string ErrorMessage;

        public OnErrorMessage(string message)
        {
            ErrorMessage = message;
        }
    }
}