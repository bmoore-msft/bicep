// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Bicep.Core.Extensions;
using Bicep.Core.Semantics.Namespaces;
using Bicep.Core.Syntax;
using Bicep.Core.TypeSystem;

namespace Bicep.Core.Semantics
{
    public class Binder : IBinder
    {
        private readonly SyntaxTree syntaxTree;
        private readonly ImmutableDictionary<SyntaxBase, Symbol> bindings;
        private readonly ImmutableDictionary<DeclaredSymbol, ImmutableArray<DeclaredSymbol>> cyclesBySymbol;

        public Binder(SyntaxTree syntaxTree, ISymbolContext symbolContext)
        {
            // TODO use lazy or some other pattern for init
            this.syntaxTree = syntaxTree;
            this.TargetScope = SyntaxHelper.GetTargetScope(syntaxTree);
            var (declarations, outermostScopes) = DeclarationVisitor.GetDeclarations(syntaxTree, symbolContext);
            var uniqueDeclarations = GetUniqueDeclarations(declarations);
            var builtInNamespaces = GetBuiltInNamespaces(this.TargetScope);
            this.bindings = GetBindings(syntaxTree, uniqueDeclarations, builtInNamespaces, outermostScopes);
            this.cyclesBySymbol = GetCyclesBySymbol(syntaxTree, this.bindings);

            // TODO: Avoid looping 5 times?
            this.FileSymbol = new FileSymbol(
                syntaxTree.FileUri.LocalPath,
                syntaxTree.ProgramSyntax,
                builtInNamespaces,
                outermostScopes,
                declarations.OfType<ParameterSymbol>(),
                declarations.OfType<VariableSymbol>(),
                declarations.OfType<ResourceSymbol>(),
                declarations.OfType<ModuleSymbol>(),
                declarations.OfType<OutputSymbol>());
        }

        public ResourceScope TargetScope { get; }

        public FileSymbol FileSymbol { get; }

        public SyntaxBase? GetParent(SyntaxBase syntax)
            => syntaxTree.Hierarchy.GetParent(syntax);

        /// <summary>
        /// Returns the symbol that was bound to the specified syntax node. Will return null for syntax nodes that never get bound to symbols. Otherwise,
        /// a symbol will always be returned. Binding failures are represented with a non-null error symbol.
        /// </summary>
        /// <param name="syntax">the syntax node</param>
        public Symbol? GetSymbolInfo(SyntaxBase syntax) => this.bindings.TryGetValue(syntax);

        public ImmutableArray<DeclaredSymbol>? TryGetCycle(DeclaredSymbol declaredSymbol)
            => this.cyclesBySymbol.TryGetValue(declaredSymbol, out var cycle) ? cycle : null;

        private static ImmutableDictionary<string, DeclaredSymbol> GetUniqueDeclarations(IEnumerable<DeclaredSymbol> outermostDeclarations)
        {
            // in cases of duplicate declarations we will see multiple declaration symbols in the result list
            // for simplicitly we will bind to the first one
            // it may cause follow-on type errors, but there will also be errors about duplicate identifiers as well
            return outermostDeclarations
                .ToLookup(x => x.Name, LanguageConstants.IdentifierComparer)
                .ToImmutableDictionary(x => x.Key, x => x.First(), LanguageConstants.IdentifierComparer);
        }

        private static ImmutableDictionary<string, NamespaceSymbol> GetBuiltInNamespaces(ResourceScope targetScope)
        {
            var namespaces = new NamespaceSymbol[] { new SystemNamespaceSymbol(), new AzNamespaceSymbol(targetScope) };

            return namespaces.ToImmutableDictionary(property => property.Name, property => property, LanguageConstants.IdentifierComparer);
        }

        private static ImmutableDictionary<SyntaxBase, Symbol> GetBindings(
            SyntaxTree syntaxTree,
            IReadOnlyDictionary<string, DeclaredSymbol> outermostDeclarations,
            ImmutableDictionary<string, NamespaceSymbol> builtInNamespaces,
            ImmutableArray<LocalScope> childScopes)
        {
            // bind identifiers to declarations
            var bindings = new Dictionary<SyntaxBase, Symbol>();
            var binder = new NameBindingVisitor(outermostDeclarations, bindings, builtInNamespaces, childScopes);
            binder.Visit(syntaxTree.ProgramSyntax);

            return bindings.ToImmutableDictionary();
        }

        private static ImmutableDictionary<DeclaredSymbol, ImmutableArray<DeclaredSymbol>> GetCyclesBySymbol(SyntaxTree syntaxTree, IReadOnlyDictionary<SyntaxBase, Symbol> bindings)
        {
            return CyclicCheckVisitor.FindCycles(syntaxTree.ProgramSyntax, bindings);
        }
    }
}
