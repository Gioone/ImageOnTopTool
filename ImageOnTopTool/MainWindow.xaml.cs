using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace ImageOnTopTool
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        // private Thread _trdSetWindowBackgroundTransparent = new Thread();
        private readonly DispatcherTimer _timer = new DispatcherTimer()
        {
            Interval = TimeSpan.FromSeconds(10)
        };
        private bool _isWindowTransparent = false;
        // Tray related.
        private System.Windows.Forms.NotifyIcon _notifyIcon = null;
        private readonly HashSet<BitmapImage> images = new HashSet<BitmapImage>();
        private Thread _trdSetApplicationAlwaysOnTop;
        private readonly object _lockIsApplicationAlwaysOnTop = new object();
        private bool _isApplicationAlwaysOnTop = true;
        public bool IsApplicationAlwaysOnTop
        {
            get { return _isApplicationAlwaysOnTop; }
            set
            {
                lock (_lockIsApplicationAlwaysOnTop)
                {
                    _isApplicationAlwaysOnTop = value;
                }
            }
        }

        // private string[]
        public MainWindow()
        {
            InitializeComponent();
            SetLanguages(Properties.Settings.Default.Language);
            SetSelectedCmbLanguage();
            InitTray();
            ShowInTaskbar = false;
            InitControls();
            SetApplicationAlwaysOnTop();
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void SetSelectedCmbLanguage()
        {
            string language = Properties.Settings.Default.Language;
            switch (language)
            {
                case "en-us":
                    cmbLanguage.SelectedIndex = 0;
                    break;
                case "en-gb":
                    cmbLanguage.SelectedIndex = 1;
                    break;
                case "en-ca":
                    cmbLanguage.SelectedIndex = 2;
                    break;
                case "es-mx":
                    cmbLanguage.SelectedIndex = 3;
                    break;
                case "es-cr":
                    cmbLanguage.SelectedIndex = 4;
                    break;
                case "es-cl":
                    cmbLanguage.SelectedIndex = 5;
                    break;
                case "es-uy":
                    cmbLanguage.SelectedIndex = 6;
                    break;
                case "zh-cn":
                    cmbLanguage.SelectedIndex = 7;
                    break;
                case "zh-sp":
                    cmbLanguage.SelectedIndex = 8;
                    break;
                case "zh-tw":
                    cmbLanguage.SelectedIndex = 9;
                    break;
                case "ja":
                    cmbLanguage.SelectedIndex = 10;
                    break;
                case "ko":
                    cmbLanguage.SelectedIndex = 11;
                    break;
                case "ru":
                    cmbLanguage.SelectedIndex = 12;
                    break;
                case "de-lu":
                    cmbLanguage.SelectedIndex = 13;
                    break;
                case "it":
                    cmbLanguage.SelectedIndex = 14;
                    break;
            }
        }

        private void SetLanguages(string language)
        {
            try
            {
                XmlDocument document = new XmlDocument();
                switch (language)
                {
                    case "en-us":
                        document.Load(@"Languages\en-us.xml");
                        break;
                    case "en-gb":
                        document.Load(@"Languages\en-gb.xml");
                        break;
                    case "en-ca":
                        document.Load(@"Languages\en-ca.xml");
                        break;
                    case "es-mx":
                        document.Load(@"Languages\es-mx.xml");
                        break;
                    case "es-cr":
                        document.Load(@"Languages\es-cr.xml");
                        break;
                    case "es-cl":
                        document.Load(@"Languages\es-cl.xml");
                        break;
                    case "es-uy":
                        document.Load(@"Languages\es-uy.xml");
                        break;
                    case "zh-cn":
                        document.Load(@"Languages\zh-cn.xml");
                        break;
                    case "zh-sp":
                        document.Load(@"Languages\zh-sp.xml");
                        break;
                    case "zh-tw":
                        document.Load(@"Languages\zh-tw.xml");
                        break;
                    case "ja":
                        document.Load(@"Languages\ja.xml");
                        break;
                    case "ko":
                        document.Load(@"Languages\ko.xml");
                        break;
                    case "ru":
                        document.Load(@"Languages\ru.xml");
                        break;
                    case "de-lu":
                        document.Load(@"Languages\de-lu.xml");
                        break;
                    case "it":
                        document.Load(@"Languages\it.xml");
                        break;
                    default:
                        SetDefaultLanguage();
                        break;
                }
                XmlNode nodeText = document.SelectSingleNode("Text");
                XmlNode nodeControlText = nodeText.SelectSingleNode("ControlText");
                XmlNodeList childrenControlText = nodeControlText.ChildNodes;

                #region Control text.
                foreach (var node in childrenControlText)
                {
                    XmlElement element = (XmlElement)node;
                    string strControlName = element.GetAttribute("Name");
                    string strControlText = element.GetAttribute("Value");
                    switch (strControlName)
                    {
                        case "DialogText":
                            ControlText.DialogText = strControlText;
                            break;
                        case "BtnSelectImage":
                            ControlText.BtnSelectImage = strControlText;
                            break;
                        case "BtnCloseApp":
                            ControlText.BtnCloseApp = strControlText;
                            break;
                        case "ChkAlwaysOnTop":
                            ControlText.ChkAlwaysOnTop = strControlText;
                            break;
                        case "Language":
                            ControlText.Language = strControlText;
                            break;
                    }
                }
                #endregion

                #region Other text.
                XmlNode nodeOtherText = nodeText.SelectSingleNode("OtherText");
                XmlNodeList childrenOtherText = nodeOtherText.ChildNodes;
                foreach (var node in childrenOtherText)
                {
                    XmlElement element = (XmlElement)node;
                    string strOtherName = element.GetAttribute("Name");
                    string strOtherText = element.GetAttribute("Value");
                    switch (strOtherName)
                    {
                        case "TrayOpenText":
                            OtherText.TrayOpenText = strOtherText;
                            break;
                        case "TrayExitText":
                            OtherText.TrayExitText = strOtherText;
                            break;
                    }
                }
                #endregion

                Properties.Settings.Default.Language = language;
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
                SetDefaultLanguage();
            }
            SetText();
        }

        private void SetText()
        {
            btnSelectImage.Content = ControlText.BtnSelectImage;
            btnCloseApp.Content = ControlText.BtnCloseApp;
            chkBoxAlwaysOnTop.Content = ControlText.ChkAlwaysOnTop;
            lblLanguage.Content = ControlText.Language;
        }

        private void SetDefaultLanguage()
        {
            ControlText.DialogText = "Select image";
            ControlText.BtnSelectImage = "Select image";
            ControlText.BtnCloseApp = "Close App";
            ControlText.ChkAlwaysOnTop = "Always on Top";
            ControlText.Language = "Language";

            OtherText.TrayOpenText = "Open";
            OtherText.TrayExitText = "Exit";

            cmbLanguage.SelectedIndex = 0;
            Properties.Settings.Default.Language = "en-us";
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Tray initialize.
        /// </summary>
        private void InitTray()
        {
            _notifyIcon = new NotifyIcon();
            _notifyIcon.Text = "Image on top tool";
            _notifyIcon.Icon = Properties.Resources.logo_16x16;
            _notifyIcon.Visible = true;
            // Exit
            System.Windows.Forms.MenuItem exit = new System.Windows.Forms.MenuItem(OtherText.TrayExitText);
            exit.Click += Exit_Click;
            //关联托盘控件
            System.Windows.Forms.MenuItem[] childen = new System.Windows.Forms.MenuItem[] { exit };
            _notifyIcon.ContextMenu = new System.Windows.Forms.ContextMenu(childen);

            _notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler((o, e) =>
            {
                if (e.Button == MouseButtons.Left) Open_Click(o, e);
            });
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void Open_Click(object sender, EventArgs e)
        {
            Visibility = Visibility.Visible;
            ShowInTaskbar = true;
            Activate();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            System.Windows.Point point = Mouse.GetPosition(this);
            if ((point.X < 0 || point.Y < 0) && image.Source != null)
            {
                Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        private void InitControls()
        {
            chkBoxAlwaysOnTop.IsChecked = true;
        }

        private void BtnCloseApp_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BtnSelectImage_Click(object sender, RoutedEventArgs e)
        {
            SelectImage();
        }

        private string[] SelectImage()
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog()
            {
                CheckFileExists = true,
                CheckPathExists = true,
                // Multiselect = true,
                ValidateNames = true,
                Filter = "(*.jpg,*.png,*.jpeg,*.bmp,*.gif)|*.jgp;*.png;*.jpeg;*.bmp;*.gif",
                Title = ControlText.DialogText
            };
            bool? selectedImages = ofd.ShowDialog();
            if (selectedImages is null || !(bool)selectedImages)
            {
                return null;
            }
            string[] files = ofd.FileNames;
            BitmapImage bitmapImage = new BitmapImage(new Uri(files[0]));
            image.Source = bitmapImage;
            SetImageWidthAndHeight(bitmapImage);
            BorderThickness = new Thickness(0);
            /*for (int i = 0; i < files.Length; i++)
            {
                images.Add(new BitmapImage(new Uri(files[i])));
            }
            if (image.Source is null && images.Count != 0)
            {
                image.Source = images.ElementAt(0);
                SetImageWidthAndHeight(images.ElementAt(0));
                // Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(0, 0, 0, 0));
            }*/
            return ofd.FileNames;
        }

        private void SetImageWidthAndHeight(BitmapImage bitmapImage)
        {
            if (bitmapImage.Width < MinWidth || bitmapImage.Height < MinHeight) return;
            if (bitmapImage.Width > SystemParameters.WorkArea.Width || bitmapImage.Height > SystemParameters.WorkArea.Height) return;
            Width = bitmapImage.Width;
            Height = bitmapImage.Height;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void ChkBoxAlwaysOnTop_Click(object sender, RoutedEventArgs e)
        {
            SetApplicationAlwaysOnTop();
        }

        private void SetApplicationAlwaysOnTop()
        {
            bool b = chkBoxAlwaysOnTop.IsChecked ?? false;
            if (b)
            {
                if (!(_trdSetApplicationAlwaysOnTop is null))
                {
                    _trdSetApplicationAlwaysOnTop.Abort();
                    _trdSetApplicationAlwaysOnTop.Join();
                }
                _trdSetApplicationAlwaysOnTop = new Thread(() =>
                {
                    while (true)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            Topmost = true;
                        });
                        Thread.Sleep(10);
                    }
                })
                {
                    IsBackground = true
                };
                _trdSetApplicationAlwaysOnTop.Start();
            }
            else
            {
                try
                {
                    _trdSetApplicationAlwaysOnTop?.Abort();
                    // _trdSetApplicationAlwaysOnTop.Join();
                    Topmost = false;
                }
                catch (Exception) { }
            }
        }

        private void Window_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            DisplayControls();
        }

        private void DisplayControls()
        {
            ObjectAnimationUsingKeyFrames animation = new ObjectAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(500),
                DecelerationRatio = 1
            };
            DiscreteObjectKeyFrame frame1 = new DiscreteObjectKeyFrame(Visibility.Hidden, TimeSpan.FromMilliseconds(250));
            DiscreteObjectKeyFrame frame2 = new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.FromMilliseconds(250));
            animation.KeyFrames.Add(frame1);
            animation.KeyFrames.Add(frame2);
            grdUserInterop.BeginAnimation(VisibilityProperty, animation, HandoffBehavior.Compose);
            pnlControl.BeginAnimation(VisibilityProperty, animation, HandoffBehavior.Compose);
            Background = new SolidColorBrush(Colors.White);
            // _isWindowTransparent = false;
        }

        private void Window_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            HiddenControls();
        }

        private void HiddenControls()
        {
            if (image.Source is null) return;
            // Thread.Sleep(3000);
            ObjectAnimationUsingKeyFrames animation = new ObjectAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(500),
                DecelerationRatio = 1
            };
            DiscreteObjectKeyFrame frame1 = new DiscreteObjectKeyFrame(Visibility.Visible, TimeSpan.FromMilliseconds(250));
            DiscreteObjectKeyFrame frame2 = new DiscreteObjectKeyFrame(Visibility.Hidden, TimeSpan.FromMilliseconds(250));
            animation.KeyFrames.Add(frame1);
            animation.KeyFrames.Add(frame2);
            grdUserInterop.BeginAnimation(VisibilityProperty, animation, HandoffBehavior.Compose);
            pnlControl.BeginAnimation(VisibilityProperty, animation, HandoffBehavior.Compose);
            /*if (!(image.Source is null))
            {
                System.Windows.Point point = Mouse.GetPosition(this);
                if (point.X < 0 || point.Y < 0)
                {
                    Background = new SolidColorBrush(Colors.Transparent);
                }
                // Background = new SolidColorBrush(Colors.Transparent);
            }
            BorderThickness = new Thickness(0, 0, 0, 0);*/
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void BtnMinToTray_Click(object sender, RoutedEventArgs e)
        {
            MinToTray();
        }

        private void MinToTray()
        {
            Hide();
        }
        private void TrayIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            /*Visibility = Visibility.Visible;
            ShowInTaskbar = false;*/
        }

        private void MinimizeWindowCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CmbLanguage_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            System.Windows.Controls.ComboBox cmbBox = sender as System.Windows.Controls.ComboBox;
            ComboBoxItem cmbBoxItem = cmbBox.SelectedItem as ComboBoxItem;
            string language = cmbBoxItem.Content as string;
            switch (language)
            {
                case "English(en-us)":
                    SetLanguages("en-us");
                    break;
                case "English(en-gb)":
                    SetLanguages("en-ca");
                    break;
                case "English(en-ca)":
                    SetLanguages("en-ca");
                    break;
                case "español(es-mx)":
                    SetLanguages("es-mx");
                    break;
                case "español(es-cr)":
                    SetLanguages("es-cr");
                    break;
                case "español(es-cl)":
                    SetLanguages("es-cl");
                    break;
                case "español(es-uy)":
                    SetLanguages("es-uy");
                    break;
                case "简体中文(zh-cn)":
                    SetLanguages("zh-cn");
                    break;
                case "简体中文(zh-sp)":
                    SetLanguages("zh-sp");
                    break;
                case "正體中文(zh-tw)":
                    SetLanguages("zh-tw");
                    break;
                case "日本語(ja)":
                    SetLanguages("ja");
                    break;
                case "한국어(ko)":
                    SetLanguages("ko");
                    break;
                case "русский(ru)":
                    SetLanguages("ru");
                    break;
                case "Deutsch(de-lu)":
                    SetLanguages("de-lu");
                    break;
                case "Italiano(it)":
                    SetLanguages("it");
                    break;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _trdSetApplicationAlwaysOnTop?.Abort();
        }
    }
}
