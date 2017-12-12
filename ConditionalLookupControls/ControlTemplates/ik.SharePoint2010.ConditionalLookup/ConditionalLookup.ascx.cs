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
using System.Diagnostics;
using System.Text;
using Microsoft.SharePoint.WebControls;

namespace ik.SharePoint2010.ConditionalLookup.Controls
{
    public partial class ConditionalLookup : UserControl
    {
        protected override void Render(HtmlTextWriter writer)
        {
            base.Render(writer);
        }
        private static Control FindControlRecursive(Control Root, string Id)
        {
            if (Root.ID == Id)
                return Root;
            foreach (Control Ctl in Root.Controls)
            {

                Control FoundCtl = FindControlRecursive(Ctl, Id);
                if (FoundCtl != null)
                    return FoundCtl;
            }
            return null;
        }

        private static Control GetFirst(Control ctrl, Type type)
        {
            foreach (Control c in ctrl.Controls)
            {
                if (c.GetType().Equals(type))
                    return c;
            }
            return null;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            //Debugger.Break();

            int srcType = 0;
            int dstType = 0;

            Control srcCtrl = FindControlRecursive(this.Page, SrcControl);
            Control dstCtrl = FindControlRecursive(this.Page, DstControl);

            if (srcCtrl is FormField)
                srcCtrl = srcCtrl.Controls[0];

            if (srcCtrl is LookupField)
            {
                srcType = 0;

                Control tmp = GetFirst(srcCtrl, typeof(DropDownList));

                if (tmp == null)
                {
                    srcType = 1;
                    tmp = GetFirst(srcCtrl, typeof(TextBox));
                };

                if (tmp != null)
                    srcCtrl = tmp;
            };

            if (dstCtrl is FormField)
                dstCtrl = dstCtrl.Controls[0];

            if (dstCtrl is LookupField)
            {
                dstType = 0;
                Control tmp = GetFirst(dstCtrl, typeof(DropDownList));
                if (tmp == null)
                {
                    dstType = 1;
                    tmp = GetFirst(dstCtrl, typeof(TextBox));
                };

                if (tmp != null)
                    dstCtrl = tmp;
            }

            

            StringBuilder sb = new StringBuilder();

            sb.AppendLine(@"<script language=""javascript"" type=""text/javascript"">");
            sb.AppendLine(@"var cl_$$$$_srcctrlid=""#" + srcCtrl.ClientID + @""";");
            sb.AppendLine(@"var cl_$$$$_dstctrlid=""#" + dstCtrl.ClientID + @""";");
            sb.AppendLine(@"ikjquery(document).ready(function(){");
            sb.AppendLine(@"  if( typeof(ikjquery) == ""undefined"" && (typeof(""conditionalLookupJQueryWarning"") == ""undefined"" || conditionalLookupJQueryWarning==false))");
            sb.AppendLine(@"  {");
            sb.AppendLine(@"    alert(""Conditional Lookup: No jQuery found!"");");
            sb.AppendLine(@"    conditionalLookupJQueryWarning = true;");
            sb.AppendLine(@"  }");
            sb.AppendLine(@"  if( typeof(ikjquery) != ""undefinded"" )");
            sb.AppendLine(@"  {");
            sb.AppendLine(@"    var cl_$$$$_srcctrl=ikjquery(cl_$$$$_srcctrlid);");
            sb.AppendLine(@"    if( typeof(cl_$$$$_srcctrl) != ""undefinded"")");
            sb.AppendLine(@"    {");
            if (srcType == 0)
                sb.AppendLine(@"      ikjquery(cl_$$$$_srcctrl).change( function(){ cl_$$$$_srcctrlchange(); });");
            else
            {
                sb.AppendLine(@"      cl_$$$$_lastTimeVal = cl_$$$$_srcctrl.val();");
                sb.AppendLine(@"      window.setInterval(""cl_$$$$_chksrcctrlchange()"", 500);");
            };

            sb.AppendLine(@"    }");
            sb.AppendLine(@"  };");
            sb.AppendLine(@"  cl_$$$$_srcctrlchange();");
            sb.AppendLine(@"});");
            if (srcType == 1)
            {
                sb.AppendLine(@"var cl_$$$$_lastTimeVal="""";");
                sb.AppendLine(@"function cl_$$$$_chksrcctrlchange()");
                sb.AppendLine(@"{");
                sb.AppendLine(@"  var ctrl = document.getElementById(""" + srcCtrl.ClientID + @""");");
                sb.AppendLine(@"  if( typeof(ctrl) != ""undefined"" && ctrl.value != cl_$$$$_lastTimeVal )");
                sb.AppendLine(@"  {");
                sb.AppendLine(@"    cl_$$$$_lastTimeVal = ctrl.value;");
                sb.AppendLine(@"    cl_$$$$_srcctrlchange();");
                sb.AppendLine(@"  };");
                sb.AppendLine(@"}");
            };

            sb.AppendLine();
            sb.AppendLine(@"function cl_$$$$_srcctrlchange()");
            sb.AppendLine(@"{");
            sb.AppendLine(@"  var cl_$$$$_srcctrl=ikjquery(cl_$$$$_srcctrlid)");
            sb.AppendLine(@"  var cl_$$$$_dstctrl=ikjquery(cl_$$$$_dstctrlid)");
            sb.AppendLine(@"  if( typeof(cl_$$$$_srcctrl) != ""undefined"" )");
            sb.AppendLine(@"  {");
            sb.AppendLine(@"    var cl_$$$$_sel_str="""";");
            sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""srctype::" + srcType.ToString() + @"""");
            sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""\ndsttype::" + dstType.ToString() + @"""");
            sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""\nreflist::" + RefList + @"""");
            sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""\nrefsrcfield::" + RefSrcField + @"""");
            sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""\nrefdstfield::" + RefDstField + @"""");
            sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""\nselection::"";");
            
            if (srcType == 0)
            {
                sb.AppendLine(@"    var cl_$$$$_sel=ikjquery(cl_$$$$_srcctrlid).find("":selected"");");
                sb.AppendLine(@"    cl_$$$$_sel.each(function(idx) {;");
                sb.AppendLine(@"         cl_$$$$_sel_str = cl_$$$$_sel_str + ""||""+ikjquery(this).text()+""%%""+ikjquery(this).val();");
                sb.AppendLine(@"       });");
            }
            else if (srcType == 1)
            {
                sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""||""+cl_$$$$_srcctrl.val()+""%%-1""");
            };

            sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""\ndstselection::"";");
            if( dstType == 0 )
            {
                sb.AppendLine(@"    var cl_$$$$_sel=ikjquery(cl_$$$$_dstctrl).find("":selected"");");
                sb.AppendLine(@"    cl_$$$$_sel.each(function(idx) {;");
                sb.AppendLine(@"         cl_$$$$_sel_str = cl_$$$$_sel_str + ""||""+ikjquery(this).text()+""%%""+ikjquery(this).val();");
                sb.AppendLine(@"       });");
            }
            else if( dstType == 1 )
            {
                sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""||""+cl_$$$$_dstctrl.val()+""%%-1""");
            };

            if( dstType == 1 )
            {
                sb.AppendLine(@"    var div = document.createElement(""div"");");
                sb.AppendLine(@"    var text = document.createTextNode(cl_$$$$_dstctrl.html());");
                sb.AppendLine(@"    div.appendChild(text);");
                sb.AppendLine(@"    cl_$$$$_sel_str = cl_$$$$_sel_str + ""\ndstctrlhtml::""+div.innerHTML;");
            }

            sb.AppendLine(@"    var ret = cl_$$$$_conditionalLookupGetData( cl_$$$$_sel_str );");
            sb.AppendLine(@"    if( !ret.match(/^ERROR/gi) )");
            sb.AppendLine(@"    {");
            if (dstType == 0)
            {
                sb.AppendLine(@"      cl_$$$$_dstctrl.html(ret);");
            }
            else if (dstType == 1)
            {
                sb.AppendLine(@"
       var selectedValue=""(none)"";
       if( ret.indexOf(""\n"") > -1 ) {
          var tmp=ret.split(""\n"");
          selectedValue=tmp[1];
          ret=tmp[0];
       }                  
");
                sb.AppendLine(@"      cl_$$$$_dstctrl.attr(""choices"", ret);");
                sb.AppendLine(@"      cl_$$$$_dstctrl.val(selectedValue);");
            }
            sb.AppendLine(@"    };");
            sb.AppendLine(@"  };");
            sb.AppendLine(@"}");
            sb.AppendLine();
            sb.AppendLine(@"function cl_$$$$_conditionalLookupCreateXMLHttpRequest() {");
            sb.AppendLine(@"  try { return new XMLHttpRequest(); } catch (e) { }");
            sb.AppendLine(@"  try { return new ActiveXObject(""Msxml2.XMLHTTP""); } catch (e) { }");
            sb.AppendLine(@"  try { return new ActiveXObject(""Microsoft.XMLHTTP""); } catch (e) { }");
            sb.AppendLine(@"  alert(""XMLHttpRequest not supported"");");
            sb.AppendLine(@"  return null;");
            sb.AppendLine(@"}");

            sb.AppendLine(@"function cl_$$$$_conditionalLookupGetData(sendData) {");
            sb.AppendLine(@"  var xmlHttpReq = cl_$$$$_conditionalLookupCreateXMLHttpRequest();");
            sb.AppendLine(@"  var url = L_Menu_BaseUrl + ""/_layouts/ik.SharePoint2010.ConditionalLookup/ConditionalLookupService.ashx"";");
            sb.AppendLine(@"  xmlHttpReq.open(""POST"", url, false);");
            sb.AppendLine(@"  xmlHttpReq.send(sendData);");
            sb.AppendLine(@"  var yourJSString = xmlHttpReq.responseText;");
            sb.AppendLine(@"  return yourJSString;");
            sb.AppendLine(@"}");
            sb.AppendLine(@"</script>");

            /*
            sb.AppendLine(@"
                <input type=""text"" id=""test"" value=""test1""/>
                <input type=""button"" id=""testb"" value=""test_button"" text=""test button""/>
                <script language=""javascript"" type=""text/javascript"">
                    ikjquery(document).ready(function(){
                        ikjquery(""#test"").blur(function(){ doTest(); });
                        ikjquery(""#testb"").click(function(){ doTestButton(); });
                    });
                    function doTest()
                    {
                        alert('Hi');
                    }
                    function doTestButton()
                    {
                        ikjquery(""#test"").val('Change');
                    };
                </script>
            ");
            */

            this.Page.ClientScript.RegisterClientScriptBlock(this.GetType(), this.ID + "_script", sb.ToString().Replace("$$$$", this.ID));
        }

        public string SrcControl { get; set; }
        public string DstControl { get; set; }
        public string RefList { get; set; }
        public string RefSrcField { get; set; }
        public string RefDstField { get; set; }
    }
}

