using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Collections;
using System.Drawing;
using System.Linq;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;
using System.Workflow.Activities;
using System.Workflow.Activities.Rules;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Workflow;
using Microsoft.SharePoint.WorkflowActions;
using System.Collections.Generic;

namespace WFSolution.MoveToFolderWF
{
    public sealed partial class MoveToFolderWF : SequentialWorkflowActivity
    {
        public MoveToFolderWF()
        {
            InitializeComponent();
        }

        public Guid workflowId = default(System.Guid);
        public SPWorkflowActivationProperties workflowProperties = new SPWorkflowActivationProperties();

        private void codeActivity1_ExecuteCode(object sender, EventArgs e)
        {
            SPListItem item = this.workflowProperties.Item;
            string str = this.ItemAdded(item);
        }

        private int GetRelatedUser(SPWeb web, int userLookupId, int contractId, bool isCompany, SPListItem sourceItem)
        {
            SPList list = web.GetList("/Lists/Contracts");
            SPList list2 = web.GetList("/Lists/Areas");
            SPList list3 = web.GetList("/Lists/ContractUsers");
            SPListItem item = !isCompany ? list.GetItemById(contractId) : null;
            SPListItem itemById = list3.GetItemById(userLookupId);
            if (userLookupId == 1)
            {
                return new SPFieldLookupValue(item["ContractorUser"].ToString()).LookupId;
            }
            if (userLookupId == 2)
            {
                return new SPFieldLookupValue(item["ConsultantUser"].ToString()).LookupId;
            }
            if (userLookupId == 4)
            {
                return new SPFieldLookupValue(item["ManagerUser"].ToString()).LookupId;
            }
            if (userLookupId == 5)
            {
                return new SPFieldLookupValue(list2.GetItemById(new SPFieldLookupValue(item["Area"].ToString()).LookupId)["AreaManagerUser"].ToString()).LookupId;
            }
            if (userLookupId == 9)
            {
                SPQuery query = new SPQuery();
                query.Query = string.Format("<Where>\r\n                                                          <Eq>\r\n                                                                <FieldRef Name='Company' LookupId='TRUE' />\r\n                                                                <Value Type='Lookup'>{0}</Value>\r\n                                                            </Eq>\r\n                                                        </Where>", contractId);
                SPListItem item3 = list2.GetItems(query)[0];
                return new SPFieldLookupValue(item3["AreaManagerUser"].ToString()).LookupId;
            }
            if (userLookupId == 12)
            {
                return new SPFieldUserValue(web, sourceItem["Author"].ToString()).LookupId;
            }
            if (userLookupId == 13)
            {
                return new SPFieldLookupValue(list2.GetItemById(new SPFieldLookupValue(item["Area"].ToString()).LookupId)["ExperienceManager"].ToString()).LookupId;
            }
            return new SPFieldLookupValue(itemById["UserName"].ToString()).LookupId;
        }
        private string ItemAdded(SPListItem item)
        {
            int lineNumber = 0;
            string siteURL = item.Web.Url;
            Guid listId = item.ParentList.ID;
            int iD = item.ID;
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        int id = 0;
                        lineNumber = 1;
                        SPList list = web.GetList("/Lists/FormPermissions");
                        SPList list2 = web.GetList("/Lists/Contracts");
                        SPList list3 = web.Lists[listId];
                        string url = list3.RootFolder.Url;
                        string str3 = url.Substring(url.LastIndexOf("/") + 1);
                        SPQuery query = new SPQuery
                        {
                            Query = string.Format("<Where>\r\n                                                              <Eq>\r\n                                                                 <FieldRef Name='ListName' />\r\n                                                                 <Value Type='Text'>{0}</Value>\r\n                                                              </Eq>\r\n                                                           </Where>", str3)
                        };
                        lineNumber = 2;
                        if (list.GetItems(query).Count > 0)
                        {
                            int num7;
                            lineNumber = 3;
                            SPListItem item1 = list.GetItems(query)[0];
                            string str4 = (item1["PermissionField"] != null) ? item1["PermissionField"].ToString() : "";
                            string strUrl = (item1["PermissionLookupList"] != null) ? item1["PermissionLookupList"].ToString() : "";
                            string str6 = (item1["PermissionLookupListField"] != null) ? item1["PermissionLookupListField"].ToString() : "";
                            lineNumber = 4;
                            if (strUrl != "")
                            {
                                SPList list4 = web.GetList(strUrl);
                                int lookupId = new SPFieldLookupValue(item[str6].ToString()).LookupId;
                                id = new SPFieldLookupValue(list4.GetItemById(lookupId)[str4].ToString()).LookupId;
                            }
                            else
                            {
                                id = new SPFieldLookupValue(item[str4].ToString()).LookupId;
                            }
                            lineNumber = 5;
                            SPListItem itemById = list2.GetItemById(id);
                            int num3 = this.GetRelatedUser(web, new SPFieldLookupValue(item1["Creator"].ToString()).LookupId, id, false, item);
                            int num4 = this.GetRelatedUser(web, new SPFieldLookupValue(item1["Approver1"].ToString()).LookupId, id, false, item);
                            int num5 = (item1["Approver2"] != null) ? this.GetRelatedUser(web, new SPFieldLookupValue(item1["Approver2"].ToString()).LookupId, id, false, item) : 0;
                            int num6 = (item1["Approver3"] != null) ? this.GetRelatedUser(web, new SPFieldLookupValue(item1["Approver3"].ToString()).LookupId, id, false, item) : 0;
                            SPFieldLookupValueCollection values = (item1["Editors"] != null) ? new SPFieldLookupValueCollection(item1["Editors"].ToString()) : null;
                            SPFieldLookupValueCollection values2 = (item1["Viewers"] != null) ? new SPFieldLookupValueCollection(item1["Viewers"].ToString()) : null;
                            SPFieldUserValueCollection values3 = (itemById["Viewers"] != null) ? new SPFieldUserValueCollection(web, itemById["Viewers"].ToString()) : null;
                            lineNumber = 6;
                            SPFieldLookupValue value2 = new SPFieldLookupValue(item1["Creator"].ToString());
                            List<int> list5 = new List<int>();
                            List<int> list6 = new List<int>();
                            if (values2 != null)
                            {
                                foreach (SPFieldLookupValue value3 in values2)
                                {
                                    num7 = value3.LookupId;
                                    list5.Add(this.GetRelatedUser(web, num7, id, false, item));
                                }
                            }
                            if (values3 != null)
                            {
                                foreach (SPFieldUserValue value4 in values3)
                                {
                                    num7 = value4.LookupId;
                                    list5.Add(num7);
                                }
                            }
                            if (values != null)
                            {
                                lineNumber = 7;
                                foreach (SPFieldLookupValue value3 in values)
                                {
                                    num7 = value3.LookupId;
                                    list6.Add(this.GetRelatedUser(web, num7, id, false, item));
                                }
                            }
                            lineNumber = 8;
                            SPFolder folder = web.GetFolder("/Lists/" + str3 + "/" + id.ToString());
                            if (!folder.Exists)
                            {
                                SPListItem item4 = list3.Items.Add(list3.RootFolder.ServerRelativeUrl, SPFileSystemObjectType.Folder, id.ToString());
                                item4["Title"] = id;
                                web.AllowUnsafeUpdates = true;
                                item4.Update();
                                lineNumber = 8;
                                folder = web.GetFolder(list3.RootFolder.ServerRelativeUrl + "/" + id.ToString());
                            }
                            SPFile file = item.Web.GetFile(item.Url);
                            string newUrl = string.Format("{0}/{1}_.000", folder.ServerRelativeUrl, item.ID);
                            file.MoveTo(newUrl);
                            lineNumber = 10;
                        }
                    }
                }
            });
            return "";
        }

        private string SetListItemPermission(SPListItem Item, int userId, int PermissionID, bool ClearPreviousPermissions)
        {
            string strError = "";
            string siteURL = Item.ParentList.ParentWeb.Url;
            Guid listId = Item.ParentList.ID;
            SPSecurity.RunWithElevatedPrivileges(delegate
            {
                using (SPSite site = new SPSite(siteURL))
                {
                    using (SPWeb web = site.OpenWeb())
                    {
                        SPPrincipal byID;
                        Exception exception;
                        web.AllowUnsafeUpdates = true;
                        SPListItem itemById = web.Lists[listId].GetItemById(Item.ID);
                        if (!itemById.HasUniqueRoleAssignments)
                        {
                            itemById.BreakRoleInheritance(!ClearPreviousPermissions);
                        }
                        try
                        {
                            byID = web.SiteUsers.GetByID(userId);
                        }
                        catch (Exception exception1)
                        {
                            exception = exception1;
                            byID = web.SiteGroups.GetByID(userId);
                        }
                        SPRoleAssignment roleAssignment = new SPRoleAssignment(byID);
                        SPRoleDefinition roleDefinition = web.RoleDefinitions.GetById(PermissionID);
                        roleAssignment.RoleDefinitionBindings.Add(roleDefinition);
                        itemById.RoleAssignments.Remove(byID);
                        itemById.RoleAssignments.Add(roleAssignment);
                        try
                        {
                            itemById.SystemUpdate(false);
                        }
                        catch (Exception exception2)
                        {
                            exception = exception2;
                            strError = exception.Message;
                        }
                    }
                }
            });
            return strError;
        }

    }
}
