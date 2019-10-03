using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TimeChecker
{
    public partial class TimeCheckerForm : Form
    {
        private static string errorEmptyMessage = "を入力してください";
        private static string errorFormatWrongMessage = "を数字だけで入力してください（０ー２３）";
        private static string successMessage = "時間の範囲内";
        private static string failureMessage = "時間の範囲外";

        private static Color defaultForeColor = Color.Black;
        private static Color successForeColor = Color.Green;
        private static Color failureForeColor = Color.Red;

        public TimeCheckerForm()
        {
            InitializeComponent();
            DisableResize();
        }

        private void DisableResize()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            messageLabel.Text = "";
            messageLabel.ForeColor = defaultForeColor;

            var errorMessages = new List<string>();

            CheckText("開始時刻", errorMessages, startTimeInput, out int startTimeInt);
            CheckText("終了", errorMessages, endTimeInput, out int endTimeInt);
            CheckText("チェックする時刻", errorMessages, checkTimeInput, out int checkTimeInt);

            if (errorMessages.Count > 0)
            {
                messageLabel.Text = string.Join(Environment.NewLine, errorMessages.ToArray());
            }
            else
            {
                bool isIntimeInterval;

                if (startTimeInt == endTimeInt)
                    isIntimeInterval = checkTimeInt == endTimeInt;
                else if (startTimeInt < endTimeInt)
                    isIntimeInterval = checkTimeInt >= startTimeInt && checkTimeInt < endTimeInt;
                else if (startTimeInt > endTimeInt)
                    isIntimeInterval = checkTimeInt >= startTimeInt || checkTimeInt < endTimeInt;
                else
                    throw new Exception("Unexpected situation");

                if (isIntimeInterval)
                {
                    messageLabel.Text = successMessage;
                    messageLabel.ForeColor = successForeColor;
                }
                else
                {
                    messageLabel.Text = failureMessage;
                    messageLabel.ForeColor = failureForeColor;
                }
            }
        }

        private static void CheckText(string name, List<string> errors, TextBox textBox, out int returnValue)
        {
            returnValue = 0;

            if (string.IsNullOrEmpty(textBox.Text))
                errors.Add(name + errorEmptyMessage);
            else if (!int.TryParse(textBox.Text, out returnValue))
                errors.Add(name + errorFormatWrongMessage);
            else if (returnValue < 0 || returnValue > 23)
                errors.Add(name + errorFormatWrongMessage);
        }
    }
}
