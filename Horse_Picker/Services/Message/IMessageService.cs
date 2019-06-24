using System.Collections.ObjectModel;

namespace Horse_Picker.Services.Message
{
    public interface IMessageService
    {
        MessageDialogResult ShowUpdateWindow();
    }
    public enum MessageDialogResult
    {
        Update,
        Cancel
    }
}
