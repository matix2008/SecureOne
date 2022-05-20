using System;
using System.Windows.Forms;

namespace SecureOne
{
    /// <summary>
    /// Реализует удобные функции для сообщений пользователю
    /// </summary>
    public static class MessageHelper
    {
        public static DialogResult QuestionYN(Form owner, string message, string caption = "")
        {
            return MessageBox.Show(owner, message, caption.Length == 0 ? owner.Text : caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        }

        public static void Info(Form owner, string message, string caption = "")
        {
            MessageBox.Show(owner, message, caption.Length == 0 ? owner.Text : caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void Warning(Form owner, string message, string caption = "")
        {
            MessageBox.Show(owner, message, caption.Length == 0 ? owner.Text : caption, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static void Error(Form owner, string message, string caption = "")
        {
            MessageBox.Show(owner, message, caption.Length == 0 ? owner.Text : caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Error(Form owner, Exception ex, string caption = "")
        {
            MessageBox.Show(owner, ex.Message, caption.Length == 0 ? owner.Text : caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
