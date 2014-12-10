﻿using Microsoft.VisualStudio.Shell;
using NuGet.Client.Resolution;
using NuGet.Versioning;
using NuGet.VisualStudio;
using System.Collections.Generic;
using System.Management.Automation;



#if VS14
using Microsoft.VisualStudio.ProjectSystem.Interop;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
#endif

namespace NuGet.Client.VisualStudio.PowerShell
{
    /// <summary>
    /// This command consolidates the specified package into the specified project.
    /// TODO List
    /// 1. Have a spec to define the behaviors of this command in details.
    /// 2. Implement the spec
    /// </summary>
    [Cmdlet("Consolidate", "Package")]
    public class ConsolidatePackageCommand : PackageInstallBaseCommand
    {
        public ConsolidatePackageCommand() :
            base(ServiceLocator.GetInstance<IVsPackageSourceProvider>(),
                 ServiceLocator.GetInstance<IPackageRepositoryFactory>(),
                 ServiceLocator.GetInstance<SVsServiceProvider>(),
                 ServiceLocator.GetInstance<IVsPackageManagerFactory>(),
                 ServiceLocator.GetInstance<ISolutionManager>(),
                 ServiceLocator.GetInstance<IHttpClientEvents>())
        {
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            this.PackageActionResolver = new ActionResolver(ActiveSourceRepository, ResolutionContext);
        }

        protected override void PreprocessProjectAndIdentities()
        {
            this.Projects = GetAllProjectsInSolution();
            this.Identities = GetConsolidatedPackageIdentityForResolver();
        }

        private IEnumerable<PackageIdentity> GetConsolidatedPackageIdentityForResolver()
        {
            PackageIdentity identity = null;

            // If Version is specified by commandline parameter
            if (!string.IsNullOrEmpty(Version))
            {
                identity = new PackageIdentity(Id, NuGetVersion.Parse(Version));
                identity = Client.PackageRepositoryHelper.ResolvePackage(V2LocalRepository, identity, true);
            }
            else
            {
                VsProject target = this.GetProject(true);
                InstalledPackageReference installedPackage = GetInstalledReference(target, Id);
                if (installedPackage == null)
                {
                    Log(MessageLevel.Error, Resources.Cmdlet_PackageNotInstalled, Id);
                }
                else
                {
                    identity = installedPackage.Identity;
                    Version = identity.Version.ToNormalizedString();
                }
            }

            return new List<PackageIdentity>() { identity };
        }
    }
}