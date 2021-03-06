﻿using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Synapse.ActiveDirectory.Core
{
    public class GroupPrincipalObject : PrincipalObject, ICloneable
    {
        public GroupPrincipalObject() { }
        public GroupPrincipalObject(GroupPrincipal gp, bool getAccessRules = false, bool getObjectProperties = true)
        {
            SetPropertiesFromGroupPrincipal( gp, getAccessRules, getObjectProperties );
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }


        //
        // Summary:
        //     Gets or sets a Nullable System.DirectoryServices.AccountManagement.GroupScope
        //     enumeration that specifies the scope for this group principal.
        //
        // Returns:
        //     A nullable System.DirectoryServices.AccountManagement.GroupScope enumeration
        //     value that specifies the scope of this group or null if no scope has been set.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The application may not set this property to null.
        public GroupScope? GroupScope { get; set; }
        //
        // Summary:
        //     Gets or sets a Nullable Boolean value that indicates whether the group is security-enabled.
        //
        // Returns:
        //     true if the group is security enabled, or null if the group has not been persisted;
        //     otherwise false.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     The application may not set this property to null.
        public bool? IsSecurityGroup { get; set; }
        //
        // Summary:
        //     Gets a collection of principal objects that represent the members of the group.
        //
        // Returns:
        //     A System.DirectoryServices.AccountManagement.PrincipalCollection object that
        //     contains the principal objects that represent the members of the group.
        public List<PrincipalObject> Members { get; set; }

        public SerializableDictionary<string, List<string>> Properties { get; set; }


        public static GroupPrincipalObject FromGroupPrincipal(GroupPrincipal gp)
        {
            return new GroupPrincipalObject( gp );
        }

        public void SetPropertiesFromGroupPrincipal(GroupPrincipal gp, bool getAccessRules, bool getObjectProperties)
        {
            if( gp == null ) return;

            SetPropertiesFromPrincipal( gp, getAccessRules );

            object obj = gp.GetUnderlyingObject();
            if ( obj.GetType() == typeof( DirectoryEntry ) && getObjectProperties )
            {
                DirectoryEntry gde = (DirectoryEntry)obj;
                Properties = DirectoryServices.GetProperties( gde );
            }


            GroupScope = gp.GroupScope;
            IsSecurityGroup = gp.IsSecurityGroup;

            if( gp.Members?.Count > 0 )
            {
                Members = new List<PrincipalObject>();
                foreach( Principal p in gp.Members )
                    Members.Add( new PrincipalObject( p ) );
            }
        }
    }
}