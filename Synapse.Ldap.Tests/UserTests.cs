﻿using System;
using System.DirectoryServices.AccountManagement;
using NUnit.Framework;
using Synapse.Ldap.Core;

namespace Synapse.Ldap.Tests
{
    /// <summary>
    /// Summary description for UserTests
    /// </summary>
    [TestFixture]
    public class UserTests
    {
        [Test]
        public void MoveUserToOrganizationUnitSucceeds()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainDistinguishedName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
            
            string destOrgUnit = $"OU=TestOU,{DirectoryServices.GetDomainDistinguishedName()}";

            // Act
            Console.WriteLine("Moving user to destination OU...");
            bool status = DirectoryServices.MoveUserToOrganizationUnit($"CN={userName},{DirectoryServices.GetDomainDistinguishedName()}", destOrgUnit);

            // Assert
            Assert.IsTrue(status);
        }


        [Test]
        public void MoveUserToOrganizationUnitWithInvalidOrganizationUnitFails()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainDistinguishedName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
            string destOrgUnit = $"OU=XXXX,{DirectoryServices.GetDomainDistinguishedName()}";

            // Act
            Console.WriteLine("Moving user to destination OU...");
            bool status = DirectoryServices.MoveUserToOrganizationUnit($"CN={userName},{DirectoryServices.GetDomainDistinguishedName()}", destOrgUnit);

            // Assert
            Assert.IsFalse(status);
        }

        [Test]
        public void SetUserPassword_Without_Username_Throw_Exception()
        {
            // Arrange 
            string username = "";
            string newPassword = "O4l8e73d0sYyOzh";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.SetUserPassword(username, newPassword));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Username is not specified."));
        }


        [Test]
        public void SetUserPassword_Without_Password_Throw_Exception()
        {
            // Arrange 
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string newPassword = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.SetUserPassword(username, newPassword));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("New password is not specified."));
        }

        [Test]
        public void SetUserPassword_Non_Existent_User_Throw_Exception()
        {
            // Arrange 
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string newPassword = "O4l8e73d0sYyOzh";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.SetUserPassword(username, newPassword));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void SetUserPassword_Non_Existent_User_Dry_Run_Throw_Exception()
        {
            // Arrange 
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string newPassword = "O4l8e73d0sYyOzh";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.SetUserPassword(username, newPassword, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void SetUserPassword_With_Valid_User_And_New_Password_Succeed()
        {
            // Arrange 
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string initialPassword = "3J84Ot2FDj9q4Ig";
            string newPassword = "O4l8e73d0sYyOzh";

            // Act
            Console.WriteLine($"Creating user {username}...");
            DirectoryServices.CreateUser("", username, initialPassword, username, username, "Created by Synapse");

            // Assert
            Console.WriteLine($"Setting new password for {username}...");
            Assert.DoesNotThrow(() => DirectoryServices.SetUserPassword(username, newPassword, true));
        }

        //        [Test]
        //        public void AddUserToGroupReturnSuccess()
        //        {
        //            // Arrange 
        //            string ldapPath = DirectoryServices.GetDomainDistinguishedName();
        //            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
        //            string userPassword = "bi@02LL49_VWQ{b";
        //            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
        //            string groupDn = $"CN=TestGroup,{ldapPath}";
        //
        //            // Act
        //            bool status = DirectoryServices.AddUserToGroup($"CN={userName},{ldapPath}", groupDn);
        //
        //            // Assert
        //            Assert.IsTrue(status);
        //        }
        //
        //        [Test]
        //        public void AddUserToGroupWithInvalidGroupReturnFailure()
        //        {
        //            // Arrange 
        //            string ldapPath = DirectoryServices.GetDomainDistinguishedName();
        //            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
        //            string userPassword = "bi@02LL49_VWQ{b";
        //            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
        //            string groupDn = $"CN=XXXXXX,{ldapPath}";
        //
        //            // Act
        //            bool status = DirectoryServices.AddUserToGroup($"CN={userName},{ldapPath}", groupDn);
        //
        //            // Assert
        //            Assert.IsFalse(status);
        //        }

        //        [Test]
        //        public void RemoveUserFromGroupReturnSuccess()
        //        {
        //            // Arrange 
        //            string ldapPath = DirectoryServices.GetDomainDistinguishedName();
        //            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
        //            string userPassword = "bi@02LL49_VWQ{b";
        //            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
        //            string groupDn = $"CN=TestGroup,{ldapPath}";
        //
        //            // Act
        //            bool status = DirectoryServices.RemoveUserFromGroup($"CN={userName},{ldapPath}", groupDn);
        //
        //            // Assert
        //            Assert.IsTrue(status);
        //        }

        //        [Test]
        //        public void RemoveUserFromGroupWithInvalidGroupReturnFailure()
        //        {
        //            // Arrange 
        //            string ldapPath = DirectoryServices.GetDomainDistinguishedName();
        //            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
        //            string userPassword = "bi@02LL49_VWQ{b";
        //            DirectoryServices.CreateUser(ldapPath, userName, userPassword);
        //            string groupDn = $"CN=XXXXXX,{ldapPath}";
        //
        //            // Act
        //            bool status = DirectoryServices.RemoveUserFromGroup($"CN={userName},{ldapPath}", groupDn);
        //
        //            // Assert
        //            Assert.IsFalse(status);
        //        }

        [Test]
        public void DeleteUserReturnSuccess()
        {
            // Arrange 
            string ldapPath = DirectoryServices.GetDomainDistinguishedName();
            string userName = $"User-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            DirectoryServices.CreateUser(ldapPath, userName, userPassword);

            // Act
            bool status = DirectoryServices.DeleteUser(userName);

            // Assert
            Assert.IsTrue(status);
        }

        [Test]
        public void CreateUser_Without_Username_Throw_Exception()
        {
            // Arrange 
            string ldapPath = "";
            string username = "";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Username is not specified."));
        }

        [Test]
        public void CreateUser_Without_Password_Throw_Exception()
        {
            // Arrange 
            string ldapPath = "";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Password is not specified."));
        }

        [Test]
        public void CreateUser_Without_Given_Name_Throw_Exception()
        {
            // Arrange 
            string ldapPath = "";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = "";
            string surname = username;
            string description = "Created by Synapse";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Given name is not specified."));
        }

        [Test]
        public void CreateUser_Without_Surname_Throw_Exception()
        {
            // Arrange 
            string ldapPath = "";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = "";
            string description = "Created by Synapse";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Surname is not specified."));
        }

        [Test]
        public void CreateUser_With_Complex_Password_Suceed()
        {
            // Arrange 
            string ldapPath = "";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description);

            // Assert
            Assert.IsTrue(DirectoryServices.IsExistingUser(username));
        }

        [Test]
        public void CreateUser_With_Non_Complex_Password_Throw_Exception()
        {
            // Arrange 
            string ldapPath = "";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "XXX";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("The password does not meet the password policy requirements."));
        }

        [Test]
        public void CreateUser_With_Complex_Password_Dry_Run_User_Not_Created()
        {
            // Arrange 
            string ldapPath = "";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description, true, true);

            // Assert
            Assert.IsFalse(DirectoryServices.IsExistingUser(username));
        }

        [Test]
        public void CreateUser_Without_OU_Path_Default_To_Member_Of_Users()
        {
            // Arrange 
            string ldapPath = "";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";

            // Act
            Console.WriteLine($"Creating user {username}...");
            DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description);
            UserPrincipal userPrincipal = DirectoryServices.GetUser(username);

            // Assert
            Assert.IsTrue(userPrincipal.DistinguishedName.StartsWith($"CN={username},CN=Users"));
        }

        [Test]
        public void CreateUser_With_Valid_OU_Path_Succeed()
        {
            // Arrange 
            string ldapPath = $"OU=Synapse,{DirectoryServices.GetDomainDistinguishedName()}";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";

            // Act
            Console.WriteLine($"Creating user {username}...");
            DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description);
            UserPrincipal userPrincipal = DirectoryServices.GetUser(username);

            // Assert
            Assert.IsTrue(userPrincipal.DistinguishedName.StartsWith($"CN={username},OU=Synapse"));
        }

        [Test]
        public void CreateUser_With_Invalid_OU_Path_Throw_Exception()
        {
            // Arrange 
            string ldapPath = $"OU=XXX,{DirectoryServices.GetDomainDistinguishedName()}";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = username;
            string surname = username;
            string description = "Created by Synapse";

            // Act
            Console.WriteLine($"Creating user {username}...");
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("OU path specified is not valid."));
        }

        [Test]
        public void CreateUser_With_Given_Name_And_Surname_Generate_Expected_Display_Name()
        {
            // Arrange 
            string ldapPath = $"";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = "XXX";
            string surname = "YYY";
            string description = "Created by Synapse";

            // Act
            Console.WriteLine($"Creating user {username}...");
            DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description);
            UserPrincipal userPrincipal = DirectoryServices.GetUser(username);

            // Assert
            Assert.That(userPrincipal.DisplayName, Is.EqualTo($"{surname}, {givenName}"));
        }

        [Test]
        public void CreateUser_Existing_User_Throw_Exception()
        {
            // Arrange 
            string ldapPath = $"";
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string userPassword = "bi@02LL49_VWQ{b";
            string givenName = "XXX";
            string surname = "YYY";
            string description = "Created by Synapse";

            // Act
            Console.WriteLine($"Creating user {username}...");
            DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description);
            Console.WriteLine($"Recreating the same user...");
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.CreateUser(ldapPath, username, userPassword, givenName, surname, description));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("The user already exists."));
        }

        [Test]
        public void DeleteUserWithInvalidDetailReturnFailure()
        {
            // Arrange 

            // Act
            bool status = DirectoryServices.DeleteUser("XXXXXXX");

            // Assert
            Assert.IsFalse(status);
        }

        [Test]
        public void DisableUserAccount_Without_Username_Throw_Exception()
        {
            // Arrange
            string username = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.DisableUserAccount(username));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Username is not provided."));
        }

        [Test]
        public void DisableUserAccount_Non_Existent_User_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.DisableUserAccount(username));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void DisableUserAccount_Non_Existent_User_Dry_Run_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.DisableUserAccount(username, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }


        [Test]
        public void DisableUserAccount_Existing_User_Succeed()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";
            string groupName = $"TestGroup-{DirectoryServices.GenerateToken(8)}";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description);
            DirectoryServices.DisableUserAccount(username);

            // Assert
            Assert.IsFalse(DirectoryServices.IsUserEnabled(username));
        }

        [Test]
        public void EnableUserAccount_Without_Username_Throw_Exception()
        {
            // Arrange
            string username = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.EnableUserAccount(username));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Username is not provided."));
        }

        [Test]
        public void EnableUserAccount_Non_Existent_User_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.EnableUserAccount(username));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void EnableUserAccount_Non_Existent_User_Dry_Run_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.EnableUserAccount(username, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void EnableUserAccount_Existing_User_Succeed()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description, false);
            DirectoryServices.EnableUserAccount(username);

            // Assert
            Assert.IsTrue(DirectoryServices.IsUserEnabled(username));
        }

        [Test]
        public void ExpireUserPassword_Without_Username_Throw_Exception()
        {
            // Arrange
            string username = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.ExpireUserPassword(username));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Username is not provided."));
        }

        [Test]
        public void ExpireUserPassword_Non_Existent_User_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.ExpireUserPassword(username));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void ExpireUserPassword_Non_Existent_User_Dry_Run_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.ExpireUserPassword(username, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void ExpireUserPassword_Existing_User_Succeed()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description);

            // Act
            DirectoryServices.ExpireUserPassword(username);

            // Assert
            UserPrincipal userPrincipal = DirectoryServices.GetUser(username);
            Assert.IsNull(userPrincipal.LastPasswordSet);
        }

        [Test]
        public void UnlockUserAccount_Without_Username_Throw_Exception()
        {
            // Arrange
            string username = "";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.UnlockUserAccount(username));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("Username is not specified."));
        }

        [Test]
        public void UnlockUserAccount_Non_Existent_User_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.UnlockUserAccount(username));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void UnlockUserAccount_Non_Existent_User_Dry_Run_Throw_Exception()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";

            // Act
            Exception ex = Assert.Throws<Exception>(() => DirectoryServices.UnlockUserAccount(username, true));

            // Assert
            Assert.That(ex.Message, Is.EqualTo("User cannot be found."));
        }

        [Test]
        public void UnlockUserAccount_Existing_User_Succeed()
        {
            // Arrange
            string username = $"TestUser-{DirectoryServices.GenerateToken(8)}";
            string givenName = "TestUser";
            string surname = "Synapse";
            string password = "1x034abe5A#1!";
            string description = "Created by Synapse";

            // Act
            DirectoryServices.CreateUser("", username, password, givenName, surname, description);

            // Act
            DirectoryServices.UnlockUserAccount(username);

            // Assert
            UserPrincipal userPrincipal = DirectoryServices.GetUser(username);
            Assert.IsFalse(userPrincipal.IsAccountLockedOut());
        }



    }
}
