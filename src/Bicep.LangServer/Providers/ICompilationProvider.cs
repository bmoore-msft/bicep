// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
using System.Collections.Generic;
using Bicep.LanguageServer.CompilationManager;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace Bicep.LanguageServer.Providers
{
    public interface ICompilationProvider
    {
        CompilationContext Create(DocumentUri documentUri, string text);

        CompilationContext Update(CompilationContext existingContext, DocumentUri documentUri, IEnumerable<DocumentUri> updatedUris);
    }
}
