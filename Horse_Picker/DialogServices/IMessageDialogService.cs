using Horse_Picker.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horse_Picker.Dialogs
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
