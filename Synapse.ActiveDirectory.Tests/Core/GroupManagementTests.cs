﻿using System;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

using NUnit.Framework;

using Synapse.ActiveDirectory.Core;

namespace Synapse.ActiveDirectory.Tests.Core
{
    [TestFixture]
    public class GroupManagementTests
    {
        DirectoryEntry workspace = null;
        String workspaceName = null;
        GroupPrincipal group = null;

        [SetUp]
        public void Setup()
        {
            // Setup Workspace
            workspace = Utility.CreateWorkspace();
            workspaceName = workspace.Properties["distinguishedName"].Value.ToString();
            group = Utility.CreateGroup( workspaceName );
        }

        [TearDown]
        public void TearDown()
        {
            // Cleanup Workspace
            Utility.DeleteGroup( group.DistinguishedName );
            Utility.DeleteWorkspace( workspaceName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_GroupManagementTest()
        {
            // Add User To Group
            UserPrincipal user = Utility.CreateUser( workspaceName );
            UserPrincipalObject upo = DirectoryServices.GetUser( user.DistinguishedName, true, false, false );
            int beforeCount = upo.Groups.Count;
            Console.WriteLine( $"Adding User [{user.Name}] To Group [{group.Name}]." );
            DirectoryServices.AddUserToGroup( user.DistinguishedName, group.DistinguishedName );
            upo = DirectoryServices.GetUser( user.DistinguishedName, true, false, false );
            int afterCount = upo.Groups.Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount + 1 ) );

            // Remove User From Group
            beforeCount = afterCount;
            Console.WriteLine( $"Removing User [{user.Name}] From Group [{group.Name}]." );
            DirectoryServices.RemoveUserFromGroup( user.DistinguishedName, group.DistinguishedName );
            upo = DirectoryServices.GetUser( user.DistinguishedName, true, false, false );
            afterCount = upo.Groups.Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount - 1 ) );

            // Delete User
            Utility.DeleteUser( user.DistinguishedName );

            // Add Group To Group
            GroupPrincipal newGroup = Utility.CreateGroup( workspaceName );
            GroupPrincipalObject gpo = DirectoryServices.GetGroup( newGroup.DistinguishedName, true, false, false );
            beforeCount = gpo.Groups.Count;
            Console.WriteLine( $"Adding Group [{newGroup.Name}] To Group [{group.Name}]." );
            DirectoryServices.AddGroupToGroup( newGroup.DistinguishedName, group.DistinguishedName );
            gpo = DirectoryServices.GetGroup( newGroup.DistinguishedName, true, false, false );
            afterCount = gpo.Groups.Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount + 1 ) );

            // Remove Group From Group
            beforeCount = afterCount;
            Console.WriteLine( $"Removing Group [{newGroup.Name}] From Group [{group.Name}]." );
            DirectoryServices.RemoveGroupFromGroup( newGroup.DistinguishedName, group.DistinguishedName );
            gpo = DirectoryServices.GetGroup( newGroup.DistinguishedName, true, false, false );
            afterCount = gpo.Groups.Count;
            Assert.That( afterCount, Is.EqualTo( beforeCount - 1 ) );

            // Delete Groups
            Utility.DeleteGroup( newGroup.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_AddUserToNonExistantGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            UserPrincipal up = Utility.CreateUser( workspaceName );

            Console.WriteLine( $"Adding User [{up.DistinguishedName}] To Group [{groupDistinguishedName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.AddUserToGroup( up.DistinguishedName, groupDistinguishedName ) ).Message.Contains( "cannot be found" );

            Utility.DeleteUser( up.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_AddGroupToNonExistantGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            GroupPrincipal gp = Utility.CreateGroup( workspaceName );

            Console.WriteLine( $"Adding Group [{gp.DistinguishedName}] To Group [{groupDistinguishedName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.AddGroupToGroup( gp.DistinguishedName, groupDistinguishedName ) ).Message.Contains( "cannot be found" );

            Utility.DeleteGroup( gp.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_AddNonExistantUserToGroup()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"OU={userName},{workspaceName}";

            Console.WriteLine( $"Adding User [{userDistinguishedName}] Which Should Not Exist To Group [{group.DistinguishedName}]." );
            Assert.Throws<AdException>( () => DirectoryServices.AddGroupToGroup( userDistinguishedName, group.DistinguishedName ) ).Message.Contains( "cannot be found" );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_AddNonExistantGroupToGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";

            Console.WriteLine( $"Adding Group [{groupDistinguishedName}] Which Should Not Exist To Group [{group.DistinguishedName}]." );
            Assert.Throws<AdException>( () => DirectoryServices.AddGroupToGroup( groupDistinguishedName, group.DistinguishedName ) ).Message.Contains( "cannot be found" );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveUserFromNonExistantGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            UserPrincipal up = Utility.CreateUser( workspaceName );

            Console.WriteLine( $"Removing User [{up.DistinguishedName}] From Group [{groupDistinguishedName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.RemoveUserFromGroup( up.DistinguishedName, groupDistinguishedName ) ).Message.Contains( "cannot be found" );

            Utility.DeleteUser( up.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveGroupFromNonExistantGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";
            GroupPrincipal gp = Utility.CreateGroup( workspaceName );

            Console.WriteLine( $"Removing Group [{gp.DistinguishedName}] From Group [{groupDistinguishedName}] Which Should Not Exist." );
            Assert.Throws<AdException>( () => DirectoryServices.RemoveGroupFromGroup( gp.DistinguishedName, groupDistinguishedName ) ).Message.Contains( "cannot be found" );

            Utility.DeleteGroup( gp.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveNonExistantUserFromGroup()
        {
            // Get User That Does Not Exist
            String userName = $"testuser_{Utility.GenerateToken( 8 )}";
            String userDistinguishedName = $"OU={userName},{workspaceName}";

            Console.WriteLine( $"Removing User [{userDistinguishedName}] Which Should Not Exist From Group [{group.DistinguishedName}]." );
            Assert.Throws<AdException>( () => DirectoryServices.RemoveUserFromGroup( userDistinguishedName, group.DistinguishedName ) ).Message.Contains( "cannot be found" );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveNonExistantGroupFromGroup()
        {
            // Get Group That Does Not Exist
            String groupName = $"testgroup_{Utility.GenerateToken( 8 )}";
            String groupDistinguishedName = $"OU={groupName},{workspaceName}";

            Console.WriteLine( $"Removing Group [{groupDistinguishedName}] Which Should Not Exist From Group [{group.DistinguishedName}]." );
            Assert.Throws<AdException>( () => DirectoryServices.RemoveGroupFromGroup( groupDistinguishedName, group.DistinguishedName ) ).Message.Contains( "cannot be found" );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveUserFromGroupWhenNotMember()
        {
            UserPrincipal up = Utility.CreateUser( workspaceName );
            Assert.Throws<AdException>( () => DirectoryServices.RemoveUserFromGroup( up.DistinguishedName, group.DistinguishedName ) ).Message.Contains( "does not exist in the group" );
            Utility.DeleteUser( up.DistinguishedName );
        }

        [Test, Category( "Core" ), Category( "GroupManagement" )]
        public void Core_RemoveGroupFromGroupWhenNotMember()
        {
            GroupPrincipal gp = Utility.CreateGroup( workspaceName );
            Assert.Throws<AdException>( () => DirectoryServices.RemoveGroupFromGroup( gp.DistinguishedName, group.DistinguishedName ) ).Message.Contains( "does not exist in the group" );
            Utility.DeleteGroup( gp.DistinguishedName );
        }

    }
}
