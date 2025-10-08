using System.Collections.Immutable;
using HLab.ColorTools;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ColorPreviewAnalyzer : DiagnosticAnalyzer
{
   public const string DiagnosticId = "ColorPreview";

   private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
      DiagnosticId,
      "Color Preview",
      "Displays a preview of the color",
      "Visual",
      DiagnosticSeverity.Info,
      isEnabledByDefault: true);

   public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

   public override void Initialize(AnalysisContext context)
   {
      context.EnableConcurrentExecution();
      context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
      context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.InvocationExpression);
   }

   private void AnalyzeNode(SyntaxNodeAnalysisContext context)
   {
      var invocation = (InvocationExpressionSyntax)context.Node;

      // Detect your custom color method, e.g., `MyColor.FromArgb(r, g, b)`
      var methodSymbol = ModelExtensions.GetSymbolInfo(context.SemanticModel, invocation).Symbol as IMethodSymbol;
      if (methodSymbol?.Name == "FromArgb" && methodSymbol.ContainingType.Name == "ColorRGB")
      {
         var arguments = invocation.ArgumentList.Arguments;
         if (arguments.Count == 3 &&
             byte.TryParse(arguments[0].ToString(), out var a) &&
             byte.TryParse(arguments[1].ToString(), out var r) &&
             byte.TryParse(arguments[2].ToString(), out var g) &&
             byte.TryParse(arguments[3].ToString(), out var b))
         {
            var color = HLabColors.RGB(a,r, g, b);
            var diagnostic = Diagnostic.Create(
               Rule, 
               invocation.GetLocation(),
               properties: ImmutableDictionary<string, string>.Empty.Add("Color", $"{color.ToUInt():X8}" ));
            context.ReportDiagnostic(diagnostic);
         }
      }
   }
}