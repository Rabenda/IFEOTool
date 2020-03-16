using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace IFEOTool
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string openKey =
            @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Image File Execution Options\";
        private RegistryKey IFEOKey = Registry.LocalMachine.OpenSubKey(openKey, true);
        private readonly ObservableCollection<Member> memberData = new ObservableCollection<Member>();
        public MainWindow()
        {
            InitializeComponent();
            Refresh();
            Title = "IFEOTool";
        }

        void Refresh()
        {
            memberData.Clear();
            ReadRegistry().ForEach(e => memberData.Add(e));
            dataGrid.DataContext = memberData;
        }

        private List<Member> ReadRegistry()
        {
            var IFEOSubProgramName = IFEOKey.GetSubKeyNames();

            return IFEOSubProgramName.ToList()
                .Where(e =>
                {
                    if (!e.EndsWith(".exe"))
                        return false;
                    using (var programKey = IFEOKey.OpenSubKey(e))
                    {
                        return programKey.GetValue("Debugger") != null;
                    }

                })
                .Select(e =>
            {
                using (var programKey = IFEOKey.OpenSubKey(e))
                {
                    var debugger = programKey.GetValue("Debugger").ToString();
                    return new Member(e, debugger);
                }
            }).ToList();
        }

        private void SaveRegistry(List<Member> memberList)
        {
            var originList = ReadRegistry().Select(e => e.Name).ToList();
            var newList = memberList.Select(e => e.Name).ToList();
            var removeList = originList.Where(e => !newList.Contains(e)).ToList();
            removeList.ForEach(e =>
            {
                using (var programKey = IFEOKey.OpenSubKey(e, true))
                {
                    programKey?.DeleteValue("Debugger");
                }
            });
            memberList.ForEach(e =>
            {
                var programKey = IFEOKey.OpenSubKey(e.Name, true) ?? IFEOKey.CreateSubKey(e.Name, true);

                programKey.SetValue("Debugger", e.Location);
                programKey.Close();
            });
        }

        private void RefreshItem_OnClick(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void NewItem_OnClick(object sender, RoutedEventArgs e)
        {
            memberData.Add(new Member("", ""));
        }

        private void SaveItem_OnClick(object sender, RoutedEventArgs e)
        {
            var memberList = memberData.ToList();
            SaveRegistry(memberList);
            Refresh();
        }


    }



    public class Member
    {
        public Member(string name, string location)
        {
            Name = name;
            Location = location;
        }
        public string Name { get; set; }
        public string Location { get; set; }
    }
}
