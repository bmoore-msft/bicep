// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Immutable;
using System.Linq;
using Bicep.Core.Parser;
using Bicep.Core.SemanticModel;
using Bicep.Core.Syntax;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Bicep.LanguageServer.CompilationManager
{
    public class CompilationContext
    {
        public CompilationContext(Compilation compilation)
        {
            this.Compilation = compilation;
            this.DocumentUris = compilation.SyntaxTreeGrouping.FilePathDependencies.Select(DocumentUri.FromFileSystemPath).ToImmutableHashSet(DocumentUri.Comparer);
        }

        public Compilation Compilation { get; }

        public ProgramSyntax ProgramSyntax => Compilation.SyntaxTreeGrouping.EntryPoint.ProgramSyntax;

        public ImmutableArray<int> LineStarts => Compilation.SyntaxTreeGrouping.EntryPoint.LineStarts;

        public ImmutableHashSet<DocumentUri> DocumentUris { get; }
    }
}
