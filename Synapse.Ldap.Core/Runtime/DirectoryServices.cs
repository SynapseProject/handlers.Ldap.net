﻿using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Text.RegularExpressions;
using System.DirectoryServices.AccountManagement;


namespace Synapse.Ldap.Core
{
    public partial class DirectoryServices
    {
        public static string GetObjectDistinguishedName(ObjectClass objectClass, string objectName, string ldapRoot)
        {
            string distinguishedName = string.Empty;

            using( DirectoryEntry entry = new DirectoryEntry( ldapRoot ) )
            using( DirectorySearcher searcher = new DirectorySearcher( entry ) )
            {
                switch( objectClass )
                {
                    case ObjectClass.User:
                    {
                        searcher.Filter = "(&(objectClass=user)(|(cn=" + objectName + ")(sAMAccountName=" + objectName + ")))";
                        break;
                    }
                    case ObjectClass.Group:
                    case ObjectClass.Computer:
                    {
                        searcher.Filter = $"(&(objectClass={objectClass.ToString().ToLower()})(|(cn=" + objectName + ")(dn=" + objectName + ")))";
                        break;
                    }
                }
                SearchResult result = searcher.FindOne();

                if( result == null )
                    throw new KeyNotFoundException( "unable to locate the distinguishedName for the object " + objectName + " in the " + ldapRoot + " ldapRoot" );

                DirectoryEntry directoryObject = result.GetDirectoryEntry();
                distinguishedName = "LDAP://" + directoryObject.Properties["distinguishedName"].Value;

                entry.Close();
            }

            return distinguishedName;
        }

        private static string GetCommonName(String distinguishedName)
        {
            Regex regex = new Regex( @"cn=(.*?),(.*)$", RegexOptions.IgnoreCase );
            Match match = regex.Match( distinguishedName );
            if ( match.Success )
                return match.Groups[1]?.Value?.Trim();
            else
                return distinguishedName;
        }

        public static PrincipalContext GetPrincipalContext(string ouPath = "")
        {
            PrincipalContext principalContext = !String.IsNullOrWhiteSpace( ouPath ) ? new PrincipalContext( ContextType.Domain, null, ouPath ) : new PrincipalContext( ContextType.Domain );
            return principalContext;
        }

    }
}