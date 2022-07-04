
/* Prod by turacican
 * 
 * If you are getting the DLL error, you can find it in the 'DLL FILES' folder in the dll files file location.
 * 
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Automation; // A necessary plugin for us to get a URL link.
using System.Diagnostics;
using System.Runtime.InteropServices; // A necessary plugin for winApi

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false; // We need to set this setting so that we can use the Worker object.
        }

        #region  WINAPI plugins

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hwnd, StringBuilder ss, int count);


        #endregion


        bool FirstWorking = true;
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 20) listBox1.Items.Clear();

            if (FirstWorking)
            {
                FirstWorking = false;
                bgWorker.RunWorkerAsync();
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Start();
        }

        /* ------------------- FUNCTIONS --------------------------  */

        #region FUNCTIONS 

        public string ActiveWindowTitle()
        {
            const int nChar = 256;
            StringBuilder ss = new StringBuilder(nChar);

            IntPtr handle = IntPtr.Zero;
            handle = GetForegroundWindow();

            if (GetWindowText(handle, ss, nChar) > 0)
            {
                string url = GetUrl(ss.ToString()).ToString();

                if (!string.IsNullOrEmpty(url) && !string.IsNullOrWhiteSpace(url))
                {
                    return url;
                }

                else
                {
                    return ss.ToString();
                }
            }

            else return "Desktop";

        }

        private string GetUrl(string allText)
        {

            if (allText.Contains("Chrome"))
            {
                return ReturnUrl("chrome");
            }

            else if (allText.Contains("Brave"))
            {
                return ReturnUrl("brave");
            }

            else if (allText.Contains("Edge"))
            {
                return ReturnUrl("msedge");
            }

            else if (allText.Contains("Firefox"))
            {
                return ReturnUrl("firefox");
            }

            else
            {
                return "";
            }

        }

        private string FinedUrl(Process process)
        {
            try
            {
                string _url = "";

                if (process == null)
                    throw new ArgumentNullException("process");

                if (process.MainWindowHandle == IntPtr.Zero)
                    return null;

                AutomationElement element = AutomationElement.FromHandle(process.MainWindowHandle);
                if (element == null)
                    return null;

                AutomationElementCollection elm1 = element.FindAll(TreeScope.Subtree, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));
                AutomationElement elm = elm1[0];
                string vp = ((ValuePattern)elm.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;

                if (vp.Contains("https") && vp.Contains("http"))
                {
                    _url = vp;
                }

                else
                {
                    _url = vp;
                }

                return _url;
            }
            catch (Exception)
            {
                Console.WriteLine("Error etc.");
                return null;
            }
        }

        private string ReturnUrl(string browserName)
        {
            string _url = "";

            if (browserName == "firefox")
            {
                Process[] procsfirefox = Process.GetProcessesByName("firefox");
                foreach (Process firefox in procsfirefox)
                {
                    if (firefox.MainWindowHandle == IntPtr.Zero)
                    {
                        continue;
                    }
                    AutomationElement sourceElement = AutomationElement.FromHandle(firefox.MainWindowHandle);
                    AutomationElement editBox = sourceElement.FindFirst(TreeScope.Descendants, new PropertyCondition(AutomationElement.NameProperty, "Search with Google or enter address"));
                    if (editBox != null)
                    {
                        ValuePattern val = ((ValuePattern)editBox.GetCurrentPattern(ValuePattern.Pattern));

                        _url = val.Current.Value;
                    }
                }
            }

            else
            {
                foreach (Process process in Process.GetProcessesByName(browserName))
                {
                    string url = FinedUrl(process);
                    if (url == null)
                        continue;

                    _url = url;
                }
            }

            return _url;

        }

        #endregion

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            listBox1.Items.Add(ActiveWindowTitle());
            FirstWorking = true;
        }
    }
}