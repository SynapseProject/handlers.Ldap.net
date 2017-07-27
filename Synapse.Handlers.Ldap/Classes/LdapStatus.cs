﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;
using Synapse.Core.Utilities;
using Synapse.Ldap.Core;

namespace Synapse.Handlers.Ldap
{
    public class LdapStatus
    {
        [XmlElement]
        public LdapStatusType Status { get; set; } = LdapStatusType.Success;
        [XmlElement]
        public LdapObjectType Type { get; set; }
        [XmlElement]
        public string Message { get; set; } = "Success";
        [XmlElement]
        public ActionType Action { get; set; }
        [XmlElement]
        public string Name { get; set; }
        [XmlElement]
        public string Path { get; set; }
        [XmlElement]
        public string DistinguishedName { get; set; }
        [XmlElement]
        public UserPrincipalObject User { get; set; }
        [XmlElement]
        public GroupPrincipalObject Group { get; set; }
        [XmlElement]
        public OrganizationalUnitObject OrganizationalUnit { get; set; }

        public LdapStatus() { }


        public LdapStatus(LdapStatus status)
        {
            Init( status );
        }

        public LdapStatus(LdapStatus status, UserPrincipalObject user)
        {
            Init( status );
            User = user;
        }

        public LdapStatus(LdapStatus status, GroupPrincipalObject group)
        {
            Init( status );
            Group = group;
        }

        public LdapStatus(LdapStatus status, OrganizationalUnitObject orgUnit)
        {
            Init( status );
            OrganizationalUnit = orgUnit;
        }

        private void Init(LdapStatus status)
        {
            Status = status.Status;
            Type = status.Type;
            Message = status.Message;
            Action = status.Action;
            Name = status.Name;
            Path = status.Path;
            DistinguishedName = status.DistinguishedName;
        }

    }
}
