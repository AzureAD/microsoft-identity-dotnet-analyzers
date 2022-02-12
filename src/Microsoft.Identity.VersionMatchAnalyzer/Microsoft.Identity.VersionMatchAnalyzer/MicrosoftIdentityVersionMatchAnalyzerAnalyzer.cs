using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Microsoft.Identity.VersionMatchAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MicrosoftIdentityVersionMatchAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "MicrosoftIdentityVersionMatchAnalyzer";

        private static DiagnosticDescriptor AssembliesVersionMismatch { get; } =
       new DiagnosticDescriptor(
           DiagnosticId,
           "Multiple versions of Microsoft.IdentityModel.* are referenced",
           "The project '{0}' has Microsoft.IdentityModel.* references that differ in version number: {1}",
           category: "Maintainability",
           defaultSeverity: DiagnosticSeverity.Warning,
           isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(AssembliesVersionMismatch); } }

        public override void Initialize(AnalysisContext context)
        {

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterCompilationAction(ctxt =>
            {
                //Debugger.Launch();
                var compilation = ctxt.Compilation;
                var assemblies = compilation.ReferencedAssemblyNames;
                int count = assemblies.Count();
                var wilsonAssemblies = new List<AssemblyIdentity>();

                foreach (var assembly in assemblies)
                {
                    if (assembly.Name.StartsWith("Microsoft.IdentityModel.") || assembly.Name.StartsWith("System.IdentityModel."))
                    {
                        wilsonAssemblies.Add(assembly);
                    }
                }

                var wilsonVersion = wilsonAssemblies.Any() ? wilsonAssemblies.First().Version : null;
                var message = new StringBuilder();
                if (wilsonVersion != null)
                {
                    foreach (var item in wilsonAssemblies)
                    {
                        message.Append(item.Name + ":" + item.Version);
                        message.Append(";");
                    }

                    if (!wilsonAssemblies.All(x => x.Version == wilsonVersion))
                    {
                        ctxt.ReportDiagnostic(
                            Diagnostic.Create(
                                AssembliesVersionMismatch,
                                null,
                                compilation.AssemblyName,
                                message
                                ));
                    }
                }

                return;
            });
        }
    }
}
