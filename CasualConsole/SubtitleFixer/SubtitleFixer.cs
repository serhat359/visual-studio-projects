using System.Text;

namespace SubtitleFixer;

public partial class SubtitleFixer : Form
{
    private string[] simpleFileNames = new string[] { };
    private string[] complexFileNames = new string[] { };

    public SubtitleFixer()
    {
        InitializeComponent();

        this.simpleFolderPathTextBox.Click += new System.EventHandler(this.simpleFolderPathTextBox_Click);

        this.complexFolderPathTextBox.Click += new System.EventHandler(this.complexFolderPathTextBox_Click);
    }

    private void simpleFolderPathTextBox_Click(object? sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        openFileDialog.Multiselect = false;
        openFileDialog.Filter = "Custom Files (*.srt)|*.srt|All files (*.*)|*.*";

        var result = openFileDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            this.simpleFileNames = openFileDialog.FileNames;

            simpleFolderPathTextBox.Text = string.Join(",", this.simpleFileNames);

            CheckSimpleFixButton();
        }
    }

    private void complexFolderPathTextBox_Click(object? sender, EventArgs e)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        openFileDialog.Multiselect = false;
        openFileDialog.Filter = "Custom Files (*.srt)|*.srt|All files (*.*)|*.*";

        var result = openFileDialog.ShowDialog();

        if (result == DialogResult.OK)
        {
            this.complexFileNames = openFileDialog.FileNames;

            complexFolderPathTextBox.Text = string.Join(",", this.complexFileNames);

            CheckComplexFixButton();
        }
    }

    private void CheckSimpleFixButton()
    {
        var success = this.simpleFileNames?.Length > 0 && int.TryParse(msTextbox.Text, out var millis);

        simpleFixButton.Enabled = success;
    }

    private void CheckComplexFixButton()
    {
        var success = this.complexFileNames?.Length > 0
            && TryParseTimeSpan(expected1Textbox.Text, out var expected1Value)
            && TryParseTimeSpan(expected2Textbox.Text, out var expected2Value)
            && TryParseTimeSpan(inFile1Textbox.Text, out var inFile1Value)
            && TryParseTimeSpan(inFile2Textbox.Text, out var inFile2Value);

        complexFixButton.Enabled = success;
    }

    private void msTextbox_TextChanged(object sender, EventArgs e)
    {
        CheckSimpleFixButton();
    }

    private void complexTextbox_TextChanged(object sender, EventArgs e)
    {
        CheckComplexFixButton();
    }

    private void simpleFixButton_Click(object sender, EventArgs e)
    {
        int.TryParse(msTextbox.Text, out var millis);

        var fileName = this.simpleFileNames[0];
        var i = fileName.LastIndexOf('.');
        var writeFileName = fileName.Substring(0, i) + "_2" + fileName.Substring(i);

        using (var fWrite = File.OpenWrite(writeFileName))
        {
            foreach (var line in File.ReadAllLines(fileName))
            {
                var line2 = line;
                var divider = " --> ";
                if (line2.Contains(divider))
                {
                    var parts = line2.Split(new[] { divider }, StringSplitOptions.None);
                    TryParseTimeSpan(parts[0], out var t1);
                    TryParseTimeSpan(parts[1], out var t2);

                    t1 += TimeSpan.FromMilliseconds(millis);
                    t2 += TimeSpan.FromMilliseconds(millis);

                    var format = "hh\\:mm\\:ss\\,fff";
                    line2 = t1.ToString(format) + divider + t2.ToString(format);
                }

                var bytes = Encoding.UTF8.GetBytes(line2 + "\n");
                fWrite.Write(bytes, 0, bytes.Length);
            }
        }

        MessageBox.Show("Done!");
    }

    private void complexFixButton_Click(object sender, EventArgs e)
    {
        TryParseTimeSpan(expected1Textbox.Text, out var expectedTime1);
        TryParseTimeSpan(expected2Textbox.Text, out var expectedTime2);
        TryParseTimeSpan(inFile1Textbox.Text, out var inFile1);
        TryParseTimeSpan(inFile2Textbox.Text, out var inFile2);

        var off1 = inFile1 - expectedTime1;
        var off2 = inFile2 - expectedTime2;
        var inFileDiff = inFile2 - inFile1;
        var offDiff = off2 - off1;
        var ratio = offDiff / inFileDiff;

        TimeSpan fixTimeSpan(TimeSpan timex)
        {
            var offRealx = ratio * (timex - inFile1) + off1;
            return timex - offRealx;
        }

        var fileName = this.complexFileNames[0];
        var i = fileName.LastIndexOf('.');
        var writeFileName = fileName.Substring(0, i) + "_2" + fileName.Substring(i);

        using (var fWrite = File.OpenWrite(writeFileName))
        {
            foreach (var line in File.ReadAllLines(fileName))
            {
                var line2 = line;
                var divider = " --> ";
                if (line2.Contains(divider))
                {
                    var parts = line2.Split(new[] { divider }, StringSplitOptions.None);
                    TryParseTimeSpan(parts[0], out var t1);
                    TryParseTimeSpan(parts[1], out var t2);

                    t1 = fixTimeSpan(t1);
                    t2 = fixTimeSpan(t2);

                    var format = "hh\\:mm\\:ss\\,fff";
                    line2 = t1.ToString(format) + divider + t2.ToString(format);
                }

                var bytes = Encoding.UTF8.GetBytes(line2 + "\n");
                fWrite.Write(bytes, 0, bytes.Length);
            }
        }

        MessageBox.Show("Done!");
    }

    private static bool TryParseTimeSpan(string s, out TimeSpan timeSpan)
    {
        if (s.Length == 0)
        {
            timeSpan = default;
            return false;
        }

        var parts = s.Split(',');
        if (parts.Length < 2)
        {
            timeSpan = default;
            return false;
        }

        if (!int.TryParse(parts[1], out var millis))
        {
            timeSpan = default;
            return false;
        }

        timeSpan = TimeSpan.Parse(parts[0]) + TimeSpan.FromMilliseconds(millis);

        return true;
    }
}