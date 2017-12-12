using System;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Security;
using System.Collections.Generic;

namespace ConditionalLookupTest.Features.Feature1
{
    /// <summary>
    /// This class handles events raised during feature activation, deactivation, installation, uninstallation, and upgrade.
    /// </summary>
    /// <remarks>
    /// The GUID attached to this class may be used during packaging and should not be modified.
    /// </remarks>

    [Guid("e869a947-c32c-4d82-9c76-c819b3af5088")]
    public class Feature1EventReceiver : SPFeatureReceiver
    {
        private String GetLookupValue(SPWeb web, string listname, string field, string value)
        {
            string ret = "";
            try
            {
                SPList list = web.GetList(web.ServerRelativeUrl + "/Lists/" + listname);
                SPQuery query = new SPQuery();
                query.Query = "<Where><Eq><FieldRef Name='" + field + "'/><Value Type='Text'>" + value + "</Value></Eq></Where>";
                SPListItemCollection lic = list.GetItems(query);
                if (lic.Count > 0)
                {
                    ret = lic[0].ID.ToString() + ";#" + value;
                    return ret;
                }

            }
            catch (Exception ex)
            {
            };
            return ret;
        }

        private void AddListItem(SPWeb web, SPList list, string Field1, string Field2)
        {
            SPListItem dm1 = list.AddItem();
            dm1["RefListSrc"] = GetLookupValue(web, "List1", "List1FieldA", Field1);
            dm1["RefListDst"] = GetLookupValue(web, "List3", "List3FieldA", Field2);
            dm1.Update();
        }

        // Uncomment the method below to handle the event raised after a feature has been activated.

        public override void FeatureActivated(SPFeatureReceiverProperties properties)
        {
            SPWeb web = properties.Feature.Parent as SPWeb;
            SPSite site = web.Site;

            List<Guid> doneLists = new List<Guid>();
            bool retry = false;
            bool canCreateDM = false;
            try
            {
                do
                {
                    retry = false;
                    foreach (SPList list in web.Lists)
                    {
                        try
                        {
                            if (doneLists.Contains(list.ID))
                                continue;

                            if ((int)list.BaseTemplate == 20003)
                            {
                                AddListItem(web, list, "List1-Value1", "List1-Value1-1");
                                AddListItem(web, list, "List1-Value1", "List1-Value1-2");
                                AddListItem(web, list, "List1-Value1", "List1-Value1-3");
                                AddListItem(web, list, "List1-Value1", "List1-Value1-4");

                                AddListItem(web, list, "List1-Value2", "List1-Value2-1");
                                AddListItem(web, list, "List1-Value2", "List1-Value2-2");
                                AddListItem(web, list, "List1-Value2", "List1-Value2-3");
                                AddListItem(web, list, "List1-Value2", "List1-Value2-4");

                                AddListItem(web, list, "List1-Value3", "List1-Value3-1");
                                AddListItem(web, list, "List1-Value3", "List1-Value3-2");
                                AddListItem(web, list, "List1-Value3", "List1-Value3-3");
                                AddListItem(web, list, "List1-Value3", "List1-Value3-4");

                                AddListItem(web, list, "List1-Value4", "List1-Value4-1");
                                AddListItem(web, list, "List1-Value4", "List1-Value4-2");
                                AddListItem(web, list, "List1-Value4", "List1-Value4-3");
                                AddListItem(web, list, "List1-Value4", "List1-Value4-4");

                                AddListItem(web, list, "List1-Value5", "List1-Value5-1");
                                AddListItem(web, list, "List1-Value5", "List1-Value5-2");
                                AddListItem(web, list, "List1-Value5", "List1-Value5-3");
                                AddListItem(web, list, "List1-Value5", "List1-Value5-4");

                                AddListItem(web, list, "List1-Value6", "List1-Value6-1");
                                AddListItem(web, list, "List1-Value6", "List1-Value6-2");
                                AddListItem(web, list, "List1-Value6", "List1-Value6-3");
                                AddListItem(web, list, "List1-Value6", "List1-Value6-4");

                                doneLists.Add(list.ID);
                                retry = true;
                                break;
                            };

                        }
                        catch (Exception ex2)
                        {
                        }

                    };
                } while (retry);
            }
            catch (Exception ex)
            {
                return;
            };
        }


        // Uncomment the method below to handle the event raised before a feature is deactivated.

        //public override void FeatureDeactivating(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised after a feature has been installed.

        //public override void FeatureInstalled(SPFeatureReceiverProperties properties)
        //{
        //}


        // Uncomment the method below to handle the event raised before a feature is uninstalled.

        //public override void FeatureUninstalling(SPFeatureReceiverProperties properties)
        //{
        //}

        // Uncomment the method below to handle the event raised when a feature is upgrading.

        //public override void FeatureUpgrading(SPFeatureReceiverProperties properties, string upgradeActionName, System.Collections.Generic.IDictionary<string, string> parameters)
        //{
        //}
    }
}
