// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using System.Collections.Immutable;
using Bicep.Core.Syntax;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Bicep.LanguageServer.CompilationManager
{
    public interface ICompilationManager
    {
        CompilationContext? UpsertCompilation(DocumentUri uri, int? version, string text);

        void UpdateCompilationsWithReferences(ImmutableHashSet<DocumentUri> uris);

        void CloseCompilation(DocumentUri uri);

        CompilationContext? GetCompilation(DocumentUri uri);

        IEnumerable<DocumentUri> GetCompilationReferences(DocumentUri uri);
    }
}

