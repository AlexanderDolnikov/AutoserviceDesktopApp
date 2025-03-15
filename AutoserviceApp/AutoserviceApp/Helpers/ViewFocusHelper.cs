using System.Windows;
using System.Windows.Controls;

namespace AutoserviceApp.Helpers
{
    public static class ViewFocusHelper
    {
        public static void SetFocusAndClearItemsValues(params UIElement[] items)
        {
            try
            {
                // 1. focus on the first item
                items[0].Focus();

                // 2. clear all contents of all items
                foreach (var item in items)
                {
                    switch (item)
                    {
                        case TextBox textBox:
                            textBox.Clear();
                            break;
                        case PasswordBox passwordBox:
                            passwordBox.Clear();
                            break;
                        case DatePicker datePicker:
                            datePicker.SelectedDate = null;
                            break;
                        case ComboBox comboBox:
                            comboBox.SelectedIndex = -1;
                            break;

                        default:
                            throw new ArgumentException($"argument is not supported: {item.GetType()}");
                    }
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
                        
        }
    }
}
