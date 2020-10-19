using Atomus.Diagnostics;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Atomus.Control.Toolbar
{
    public partial class DefaultToolbar : UserControl, IAction
    {
        private AtomusControlEventHandler beforeActionEventHandler;
        private AtomusControlEventHandler afterActionEventHandler;
        private ImageList imageList;
        Size buttonSize;
        string[] items;
        private List<Button> buttonList;
        
        #region Init
        public DefaultToolbar()
        {
            InitializeComponent();

            this.imageList = new ImageList
            {
                ColorDepth = ColorDepth.Depth32Bit
            };

            try
            {
                this.imageList.ImageSize = this.GetAttributeSize("ImageSize");
            }
            catch (Exception _Exception)
            {
                DiagnosticsTool.MyTrace(_Exception);
                this.imageList.ImageSize = new Size(32, 32);
            }

            this.buttonSize = this.GetAttributeSize("ButtonSize");
            this.buttonList = new List<Button>();
            this.items = this.GetAttribute("Items").Split(',');

        }
        #endregion

        #region Dictionary
        #endregion

        #region Spread
        #endregion

        #region IO
        object IAction.ControlAction(ICore sender, AtomusControlArgs e)
        {
            System.Windows.Forms.Control[] controls;
            List<Button> listRemoveButton;
            string[] toolbarItems;

            try
            {
                if (e.Action != "UserToolbarButton.Add" & e.Action != "UserToolbarButton.Remove")
                    this.beforeActionEventHandler?.Invoke(this, e);

                switch (e.Action)
                {
                    case "UserToolbarButton.Remove":
                        listRemoveButton = new List<Button>();

                        //기본 버튼 다음부터; 마지막 버튼까지
                        for (int i = items.Length; i < this.buttonList.Count; i++)
                        {

                            //Global 버튼이 아니면 제거
                            if ((this.buttonList[i].Tag as ICore).GetAttribute(string.Format("{0}.Global", this.buttonList[i].Name)) == null
                                || (this.buttonList[i].Tag as ICore).GetAttribute(string.Format("{0}.Global", this.buttonList[i].Name)) == "N")
                            {
                                this.Controls.Remove(this.buttonList[i]);
                                this.RemoveButton(this.buttonList[i]);
                                listRemoveButton.Add(this.buttonList[i]);
                            }
                        }

                        foreach (Button button in listRemoveButton)
                        {
                            this.buttonList.Remove(button);
                        }
                        return true;

                    case "UserToolbarButton.Add":
                        listRemoveButton = ((List<Button>)Config.Client.GetAttribute(sender, "ToolbarButtons"));

                        if (listRemoveButton != null)//기존에 등록되어 있으면
                        {
                            foreach (Button button in listRemoveButton)
                            {
                                //Global 버튼이 아니면
                                if (sender.GetAttribute(string.Format("{0}.Global", button.Name)) == null || sender.GetAttribute(string.Format("{0}.Global", button.Name)) == "N")
                                {
                                    this.Controls.Add(button);
                                    this.AddButton(sender, button);
                                }
                            }
                        }
                        else
                        {
                            //toolbarItems = ((string)Config.Client.GetAttribute(sender, "ToolbarButtonItems"))?.Split(',');
                            toolbarItems = sender.GetAttribute("ToolbarButtonItems")?.Split(',');

                            if (toolbarItems != null)
                            {
                                for (int i = 0; i < toolbarItems.Length; i++)
                                {
                                    if (toolbarItems[i] != null && toolbarItems[i] != "")
                                        this.AddButton(sender, toolbarItems[i]);
                                }
                            }
                        }

                        return true;

                    default:
                        controls = this.Controls.Find(e.Action.Split('.')[1], true);

                        if (controls.Length == 1)
                        {
                            controls[0].Enabled = e.Value.Equals("Y");
                            return true;
                        }
                        else
                            return false;
                }
            }
            finally
            {
                if (e.Action != "UserToolbarButton.Add" & e.Action != "UserToolbarButton.Remove")
                    this.afterActionEventHandler?.Invoke(this, e);
            }
        }
        #endregion

        #region Event
        event AtomusControlEventHandler IAction.BeforeActionEventHandler
        {
            add
            {
                this.beforeActionEventHandler += value;
            }
            remove
            {
                this.beforeActionEventHandler -= value;
            }
        }
        event AtomusControlEventHandler IAction.AfterActionEventHandler
        {
            add
            {
                this.afterActionEventHandler += value;
            }
            remove
            {
                this.afterActionEventHandler -= value;
            }
        }

        private void DefaultMenu_Load(object sender, EventArgs e)
        {
            string[] tmps;
            string noticeString;

            try
            {
                if (this.GetAttribute("VisibleResponsibilityID") != "")
                {
                    tmps = this.GetAttribute("VisibleResponsibilityID").Split(',');

                    this.Visible = tmps.Contains(Config.Client.GetAttribute("Account.RESPONSIBILITY_ID").ToString());
                }

                for (int i = 0; i < this.items.Length; i++)
                {
                    this.AddButton(this, this.items[i]);
                }

                try
                {
                    this.webBrowser1.Url = new Uri(this.GetAttribute("Advertising.Url"), UriKind.Absolute);
                }
                catch (Exception _Exception)
                {
                    DiagnosticsTool.MyTrace(_Exception);
                }

                try
                {
                    this.webBrowser1.Size = this.GetAttributeSize("Advertising.Size");
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                try
                {
                    noticeString = this.GetAttribute("NoticeString");

                    if (noticeString != null && noticeString != "")
                        this.AddNotice(this, noticeString);
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }


                this.DefaultToolbar_SizeChanged(this, null);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(exception);
            }
        }

        private void Bnt_MouseHover(object sender, EventArgs e)
        {
            Button button;
            string[] tmps;

            button = (Button)sender;

            tmps = button.ImageKey.Split('.');

            button.ImageKey = string.Format("{0}.{1}.{2}", tmps[0], tmps[1], "ImageOn");
        }
        private void Bnt_MouseLeave(object sender, EventArgs e)
        {
            Button button;
            string[] tmps;

            button = (Button)sender;

            tmps = button.ImageKey.Split('.');

            button.ImageKey = string.Format("{0}.{1}.{2}", tmps[0], tmps[1], "Image");
        }
        private void Bnt_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.Control control;
            string action;
            string message;

            try
            {
                control = (System.Windows.Forms.Control)sender;

                action = control.Name.ToString();

                message = this.GetAttribute(string.Format("{0}.ClickMessage", action));

                if (message != null && message != "")
                    if (this.MessageBoxShow(this, message, action, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        return;

                this.afterActionEventHandler?.Invoke((IAction)control.Tag, action);
            }
            catch (Exception exception)
            {
                this.MessageBoxShow(this, exception);
            }
        }

        private void DefaultToolbar_SizeChanged(object sender, EventArgs e)
        {
            //UserControl userControl;

            //userControl = (UserControl)sender;

            //if (userControl.Size.Width <= 990)
            //{
            //    if (this.webBrowser1.Visible)
            //    {
            //        this.webBrowser1.Visible = false;
            //    }
            //}
            //else
            //{
            //    if (!this.webBrowser1.Visible)
            //    {
            //        this.webBrowser1.Visible = true;
            //        this.webBrowser1.Refresh(WebBrowserRefreshOption.Completely);
            //        //this.webBrowser1.Url = new System.Uri("http://atomus.dsun.kr/RealClick/Toolbar001.html", System.UriKind.Absolute); 
            //    }
            //}
        }
        #endregion

        #region "ETC"
        private void AddButton(ICore core, string name)
        {
            List<Button> listRemoveButton;
            Button button;

            button = new Button();

            try
            {
                listRemoveButton = ((List<Button>)Config.Client.GetAttribute(core, "ToolbarButtons"));

                if (listRemoveButton == null)//기존에 등록되어 있으면
                {
                    listRemoveButton = new List<Button>();

                    Config.Client.SetAttribute(core, "ToolbarButtons", listRemoveButton);
                }

                listRemoveButton.Add(button);

                button.Name = name;
                button.Tag = core;
                if (this.GetAttributeBool("TextVisible"))
                    button.Text = core.GetAttribute(string.Format("{0}.{1}", name, "Text"));

                try
                {
                    button.TextAlign = (ContentAlignment)Enum.Parse(typeof(ContentAlignment), this.GetAttribute("TextAlign"));
                }
                catch (Exception exception)
                {
                    DiagnosticsTool.MyTrace(exception);
                }

                button.UseVisualStyleBackColor = true;
                button.DoubleBuffered(true);
                this.Controls.Add(button);
                button.Size = this.buttonSize;
                button.Location = new Point(2 + (button.Size.Width * (this.buttonList.Count)), 2);

                this.AddImageList(core, button);
                this.AddButton(core, button);
            }
            catch (Exception exception)
            {
                DiagnosticsTool.MyTrace(exception);
            }
        }

        private void AddButton(ICore core, Button button)
        {
            if (!this.buttonList.Contains(button))
                this.buttonList.Add(button);

            button.Tag = core;
            button.ImageList = this.imageList;
            button.ImageKey = string.Format("{0}.{1}.{2}", ((System.Windows.Forms.Control)core).Name, button.Name, "Image");

            button.MouseEnter -= this.Bnt_MouseHover;
            button.MouseHover -= this.Bnt_MouseHover;
            button.MouseLeave -= this.Bnt_MouseLeave;
            button.Click -= this.Bnt_Click;

            button.MouseEnter += this.Bnt_MouseHover;
            button.MouseHover += this.Bnt_MouseHover;
            button.MouseLeave += this.Bnt_MouseLeave;
            button.Click += this.Bnt_Click;
        }
        private async void AddImageList(ICore core, Button button)
        {
            string tmp;
            string key;

            try
            {
                tmp = string.Format("{0}.{1}", button.Name, "Image");
                key = string.Format("{0}.{1}", ((System.Windows.Forms.Control)core).Name, tmp);

                if (!this.imageList.Images.ContainsKey(key))
                    this.imageList.Images.Add(key, await core.GetAttributeWebImage(tmp));
            }
            catch (Exception exception)
            {
                DiagnosticsTool.MyTrace(exception);
            }

            try
            {
                tmp = string.Format("{0}.{1}", button.Name, "ImageOn");
                key = string.Format("{0}.{1}", ((System.Windows.Forms.Control)core).Name, tmp);

                if (!this.imageList.Images.ContainsKey(key))
                    this.imageList.Images.Add(key, await core.GetAttributeWebImage(tmp));

                button.Refresh();
            }
            catch (Exception exception)
            {
                DiagnosticsTool.MyTrace(exception);
            }
        }


        private void AddNotice(ICore core, string noticeString)
        {
            Label label;
            Font font;

            label = new Label
            {
                Text = noticeString,
                AutoSize = true
            };

            font = core.GetAttributeFont(new Font(this.Font.FontFamily, this.Font.Size * 2), "NoticeString.Font");
            if (font == null)
                label.Font = new Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
            else
                label.Font = font;

            label.ForeColor = core.GetAttributeColor("NoticeString.ForeColor");

            this.Controls.Add(label);

            label.Dock = (DockStyle)Enum.Parse(typeof(DockStyle), core.GetAttribute("NoticeString.Dock"));
        }

        private void RemoveButton(Button button)
        {
            button.Tag = null;
            button.ImageList = null;
            button.ImageKey = null;

            button.MouseEnter -= this.Bnt_MouseHover;
            button.MouseHover -= this.Bnt_MouseHover;
            button.MouseLeave -= this.Bnt_MouseLeave;
            button.Click -= this.Bnt_Click;
        }
        private void RemoveImageList(ICore core, string name)
        {
            string _Tmp;
            string _Key;

            try
            {
                _Tmp = string.Format("{0}.{1}", name, "Image");
                _Key = string.Format("{0}.{1}", ((System.Windows.Forms.Control)core).Name, _Tmp);
                this.imageList.Images.RemoveByKey(_Key);
            }
            catch (Exception _Exception)
            {
                DiagnosticsTool.MyTrace(_Exception);
            }

            try
            {
                _Tmp = string.Format("{0}.{1}", name, "ImageOn");
                _Key = string.Format("{0}.{1}", ((System.Windows.Forms.Control)core).Name, _Tmp);
                this.imageList.Images.RemoveByKey(_Key);
            }
            catch (Exception _Exception)
            {
                DiagnosticsTool.MyTrace(_Exception);
            }
        }
        #endregion
    }
}