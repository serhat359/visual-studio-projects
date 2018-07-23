using System;
using System.Reflection;
using System.Windows.Forms;
using RedGate.SIPFrameworkShared;

namespace SampleSsmsEcosystemAddin.Examples.CustomQueryWindow
{
    internal class OpenCustomQueryWindowCommand : ISharedCommandWithExecuteParameter, ISharedCommand
    {
        private readonly ISsmsFunctionalityProvider6 m_Provider;
        private readonly Action<string> m_LogMessage;
        private readonly ICommandImage m_CommandImage = new CommandImageForEmbeddedResources(Assembly.GetExecutingAssembly(), "SampleSsmsEcosystemAddin.Examples.rg_icon.ico");

        public OpenCustomQueryWindowCommand(ISsmsFunctionalityProvider6 provider, Action<string> logMessageCallback)
        {
            m_Provider = provider;
            m_LogMessage = logMessageCallback;
        }

        public string Name { get { return "RedGate_Sample_OpenCustomQueryWindow"; } }

        public void Execute(object parameter)
        {
            Execute();
        }

        public void Execute()
        {
            var control = new CustomQueryWindowControl(m_Provider);
            m_Provider.GetQueryWindowManager().CreateAugmentedQueryWindow(string.Empty, "SQL", control);
            control.Dock = DockStyle.Bottom;
            control.Width = 0;
            control.Height = 0;
            GetConnection(control);

            m_LogMessage("Just Created A Tab Made By Serhat");
            m_Provider.GetQueryWindowManager().AddQueryWindowContextMenuItem("rename serhat", new SharedCommand(m_Provider, m_LogMessage));
        }

        private void GetConnection(CustomQueryWindowControl control)
        {
            var parent = control.Parent;
            var type = parent.GetType();
            var connection = parent.GetType().GetProperty("Connection").GetValue(parent, new object[] { });
            //connection has all the information in it

            //Dumper.Dump(parent, s =>
            //{
            //    m_LogMessage(s ?? "");
            //});
            //
            //m_LogMessage("Finished parent");
            //
            //Dumper.Dump(connection, s =>
            //{
            //    m_LogMessage(s ?? "");
            //});
            //
            //var color = parent.Prop("BackColor");

            m_LogMessage(type.FullName);
            m_LogMessage(connection.GetType().FullName);
        }

        public string Caption { get { return "Serhat Query"; } }
        public string Tooltip { get { return "Tooltip"; } }
        public ICommandImage Icon { get { return m_CommandImage; } }
        public string[] DefaultBindings { get { return new[] { "global::Ctrl+N" }; } }
        public bool Visible { get { return true; } }
        public bool Enabled { get { return true; } }
    }
}