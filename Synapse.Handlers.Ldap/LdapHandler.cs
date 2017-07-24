﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.DirectoryServices.AccountManagement;

using Synapse.Core;
using Synapse.Ldap.Core;
using Synapse.Handlers.Ldap;

public class LdapHandler : HandlerRuntimeBase
{
    LdapHandlerConfig config = null;
    LdapHandlerResults results = new LdapHandlerResults();
    bool isDryRun = false;

    public override IHandlerRuntime Initialize(string config)
    {
        //deserialize the Config from the Handler declaration
        this.config = DeserializeOrNew<LdapHandlerConfig>( config );
        return this;
    }

    public override ExecuteResult Execute(HandlerStartInfo startInfo)
    {
        int cheapSequence = 0;
        const string __context = "Execute";
        ExecuteResult result = new ExecuteResult()
        {
            Status = StatusType.Complete,
            Sequence = int.MaxValue
        };
        string msg = "Complete";
        Exception exc = null;

        isDryRun = startInfo.IsDryRun;

        //deserialize the Parameters from the Action declaration
        Synapse.Handlers.Ldap.LdapHandlerParameters parameters = base.DeserializeOrNew<Synapse.Handlers.Ldap.LdapHandlerParameters>( startInfo.Parameters );

        OnLogMessage( "Execute", $"Running Handler As User [{System.Security.Principal.WindowsIdentity.GetCurrent().Name}]" );

        try
        {
            //if IsDryRun == true, test if ConnectionString is valid and works.
            if( startInfo.IsDryRun )
            {
                OnProgress( __context, "Attempting connection", sequence: cheapSequence++ );


                result.ExitData = config.LdapRoot;
                result.Message = msg =
                    $"Connection test successful! Connection string: {config.LdapRoot}";
            }
            //else, select data as declared in Parameters.QueryString
            else
            {
                switch( config.Action )
                {
                    case ActionType.Query:
                        ProcessLdapObjects( parameters.Users, ProcessQuery );
                        ProcessLdapObjects( parameters.Groups, ProcessQuery );
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessQuery );
                        break;
                    case ActionType.Create:
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessCreate );
                        ProcessLdapObjects( parameters.Groups, ProcessCreate );
                        ProcessLdapObjects( parameters.Users, ProcessCreate );
                        break;
                    case ActionType.Modify:
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessModify );
                        ProcessLdapObjects( parameters.Groups, ProcessModify );
                        ProcessLdapObjects( parameters.Users, ProcessModify );
                        break;
                    case ActionType.Delete:
                        ProcessLdapObjects( parameters.Users, ProcessDelete );
                        ProcessLdapObjects( parameters.Groups, ProcessDelete );
                        ProcessLdapObjects( parameters.OrganizationalUnits, ProcessDelete );
                        break;
                    case ActionType.AddToGroup:
                        ProcessLdapObjects( parameters.Users, ProcessGroupAdd );
                        ProcessLdapObjects( parameters.Groups, ProcessGroupAdd );
                        break;
                    case ActionType.RemoveFromGroup:
                        ProcessLdapObjects( parameters.Users, ProcessGroupRemove );
                        ProcessLdapObjects( parameters.Groups, ProcessGroupRemove );
                        break;
                    default:
                        throw new LdapException( $"Unknown Action {config.Action} Specified", LdapStatusType.NotSupported );
                }
            }
        }
        //something wnet wrong: hand-back the Exception and mark the execution as Failed
        catch ( Exception ex )
        {
            exc = ex;
            result.Status = StatusType.Failed;
            result.ExitData = msg =
                ex.Message + " | " + ex.InnerException?.Message;
        }

        if (string.IsNullOrWhiteSpace(result.ExitData?.ToString()))
            result.ExitData = results.Serialize( config.OutputType, config.PrettyPrint );

        if (!config.SuppressOutput)
            OnProgress( __context, result.ExitData?.ToString(), result.Status, sequence: cheapSequence++, ex: exc );

        //final runtime notification, return sequence=Int32.MaxValue by convention to supercede any other status message
        OnProgress( __context, msg, result.Status, sequence: int.MaxValue, ex: exc );

        return result;
    }

    // TODO : Implement Me
    public override object GetConfigInstance()
    {
        throw new NotImplementedException();
    }

    // TODO : Implement Me
    public override object GetParametersInstance()
    {
        throw new NotImplementedException();
    }

    private void ProcessLdapObjects(IEnumerable<LdapObject> objs, Action<LdapObject, bool> processFunction)
    {
        if ( objs != null )
        {
            if ( config.RunSequential )
            {
                foreach ( LdapObject obj in objs )
                    processFunction( obj, config.ReturnObjects );
            }
            else
                Parallel.ForEach( objs, obj =>
                {
                    processFunction( obj, config.ReturnObjects );
                } );
        }
    }

    private void ProcessQuery(LdapObject obj, bool returnObject = true)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Status = LdapStatusType.Success,
            Message = "Success",
            Name = obj.Name,
            Path = obj.Path,
            DistinguishedName = obj.DistinguishedName
        };

        try
        {
            object ldapObject = GetLdapObject( obj );
            switch ( obj.Type )
            {
                case ObjectClass.User:
                    if ( returnObject )
                        results.Add( status, (UserPrincipalObject)ldapObject );
                    else
                        results.Add( status, (UserPrincipalObject)null );
                    break;
                case ObjectClass.Group:
                    if ( returnObject )
                        results.Add( status, (GroupPrincipalObject)ldapObject );
                    else
                        results.Add( status, (GroupPrincipalObject)null );
                    break;
                case ObjectClass.OrganizationalUnit:
                    if ( returnObject )
                        results.Add( status, (OrganizationalUnitObject)ldapObject );
                    else
                        results.Add( status, (OrganizationalUnitObject)null );

                    break;
                default:
                    throw new LdapException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", LdapStatusType.NotSupported );
            }
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessQuery", e.Message );
            OnLogMessage( "ProcessQuery", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private object GetLdapObject(LdapObject obj)
    {
        switch ( obj.Type )
        {
            case ObjectClass.User:
                LdapUser user = (LdapUser)obj;
                UserPrincipalObject upo = null;
                if (!string.IsNullOrWhiteSpace(user.DistinguishedName))
                    upo = DirectoryServices.GetUser( user.DistinguishedName, config.QueryGroupMembership );
                else
                    upo = DirectoryServices.GetUser( user.Name, config.QueryGroupMembership );
                return upo;
            case ObjectClass.Group:
                LdapGroup group = (LdapGroup)obj;
                GroupPrincipalObject gpo = null;
                if ( !String.IsNullOrWhiteSpace( group.DistinguishedName ) )
                    gpo = DirectoryServices.GetGroup( group.DistinguishedName, config.QueryGroupMembership );
                else
                    gpo = DirectoryServices.GetGroup( group.Name, config.QueryGroupMembership );
                return gpo;
            case ObjectClass.OrganizationalUnit:
                LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                OrganizationalUnitObject ouo = null;
                if ( !string.IsNullOrWhiteSpace( ou.DistinguishedName ) )
                    ouo = DirectoryServices.GetOrganizationalUnit( ou.DistinguishedName );
                else
                    ouo = DirectoryServices.GetOrganizationalUnit( ou.Name, ou.Path );
                return ouo;
            default:
                throw new LdapException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", LdapStatusType.NotSupported );
        }
    }

    private void ProcessCreate(LdapObject obj, bool returnObject = true)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Status = LdapStatusType.Success,
            Message = "Success",
            Name = obj.Name,
            Path = obj.Path,
            DistinguishedName = obj.DistinguishedName
        };

        try
        {
            object ldapObject = null;

            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    if ( !string.IsNullOrWhiteSpace( user.DistinguishedName ) )
                        DirectoryServices.CreateUser( user.DistinguishedName, user.Password, user.GivenName, user.Surname, user.Description, true, isDryRun, config.UseUpsert );
                    else
                        DirectoryServices.CreateUser( user.Name, user.Path, user.Password, user.GivenName, user.Surname, user.Description, true, isDryRun, config.UseUpsert );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Name + "] Created." );
                    if ( user.Groups != null )
                        ProcessGroupAdd( user, false );
                    if ( returnObject )
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (UserPrincipalObject)ldapObject );
                    }
                    else
                        results.Add( status, (UserPrincipalObject)null );
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    if ( !String.IsNullOrWhiteSpace( group.DistinguishedName ) )
                        DirectoryServices.CreateGroup( group.DistinguishedName, group.Description, group.Scope, group.IsSecurityGroup, isDryRun, config.UseUpsert );
                    else
                        DirectoryServices.CreateGroup( group.Name, group.Path, group.Description, group.Scope, group.IsSecurityGroup, isDryRun, config.UseUpsert );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Name + "] Created." );
                    if ( group.Groups != null )
                        ProcessGroupAdd( group, false );
                    if ( returnObject )
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (GroupPrincipalObject)ldapObject );
                    }
                    else
                        results.Add( status, (GroupPrincipalObject)null );
                    break;
                case ObjectClass.OrganizationalUnit:
                    LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                    if ( !string.IsNullOrWhiteSpace( ou.DistinguishedName ) )
                        DirectoryServices.CreateOrganizationUnit( ou.DistinguishedName, ou.Description, isDryRun, config.UseUpsert );
                    else
                        DirectoryServices.CreateOrganizationUnit( ou.Name, ou.Path, ou.Description, isDryRun, config.UseUpsert );
                    OnLogMessage( "ProcessCreate", obj.Type + " [" + obj.Name + "] Created." );
                    if ( returnObject )
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (OrganizationalUnitObject)ldapObject );
                    }
                    else
                        results.Add( status, (OrganizationalUnitObject)null );
                    break;
                default:
                    throw new LdapException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", LdapStatusType.NotSupported );
            }
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessCreate", e.Message );
            OnLogMessage( "ProcessCreate", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private void ProcessModify(LdapObject obj, bool returnObject = true)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Status = LdapStatusType.Success,
            Message = "Success",
            Name = obj.Name,
            Path = obj.Path,
            DistinguishedName = obj.DistinguishedName
        };

        try
        {
            object ldapObject = null;

            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    if ( !string.IsNullOrWhiteSpace( user.DistinguishedName ) )
                        DirectoryServices.ModifyUser( user.DistinguishedName, user.Password, user.GivenName, user.Surname, user.Description, true, isDryRun, config.UseUpsert );
                    else
                        DirectoryServices.ModifyUser( user.Name, user.Path, user.Password, user.GivenName, user.Surname, user.Description, true, isDryRun, config.UseUpsert );
                    OnLogMessage( "ProcessModify", obj.Type + " [" + obj.Name + "] Modified." );
                    if ( user.Groups != null )
                        ProcessGroupAdd( user, false );
                    if ( returnObject )
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (UserPrincipalObject)ldapObject );
                    }
                    else
                        results.Add( status, (UserPrincipalObject)null );
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    if ( !String.IsNullOrWhiteSpace( group.DistinguishedName ) )
                        DirectoryServices.ModifyGroup( group.DistinguishedName, group.Description, group.Scope, group.IsSecurityGroup, isDryRun, config.UseUpsert );
                    else
                        DirectoryServices.ModifyGroup( group.Name, group.Path, group.Description, group.Scope, group.IsSecurityGroup, isDryRun, config.UseUpsert );
                    OnLogMessage( "ProcessModify", obj.Type + " [" + obj.Name + "] Modified." );
                    if ( group.Groups != null )
                        ProcessGroupAdd( group, false );
                    if ( returnObject )
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (GroupPrincipalObject)ldapObject );
                    }
                    else
                        results.Add( status, (GroupPrincipalObject)null );
                    break;
                case ObjectClass.OrganizationalUnit:
                    LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                    if ( !string.IsNullOrWhiteSpace( ou.DistinguishedName ) )
                        DirectoryServices.ModifyOrganizationUnit( ou.DistinguishedName, ou.Description, isDryRun, config.UseUpsert);
                    else
                        DirectoryServices.ModifyOrganizationUnit( ou.Name, ou.Path, ou.Description, isDryRun, config.UseUpsert );
                    OnLogMessage( "ProcessModify", obj.Type + " [" + obj.Name + "] Modified." );
                    if ( returnObject )
                    {
                        ldapObject = GetLdapObject( obj );
                        results.Add( status, (OrganizationalUnitObject)ldapObject );
                    }
                    else
                        results.Add( status, (OrganizationalUnitObject)null );
                    break;
                default:
                    throw new LdapException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", LdapStatusType.NotSupported );
            }
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessCreate", e.Message );
            OnLogMessage( "ProcessCreate", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private void ProcessDelete(LdapObject obj, bool returnObject = false)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Status = LdapStatusType.Success,
            Message = "Success",
            Name = obj.Name,
            Path = obj.Path,
            DistinguishedName = obj.DistinguishedName
        };

        try
        {
            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    if (!string.IsNullOrWhiteSpace(user.DistinguishedName))
                        DirectoryServices.DeleteUser( user.DistinguishedName );
                    else
                        DirectoryServices.DeleteUser( user.Name );
                    results.Add( status, (UserPrincipalObject)null );
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    if ( !String.IsNullOrWhiteSpace( group.DistinguishedName ) )
                        DirectoryServices.DeleteGroup( group.DistinguishedName, isDryRun );
                    else
                        DirectoryServices.DeleteGroup( group.Name, isDryRun );
                    results.Add( status, (GroupPrincipalObject)null );
                    break;
                case ObjectClass.OrganizationalUnit:
                    LdapOrganizationalUnit ou = (LdapOrganizationalUnit)obj;
                    if (!string.IsNullOrWhiteSpace(ou.DistinguishedName))
                        DirectoryServices.DeleteOrganizationUnit( ou.DistinguishedName );
                    else
                        DirectoryServices.DeleteOrganizationUnit( ou.Name, ou.Path );
                    results.Add( status, (OrganizationalUnitObject)null );
                    break;
                default:
                    throw new LdapException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", LdapStatusType.NotSupported );
            }

            String message = $"{obj.Type} [{obj.Name}] Deleted.";
            OnLogMessage( "ProcessDelete", message );
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessDelete", e.Message );
            OnLogMessage( "ProcessDelete", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private void ProcessGroupAdd(LdapObject obj, bool returnObject = true)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Status = LdapStatusType.Success,
            Message = "Success",
            Name = obj.Name,
            Path = obj.Path,
            DistinguishedName = obj.DistinguishedName
        };

        try
        {
            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    String userName = (String.IsNullOrWhiteSpace( user.DistinguishedName )) ? user.Name : user.DistinguishedName;
                    foreach ( string userGroup in user.Groups )
                    {
                        DirectoryServices.AddUserToGroup( userName, userGroup, isDryRun );
                        String userMessage = $"{obj.Type} [{userName}] Added To Group [{userGroup}].";
                        OnLogMessage( "ProcessGroupAdd", userMessage );
                        status.Message = userMessage;
                        results.Add( status, (UserPrincipalObject)null );
                    }
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    String groupName = (String.IsNullOrWhiteSpace( group.DistinguishedName )) ? group.Name : group.DistinguishedName;
                    foreach ( string groupGroup in group.Groups )
                    {
                        DirectoryServices.AddGroupToGroup( groupName, groupGroup, isDryRun );
                        String groupMessage = $"{obj.Type} [{groupName}] Added To Group [{groupGroup}].";
                        OnLogMessage( "ProcessGroupAdd", groupMessage );
                        status.Message = groupMessage;
                        results.Add( status, (GroupPrincipalObject)null );
                    }
                    break;
                default:
                    throw new LdapException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", LdapStatusType.NotSupported );
            }

            if ( returnObject )
                ProcessQuery( obj, true );
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessGroupAdd", e.Message );
            OnLogMessage( "ProcessGroupAdd", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }

    }

    private void ProcessGroupRemove(LdapObject obj, bool returnObject = true)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = config.Action,
            Status = LdapStatusType.Success,
            Message = "Success",
            Name = obj.Name,
            Path = obj.Path,
            DistinguishedName = obj.DistinguishedName
        };

        try
        {
            switch ( obj.Type )
            {
                case ObjectClass.User:
                    LdapUser user = (LdapUser)obj;
                    String userName = (String.IsNullOrWhiteSpace( user.DistinguishedName )) ? user.Name : user.DistinguishedName;
                    foreach ( string userGroup in user.Groups )
                    {
                        DirectoryServices.RemoveUserFromGroup( userName, userGroup, isDryRun );
                        String userMessage = $"{obj.Type} [{userName}] Removed From Group [{userGroup}].";
                        OnLogMessage( "ProcessGroupRemove", userMessage );
                        status.Message = userMessage;
                        results.Add( status, (UserPrincipalObject)null );
                    }
                    break;
                case ObjectClass.Group:
                    LdapGroup group = (LdapGroup)obj;
                    String groupName = (String.IsNullOrWhiteSpace( group.DistinguishedName )) ? group.Name : group.DistinguishedName;
                    foreach ( string groupGroup in group.Groups )
                    {
                        DirectoryServices.RemoveGroupFromGroup( groupName, groupGroup, isDryRun );
                        String groupMessage = $"{obj.Type} [{groupName}] Removed From Group [{groupGroup}].";
                        OnLogMessage( "ProcessGroupRemove", groupMessage );
                        status.Message = groupMessage;
                        results.Add( status, (GroupPrincipalObject)null );
                    }
                    break;
                default:
                    throw new LdapException( "Action [" + config.Action + "] Not Implemented For Type [" + obj.Type + "]", LdapStatusType.NotSupported );
            }

            if ( returnObject )
                ProcessQuery( obj, true );
        }
        catch ( LdapException ex )
        {
            ProcessLdapException( ex, config.Action, obj );
        }
        catch ( Exception e )
        {
            OnLogMessage( "ProcessGroupRemove", e.Message );
            OnLogMessage( "ProcessGroupRemove", e.StackTrace );
            LdapException le = new LdapException( e );
            ProcessLdapException( le, config.Action, obj );
        }
    }

    private void ProcessLdapException(LdapException ex, ActionType action, LdapObject obj)
    {
        LdapStatus status = new LdapStatus()
        {
            Action = action,
            Status = ex.Type,
            Message = ex.Message,
            Name = obj.Name,
            Path = obj.Path,
            DistinguishedName = obj.DistinguishedName
        };

        switch ( obj.Type )
        {
            case ObjectClass.User:
                results.Add( status, (UserPrincipalObject)null );
                break;
            case ObjectClass.Group:
                results.Add( status, (GroupPrincipalObject)null );
                break;
            case ObjectClass.OrganizationalUnit:
                results.Add( status, (OrganizationalUnitObject)null );
                break;
            default:
                throw ex;
        }

        OnLogMessage( "Exception", ex.Message );
    }

}