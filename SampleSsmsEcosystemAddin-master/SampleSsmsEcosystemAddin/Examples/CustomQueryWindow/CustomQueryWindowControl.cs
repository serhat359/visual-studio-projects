using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RedGate.SIPFrameworkShared;

namespace SampleSsmsEcosystemAddin.Examples.CustomQueryWindow
{
    public partial class CustomQueryWindowControl : UserControl
    {
        private readonly ISsmsFunctionalityProvider6 m_Provider;

        public CustomQueryWindowControl(ISsmsFunctionalityProvider6 provider)
        {
            m_Provider = provider;
            InitializeComponent();
        }
        
        private string GetText()
        {
            return m_Provider.GetQueryWindowManager().GetActiveAugmentedQueryWindowContents();
        }

    }
}
