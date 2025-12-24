using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace Mystira.Shared.Generators;

/// <summary>
/// Source generator that creates IValidateOptions implementations for options classes
/// decorated with [GenerateValidator].
/// </summary>
[Generator]
public class OptionsValidatorGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Register the attribute source
        context.RegisterPostInitializationOutput(ctx =>
        {
            ctx.AddSource("GenerateValidatorAttribute.g.cs", SourceText.From(AttributeSource, Encoding.UTF8));
            ctx.AddSource("ValidateAttribute.g.cs", SourceText.From(ValidateAttributeSource, Encoding.UTF8));
        });

        // Find all classes with [GenerateValidator] attribute
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsCandidateClass(s),
                transform: static (ctx, _) => GetClassForGeneration(ctx))
            .Where(static m => m is not null);

        // Combine with compilation
        var compilationAndClasses = context.CompilationProvider.Combine(classDeclarations.Collect());

        // Generate source
        context.RegisterSourceOutput(compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right!, spc));
    }

    private static bool IsCandidateClass(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax classSyntax
            && classSyntax.AttributeLists.Count > 0;
    }

    private static ClassDeclarationSyntax? GetClassForGeneration(GeneratorSyntaxContext context)
    {
        var classSyntax = (ClassDeclarationSyntax)context.Node;

        foreach (var attributeList in classSyntax.AttributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var name = attribute.Name.ToString();
                if (name == "GenerateValidator" || name == "GenerateValidatorAttribute")
                {
                    return classSyntax;
                }
            }
        }

        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax?> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
            return;

        foreach (var classSyntax in classes.Distinct())
        {
            if (classSyntax is null)
                continue;

            var semanticModel = compilation.GetSemanticModel(classSyntax.SyntaxTree);
            var classSymbol = semanticModel.GetDeclaredSymbol(classSyntax);

            if (classSymbol is null)
                continue;

            var source = GenerateValidator(classSymbol, compilation);
            var fileName = $"{classSymbol.Name}Validator.g.cs";
            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
        }
    }

    private static string GenerateValidator(INamedTypeSymbol classSymbol, Compilation compilation)
    {
        var className = classSymbol.Name;
        var validatorName = className + "Validator";
        var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();

        var validations = new List<(string PropertyName, string PropertyType, List<string> Rules)>();

        // Find properties with validation attributes
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IPropertySymbol property)
            {
                var rules = new List<string>();

                foreach (var attribute in property.GetAttributes())
                {
                    var attrName = attribute.AttributeClass?.Name ?? "";

                    switch (attrName)
                    {
                        case "ValidateRangeAttribute":
                            var minArg = attribute.ConstructorArguments.Length > 0 ? attribute.ConstructorArguments[0].Value : null;
                            var maxArg = attribute.ConstructorArguments.Length > 1 ? attribute.ConstructorArguments[1].Value : null;
                            if (minArg != null && maxArg != null)
                            {
                                rules.Add($"if (options.{property.Name} < {minArg} || options.{property.Name} > {maxArg}) errors.Add($\"{{nameof(options.{property.Name})}} must be between {minArg} and {maxArg}\");");
                            }
                            break;

                        case "ValidatePositiveAttribute":
                            rules.Add($"if (options.{property.Name} <= 0) errors.Add($\"{{nameof(options.{property.Name})}} must be positive\");");
                            break;

                        case "ValidateNotEmptyAttribute":
                            rules.Add($"if (string.IsNullOrWhiteSpace(options.{property.Name})) errors.Add($\"{{nameof(options.{property.Name})}} cannot be empty\");");
                            break;

                        case "ValidateUrlAttribute":
                            rules.Add($"if (!string.IsNullOrEmpty(options.{property.Name}) && !Uri.TryCreate(options.{property.Name}, UriKind.Absolute, out _)) errors.Add($\"{{nameof(options.{property.Name})}} must be a valid URL\");");
                            break;

                        case "ValidateMinLengthAttribute":
                            var minLenArg = attribute.ConstructorArguments.Length > 0 ? attribute.ConstructorArguments[0].Value : null;
                            if (minLenArg != null)
                            {
                                rules.Add($"if (options.{property.Name}?.Length < {minLenArg}) errors.Add($\"{{nameof(options.{property.Name})}} must have at least {minLenArg} characters\");");
                            }
                            break;
                    }
                }

                if (rules.Count > 0)
                {
                    validations.Add((property.Name, property.Type.ToDisplayString(), rules));
                }
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("using Microsoft.Extensions.Options;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName};");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Auto-generated validator for {className}.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public class {validatorName} : IValidateOptions<{className}>");
        sb.AppendLine("{");
        sb.AppendLine($"    public ValidateOptionsResult Validate(string? name, {className} options)");
        sb.AppendLine("    {");
        sb.AppendLine("        var errors = new List<string>();");
        sb.AppendLine();

        foreach (var (propertyName, propertyType, rules) in validations)
        {
            foreach (var rule in rules)
            {
                sb.AppendLine($"        {rule}");
            }
        }

        sb.AppendLine();
        sb.AppendLine("        return errors.Count > 0");
        sb.AppendLine("            ? ValidateOptionsResult.Fail(errors)");
        sb.AppendLine("            : ValidateOptionsResult.Success;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine("/// <summary>");
        sb.AppendLine($"/// Extension methods for registering {validatorName}.");
        sb.AppendLine("/// </summary>");
        sb.AppendLine($"public static class {validatorName}Extensions");
        sb.AppendLine("{");
        sb.AppendLine($"    public static IServiceCollection Add{className}Validation(this IServiceCollection services)");
        sb.AppendLine("    {");
        sb.AppendLine($"        services.AddSingleton<IValidateOptions<{className}>, {validatorName}>();");
        sb.AppendLine("        return services;");
        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private const string AttributeSource = @"// <auto-generated/>
#nullable enable

namespace Mystira.Shared.Validation;

/// <summary>
/// Marks an options class for automatic validator generation.
/// The source generator will create an IValidateOptions implementation
/// based on validation attributes on the class properties.
/// </summary>
/// <example>
/// <code>
/// [GenerateValidator]
/// public class MyOptions
/// {
///     [ValidatePositive]
///     public int MaxRetries { get; set; } = 3;
///
///     [ValidateRange(1, 300)]
///     public int TimeoutSeconds { get; set; } = 30;
///
///     [ValidateNotEmpty]
///     public string ConnectionString { get; set; } = """";
/// }
///
/// // Generator creates MyOptionsValidator : IValidateOptions<MyOptions>
/// </code>
/// </example>
[System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class GenerateValidatorAttribute : System.Attribute
{
    /// <summary>
    /// Whether to fail fast (throw on first error) or collect all errors.
    /// Default is false (collect all errors).
    /// </summary>
    public bool FailFast { get; set; }
}
";

    private const string ValidateAttributeSource = @"// <auto-generated/>
#nullable enable

namespace Mystira.Shared.Validation;

/// <summary>
/// Validates that a numeric property is within a specified range.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ValidateRangeAttribute : System.Attribute
{
    public object Minimum { get; }
    public object Maximum { get; }

    public ValidateRangeAttribute(int minimum, int maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }

    public ValidateRangeAttribute(double minimum, double maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }
}

/// <summary>
/// Validates that a numeric property is greater than zero.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ValidatePositiveAttribute : System.Attribute
{
}

/// <summary>
/// Validates that a string property is not null, empty, or whitespace.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ValidateNotEmptyAttribute : System.Attribute
{
}

/// <summary>
/// Validates that a string property is a valid URL.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ValidateUrlAttribute : System.Attribute
{
}

/// <summary>
/// Validates that a string property has a minimum length.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class ValidateMinLengthAttribute : System.Attribute
{
    public int MinimumLength { get; }

    public ValidateMinLengthAttribute(int minimumLength)
    {
        MinimumLength = minimumLength;
    }
}
";
}
