﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using System.Net.Http;

using Synapse.Core;
using Synapse.Services;
using Synapse.Services.LdapApi;
using Synapse.Ldap.Core;
using Synapse.Core.Utilities;
using Synapse.Handlers.Ldap;

public partial class LdapApiController : ApiController
{
    [HttpGet]
    [Route( "ou/{distinguishedname}" )]
    public LdapHandlerResults GetOrgUnit(string distinguishedname)
    {
        string planName = @"QueryOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( distinguishedname ), distinguishedname );
        pe.DynamicParameters.Add( "name", String.Empty );
        pe.DynamicParameters.Add( "path", String.Empty );

        return CallPlan( planName, pe );
    }

    [HttpGet]
    [Route( "ou/{name}/{path}" )]
    public LdapHandlerResults GetOrgUnit(string name, string path)
    {
        string planName = @"QueryOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( nameof( path ), path );
        pe.DynamicParameters.Add( "distinguishedname", String.Empty );

        return CallPlan( planName, pe );
    }

    [HttpDelete]
    [Route( "ou/{name}" )]
    public LdapHandlerResults DeleteOrgUnit(string name)
    {
        string planName = @"DeleteOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );

        return CallPlan( planName, pe );
    }

    [HttpPost]
    [Route( "ou/{name}" )]
    public LdapHandlerResults CreateOrgUnit(string name, OrganizationalUnitObject ou)
    {
        string planName = @"CreateOrgUnit";

        StartPlanEnvelope pe = new StartPlanEnvelope() { DynamicParameters = new Dictionary<string, string>() };
        pe.DynamicParameters.Add( nameof( name ), name );
        pe.DynamicParameters.Add( @"path", ou.Path );

        return CallPlan( planName, pe );
    }

}