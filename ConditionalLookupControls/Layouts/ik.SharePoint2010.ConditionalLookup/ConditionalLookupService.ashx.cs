/*
This file is part of Ingo Karstein's Conditional Lookup project

**Do not remove this comment**

Please see the project homepage at CodePlex:
  http://spconditionallookup.codeplex.com/

Please see my blog:
  http://ikarstein.wordpress.com

*/
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Runtime.InteropServices;
using Microsoft.SharePoint;
using Microsoft.SharePoint.ApplicationPages.Calendar;
using Microsoft.SharePoint.Diagnostics;
using Microsoft.SharePoint.Utilities;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Script.Serialization;
using System.Diagnostics;

namespace ik.SharePoint2010.ConditionalLookup
{
    [Guid("bd20b531-ffec-4685-8311-9579c87b0648")]
    public class ConditionalLookupService : IHttpHandler
    {
        private class Selection
        {
            public string Key
            {
                get;
                set;
            }
            public string Value
            {
                get;
                set;
            }

            public Selection(string key, string value)
            {
                Key = key;
                Value = value;
            }
        }
        private class Parameters
        {
            public List<Selection> Selection
            {
                get;
                set;
            }
            public List<Selection> DstSelection
            {
                get;
                set;
            }
            public string RefList
            {
                get;
                set;
            }
            public string RefSrcField
            {
                get;
                set;
            }
            public string RefDstField
            {
                get;
                set;
            }
            public string SrcType
            {
                get;
                set;
            }
            public string DstType
            {
                get;
                set;
            }

            public Parameters()
            {
                Selection = new List<Selection>();
                DstSelection = new List<Selection>();
                RefList = string.Empty;
                RefDstField = string.Empty;
                RefSrcField = string.Empty;
                SrcType = string.Empty;
                DstType = string.Empty;
            }

            public bool IsValid()
            {
                return ( Selection != null && Selection.Count > 0 && !string.IsNullOrEmpty(RefList) && !string.IsNullOrEmpty(RefSrcField) &&
                        !string.IsNullOrEmpty(RefDstField) && !string.IsNullOrEmpty(SrcType) && !string.IsNullOrEmpty(DstType) );
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            //Debugger.Break();

            try
            {

                Parameters prms = new Parameters();

                TextReader tr = new StreamReader(context.Request.InputStream);
                string input = tr.ReadToEnd();
                string[] parameters = input.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                tr.Close();
                context.Request.InputStream.Close();

                foreach( string s in parameters )
                {
                    string[] itms = s.Split(new string[] { "::" }, StringSplitOptions.RemoveEmptyEntries);
                    if( itms.Length == 2 )
                    {
                        string key = itms[0].Trim(new char[] { '\r', '\n', '|' }).ToLower();
                        string value = itms[1].Trim(new char[] { '\r', '\n', '|' });
                        switch( key )
                        {
                            case "reflist":
                                prms.RefList = value;
                                break;
                            case "refsrcfield":
                                prms.RefSrcField = value;
                                break;
                            case "refdstfield":
                                prms.RefDstField = value;
                                break;
                            case "srctype":
                                prms.SrcType = value;
                                break;
                            case "dsttype":
                                prms.DstType = value;
                                break;
                            case "selection":
                                string[] selpart1 = value.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach( string selpart1i in selpart1 )
                                {
                                    string[] selpart2 = selpart1i.Split(new string[] { "%%" }, StringSplitOptions.RemoveEmptyEntries);
                                    if( selpart2.Length == 2 )
                                    {
                                        prms.Selection.Add(new Selection(selpart2[0], selpart2[1]));
                                    };
                                };
                                break;
                            case "dstselection":
                                string[] selpart3 = value.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries);
                                foreach( string selpart3i in selpart3 )
                                {
                                    string[] selpart4 = selpart3i.Split(new string[] { "%%" }, StringSplitOptions.RemoveEmptyEntries);
                                    if( selpart4.Length == 2 )
                                    {
                                        prms.DstSelection.Add(new Selection(selpart4[0], selpart4[1]));
                                    };
                                };
                                break;
                        }
                    };
                };

                if( prms.IsValid() )
                {
                    if( SPContext.Current != null )
                    {
                        SPSecurity.RunWithElevatedPrivileges(delegate()
                        {
                            //Debugger.Break();

                            using( SPSite site = new SPSite(SPContext.Current.Site.ID) )
                            {
                                using( SPWeb web = site.OpenWeb(SPContext.Current.Web.ID) )
                                {
                                    SPList reflist = web.GetList(web.ServerRelativeUrl + ( prms.RefList.StartsWith("/") ? "" : "/" ) + prms.RefList);
                                    if( reflist != null )
                                    {
                                        if( reflist.Fields.ContainsField(prms.RefDstField) )
                                        {
                                            if( reflist.Fields.ContainsField(prms.RefSrcField) )
                                            {
                                                List<string> ret = new List<string>();
                                                List<string> selret = new List<string>();

                                                switch( prms.DstType )
                                                {
                                                    case "0":
                                                        ret.Add(@"<option value=""0"">(none)</option>");
                                                        break;
                                                    case "1":
                                                        ret.Add("(none)|0");
                                                        break;
                                                };

                                                SPQuery q = new SPQuery();

                                                if( prms.Selection.Count == 1 )
                                                {
                                                    q.Query = "<Where><Eq><FieldRef Name=\"" + prms.RefSrcField + "\"/><Value Type=\"Lookup\">" + prms.Selection[0].Key + "</Value></Eq></Where>";
                                                }
                                                else
                                                {
                                                    string qq = "<Where><Or>";
                                                    foreach( Selection sel in prms.Selection )
                                                    {
                                                        qq += "<Eq><FieldRef Name=\"" + prms.RefSrcField + "\"/><Value Type=\"Lookup\">" + sel.Key + "</Value></Eq>";
                                                    }
                                                    qq += "</Or></Where>";
                                                    q.Query = qq;
                                                };

                                                SPListItemCollection lic = reflist.GetItems(q);

                                                foreach( SPListItem li in lic )
                                                {
                                                    string val = "";
                                                    string valID = "0";
                                                    if( !li.Fields.GetField(prms.RefDstField).TypeAsString.Equals("lookup", StringComparison.InvariantCultureIgnoreCase) )
                                                    {
                                                        string valTmp = (string)li[prms.RefDstField];
                                                        if( valTmp.Contains(";#") )
                                                        {
                                                            string[] fl = ( (string)li[prms.RefDstField] ).Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                                            val = fl[1];
                                                        }
                                                        else
                                                        {
                                                            val = valTmp;
                                                        };
                                                        valID = li.ID.ToString();
                                                    }
                                                    else
                                                    {
                                                        string[] fl = ( (string)li[prms.RefDstField] ).Split(new string[] { ";#" }, StringSplitOptions.RemoveEmptyEntries);
                                                        val = fl[1];
                                                        valID = fl[0];
                                                    }

                                                    bool isSelected = false;
                                                    foreach( Selection sel in prms.DstSelection )
                                                    {
                                                        if( string.Compare(sel.Key, val, true) == 0 )
                                                        {
                                                            isSelected = true;
                                                        }
                                                    }

                                                    switch( prms.DstType )
                                                    {
                                                        case "0":
                                                            ret.Add(string.Format(@"<option value=""{0}""{2}>{1}</option>", valID, val, ( isSelected ? @" selected=""selected""" : "" )));
                                                            break;
                                                        case "1":
                                                            ret.Add(string.Format(@"{0}|{1}", val, valID));
                                                            if( isSelected )
                                                                selret.Add(val);
                                                            break;
                                                    };
                                                };

                                                TextWriter tw = new StreamWriter(context.Response.OutputStream);
                                                switch( prms.DstType )
                                                {
                                                    case "0":
                                                        tw.Write(string.Join("\r\n", ret.ToArray()));
                                                        break;
                                                    case "1":
                                                        tw.Write(string.Join("|", ret.ToArray()));
                                                        if( selret.Count > 0 )
                                                        {
                                                            tw.Write("\n");
                                                            tw.Write(string.Join("|", selret.ToArray()));
                                                        }
                                                        break;
                                                };

                                                tw.Flush();
                                                tw.Close();
                                                context.Response.OutputStream.Close();
                                                return;
                                            }
                                            else
                                            {
                                                throw new Exception("Reference source field \"" + prms.RefSrcField + "\" not found.");
                                            }
                                        }
                                        else
                                        {
                                            throw new Exception("Reference destination field \"" + prms.RefDstField + "\" not found.");
                                        };
                                    }
                                    else
                                    {
                                        throw new Exception("Reference list \"" + prms.RefList + "\" not found.");
                                    }
                                };
                            };
                        });
                    }
                    else
                    {
                        throw new Exception("No valid SPContext!");
                    }
                }
                else
                {
                    throw new Exception("Parameters not valid");
                };
            }
            catch( Exception ex )
            {
                TextWriter tw = new StreamWriter(context.Response.OutputStream);
                tw.WriteLine("ERROR");
                tw.WriteLine(ex.Message);
                tw.Flush();
                tw.Close();
                context.Response.OutputStream.Close();
            }
        }
    }
}
