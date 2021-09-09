using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace azdyrski.Umbraco.UserReports.Models
{
    [DataContract]
    public class Permissions
    {
        [DataMember]
        public List<BackOfficeSection> AllowedSections { get; set; }
        [DataMember]
        public UmbracoNode StartContent { get; set; }
        [DataMember]
        public List<UmbracoNode> ExplicitUserStartContent { get; set; }
        [DataMember]
        public UmbracoNode StartMedia { get; set; }
        [DataMember]
        public List<UmbracoNode> ExplicitUserStartMedia { get; set; }
        [DataMember]
        public int EffectedMediaNodes { get; set; }
        [DataMember]
        public List<PermissionAction> Actions { get; set; }
        [DataMember]
        public bool HasExplicitPermissions { get; set; }
        [DataMember]
        public string MostPermissiveAction
        {
            get
            {
                if (Actions == null || Actions.Count == 0)
                    return "Nothing";
                var mostPermissive = Actions.Select(a => new KeyValuePair<string, double>(a.Name, a.ContentNodesEffected * a.Priority)).OrderByDescending(kvp => kvp.Value).FirstOrDefault();
                if (!mostPermissive.Equals(default(KeyValuePair<string, double>)))
                    return mostPermissive.Key;
                else
                    return "Nothing";
            }
        }
        [DataMember]
        public int MostPermissiveEffected
        {
            get
            {
                if (Actions == null || Actions.Count == 0)
                    return 0;
                var mostPermissive = Actions.Select(a => new KeyValuePair<string, double>(a.Name, a.ContentNodesEffected * a.Priority)).OrderByDescending(kvp => kvp.Value).FirstOrDefault();
                if (!mostPermissive.Equals(default(KeyValuePair<string, double>)))
                    return Actions.First(x => x.Name == mostPermissive.Key).ContentNodesEffected;
                else
                    return 0;
            }
        }

        public IContent StartContentNode { get; set; }

        public IMedia StartMediaNode { get; set; }

        public static Permissions GetFromCoreGroup(IUserGroup group, IContent startContent, int effectedContentNodes, EntityPermissionCollection explicitContentPermissions, IMedia startMedia, int effectedMediaNodes, bool startContentRoot = false, bool startMediaRoot = false)
        {
            var result = new Permissions() { StartContentNode = startContent, StartContent = startContent != null ? UmbracoNode.GetFromContent(startContent) : null, StartMediaNode = startMedia, StartMedia = startMedia != null ? UmbracoNode.GetFromMedia(startMedia) : null, AllowedSections = group.AllowedSections.Select(s => new BackOfficeSection(s)).ToList(), EffectedMediaNodes = effectedMediaNodes };
            if (startContentRoot)
                result.StartContent = new UmbracoNode() { Icon = "icon-home", Id = -1, Name = "Content Root", Level = -1 };
            if (startMediaRoot)
                result.StartMedia = new UmbracoNode() { Icon = "icon-home", Id = -1, Name = "Media Root", Level = -1 };
            if (group.Permissions != null)
            {
                result.Actions = new List<PermissionAction>();
                Dictionary<string, PermissionAction> permMap = new Dictionary<string, PermissionAction>();
                foreach (var permission in group.Permissions)
                {
                    var permAction = PermissionAction.GetFromActionLetter(permission);
                    if(permAction.Name != "Invalid")
                    {
                        permAction.ContentNodesEffected = effectedContentNodes;
                        permAction.ContentNodesEffected += explicitContentPermissions.Count(x => x.AssignedPermissions.ToList().Contains(permAction.Letter));
                        result.Actions.Add(permAction);
                        permMap.Add(permAction.Letter, permAction);
                    }
                }
                if (explicitContentPermissions.Count > 0)
                    result.HasExplicitPermissions = true;
                foreach (var explicitPermission in explicitContentPermissions)
                {
                    foreach (var permissionLetter in explicitPermission.AssignedPermissions)
                    {
                        if(permMap.ContainsKey(permissionLetter))
                            permMap[permissionLetter].ContentNodesEffected++;
                        else
                        {
                            var permAction = PermissionAction.GetFromActionLetter(permissionLetter);
                            if (permAction.Name != "Invalid")
                            {
                                permAction.ContentNodesEffected = 1;
                                result.Actions.Add(permAction);
                                permMap.Add(permAction.Letter, permAction);
                            }
                        }
                    }
                }
                result.Actions = result.Actions.OrderByDescending(a => a.Priority).ToList();
            }
            return result;
        }

        public static Permissions GetUserCombinedPermissions(List<Permissions> permissions, List<UmbracoNode> startContent, List<UmbracoNode> startMedia)
        {
            Permissions result = new Permissions() { };
            if (permissions == null || permissions.Count == 0)
                return new Permissions() { AllowedSections = null, Actions = null, EffectedMediaNodes = 0, HasExplicitPermissions = false, ExplicitUserStartContent = startContent, ExplicitUserStartMedia = startMedia };
            result = permissions.First(); //initialize to first record
            foreach (var perm in permissions)
            {
                result.EffectedMediaNodes = perm.EffectedMediaNodes > result.EffectedMediaNodes ? perm.EffectedMediaNodes : result.EffectedMediaNodes;
                result.HasExplicitPermissions = perm.HasExplicitPermissions || result.HasExplicitPermissions;
                if (startContent == null && perm.StartContent != null) //if no explicit start content node is set for user
                    result.StartContent = perm.StartContent.Level < (result.StartContent != null ? result.StartContent.Level : int.MaxValue) ? perm.StartContent : result.StartContent; //we prefer the start nodes closer to the root of the tree
                if (startMedia == null && perm.StartMedia != null) //if no explicit start media node is set for user
                    result.StartMedia = perm.StartMedia.Level < (result.StartMedia != null ? result.StartMedia.Level : int.MaxValue) ? perm.StartMedia : result.StartMedia; //we prefer the start nodes closer to the root of the tree
                foreach (var currentPermAction in perm.Actions)
                {
                    if(result.Actions.FirstOrDefault(a => a.Letter == currentPermAction.Letter) == null)
                    {
                        result.Actions.Add(currentPermAction);
                    }
                    else
                    {
                        if (result.Actions.FirstOrDefault(a => a.Letter == currentPermAction.Letter).ContentNodesEffected < currentPermAction.ContentNodesEffected)
                            result.Actions.FirstOrDefault(a => a.Letter == currentPermAction.Letter).ContentNodesEffected = currentPermAction.ContentNodesEffected;
                    }
                }
            }
            if(result.Actions != null)
                result.Actions = result.Actions.OrderByDescending(a => a.Priority).ToList();
            bool startContentRoot = (result.StartContent != null && result.StartContent.Id == -1) || (startContent != null && startContent.Select(x => x.Id).Contains(-1));
            bool startMediaRoot = (result.StartMedia != null && result.StartMedia.Id == -1) || (startMedia != null && startMedia.Select(x => x.Id).Contains(-1));
            if (startContentRoot)
                result.StartContent = new UmbracoNode() { Icon = "icon-home", Id = -1, Name = "Content Root", Level = -1 };
            else
            {
                if(startContent != null)
                {
                    foreach (var explicitStartContent in startContent)
                    {
                        result.StartContent = explicitStartContent.Level < (result.StartContent != null ? result.StartContent.Level : int.MaxValue) ? explicitStartContent : result.StartContent;
                    }
                }
            }
            if (startMediaRoot)
                result.StartMedia = new UmbracoNode() { Icon = "icon-home", Id = -1, Name = "Media Root", Level = -1 };
            else
            {
                if (startMedia != null)
                {
                    foreach (var explicitStartMedia in startMedia)
                    {
                        result.StartMedia = explicitStartMedia.Level < (result.StartMedia != null ? result.StartMedia.Level : int.MaxValue) ? explicitStartMedia : result.StartMedia;
                    }
                }
            }
            return result;
        }
    }

    public class PermissionAction
    {
        public string Name { get; set; }
        public string Letter { get; set; }
        public double Priority { get; set; }
        public int ContentNodesEffected { get; set; }

        public static PermissionAction GetFromActionLetter(string actionLetter)
        {
            switch (actionLetter)
            {
                case "D":
                    return new PermissionAction() { Name = "Delete", Letter = actionLetter, Priority = 0.9 };
                case "I":
                    return new PermissionAction() { Name = "Assign Domain", Letter = actionLetter, Priority = 0.9 };
                case "F":
                    return new PermissionAction() { Name = "Browse", Letter = actionLetter, Priority = 0.5 };
                case "O":
                    return new PermissionAction() { Name = "Copy", Letter = actionLetter, Priority = 0.6 };
                case "ï":
                    return new PermissionAction() { Name = "Blueprint", Letter = actionLetter, Priority = 0.5 };
                case "M":
                    return new PermissionAction() { Name = "Move", Letter = actionLetter, Priority = 0.8 };
                case "C":
                    return new PermissionAction() { Name = "Create", Letter = actionLetter, Priority = 0.8 };
                case "P":
                    return new PermissionAction() { Name = "Restrict Public Access", Letter = actionLetter, Priority = 0.5 };
                case "R":
                    return new PermissionAction() { Name = "Change Node Permissions", Letter = actionLetter, Priority = 0.8 };
                case "U":
                    return new PermissionAction() { Name = "Publish", Letter = actionLetter, Priority = 1.0 };
                case "V":
                    return new PermissionAction() { Name = "Restore", Letter = actionLetter, Priority = 0.7 };
                case "K":
                    return new PermissionAction() { Name = "Rollback", Letter = actionLetter, Priority = 0.7 };
                case "S":
                    return new PermissionAction() { Name = "Sort", Letter = actionLetter, Priority = 0.7 };
                case "H":
                    return new PermissionAction() { Name = "Send To Publish", Letter = actionLetter, Priority = 0.6 };
                case "Z":
                    return new PermissionAction() { Name = "Unpublish", Letter = actionLetter, Priority = 0.95 };
                case "A":
                    return new PermissionAction() { Name = "Update", Letter = actionLetter, Priority = 0.6 };
                default:
                    return new PermissionAction() { Name = "Invalid", Letter = actionLetter, Priority = 0.01 };
            }
        }
    }

    public class BackOfficeSection
    {
        public BackOfficeSection(string s)
        {
            Name = s.Substring(0,1).ToUpper() + s.Substring(1);
        }
        public string Name { get; set; }
    }
}
