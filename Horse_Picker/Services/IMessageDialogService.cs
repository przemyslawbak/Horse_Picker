namespace Horse_Picker.Services
{
    public interface IMessageDialogService
    {
        MessageDialogResult ShowUpdateWindow();
    }
    public enum MessageDialogResult
    {
        Update,
        Cancel
    }
}
