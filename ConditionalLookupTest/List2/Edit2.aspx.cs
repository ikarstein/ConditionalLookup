/* 
This file is part of Ingo Karstein's Conditional Lookup project

**Do not remove this comment**

Please see the project homepage at CodePlex:
  http://spconditionallookup.codeplex.com/

Please see my blog:
  http://ikarstein.wordpress.com

*/

using System;
using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using Microsoft.SharePoint.Security;
using System.Security.Permissions;
using Microsoft.SharePoint.WebPartPages;

namespace ik.SharePoint2010.ConditionalLookup.Layouts.ConditionalLookup
{
    public partial class Edit2 : WebPartPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        #region DynamicMasterpageFile

        private string m_masterpage = "~masterurl/default.master";

        protected internal string DetermineMasterPage(int ver)
        {
            switch (ver)
            {
                case 1:
                case 2:
                case 3:
                    return "/_layouts/layoutsv3.master";
            }
            return "/_layouts/v4.master";
        }

        [SharePointPermission(SecurityAction.Demand, ObjectModel = true)]
        protected override void OnPreInit(EventArgs e)
        {
            string customMasterUrl;


            base.OnPreInit(e);



            int ver = 4;
            bool masterPageReferenceEnabled = false;
            if (SPControl.GetContextWeb(this.Context) != null)
            {
                try
                {
                    ver = SPControl.GetContextWeb(this.Context).UIVersion;
                    masterPageReferenceEnabled = SPControl.GetContextWeb(this.Context).MasterPageReferenceEnabled;
                }
                catch
                {
                }
            }

            if (string.IsNullOrEmpty(this.MasterPageFile))
            {
                if (this.DynamicMasterPageFile == null)
                {
                    return;
                }
                customMasterUrl = null;
                if (((ver <= 3) || !masterPageReferenceEnabled) || (SPControl.GetContextWeb(this.Context) == null))
                {
                    customMasterUrl = this.DetermineMasterPage(ver);
                }
                else if ((this.DynamicMasterPageFile.Length > 1) && (this.DynamicMasterPageFile[0] == '~'))
                {
                    string str6;
                    string str3 = null;
                    int index = this.DynamicMasterPageFile.IndexOf('/');
                    if (index >= 0)
                    {
                        str3 = this.DynamicMasterPageFile.Substring(0, index);
                    }
                    if (((str6 = str3.ToUpperInvariant()) == null) || !(str6 == "~MASTERURL"))
                    {
                        customMasterUrl = null;
                    }
                    else
                    {
                        string str7 = this.DynamicMasterPageFile.Substring("~MASTERURL".Length).ToUpperInvariant();
                        if (str7 != null)
                        {
                            if (!(str7 == "/DEFAULT.MASTER"))
                            {
                                if (str7 == "/CUSTOM.MASTER")
                                {
                                    customMasterUrl = SPControl.GetContextWeb(this.Context).CustomMasterUrl;
                                }
                            }
                            else
                            {
                                customMasterUrl = SPControl.GetContextWeb(this.Context).MasterUrl;
                            }
                        }
                    }
                }
                this.MasterPageFile = customMasterUrl;
            }
            else
            {
                if (ver >= 4)
                {
                    if (this.MasterPageFile[0] == '~')
                    {
                        this.MasterPageFile = this.MasterPageFile.Substring(1);
                    }
                    string str5 = this.MasterPageFile.ToLowerInvariant();
                    if (str5 != null)
                    {
                        if (!(str5 == "/_layouts/application.master"))
                        {
                            if (str5 == "/_layouts/simple.master")
                            {
                                this.MasterPageFile = "/_layouts/simplev4.master";
                            }
                        }
                        else
                        {
                            this.MasterPageFile = "/_layouts/applicationv4.master";
                        }
                    }
                }

            }
        }

        public string DynamicMasterPageFile
        {
            get
            {
                return m_masterpage;
            }
            set
            {
                m_masterpage = value;
            }
        }
        #endregion
    }
}
