// Copyright (c) Arun Mahapatra. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Noted.Extensions.Libraries.KOReader;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NLua;
using Noted.Core.Extensions;
using Noted.Core.Models;
using Noted.Core.Platform.IO;

public class KOReaderAnnotationProvider(IFileSystem fileSystem, ILogger logger) : IAnnotationProvider
{
    private readonly ILogger logger = logger;

    private readonly IFileSystem fileSystem = fileSystem;

    public bool IsAvailable(string sourcePath)
    {
        return this.fileSystem.GetFiles(sourcePath, ".lua").Any();
    }

    public IEnumerable<Annotation> GetAnnotations(string sourcePath)
    {
        this.logger.Info("KOReaderAnnotationProvider: Get annotations for '{0}'", sourcePath);
        if (!this.IsAvailable(sourcePath))
        {
            this.logger.Debug("KOReaderAnnotationProvider: Path not available. Skip.");
            yield break;
        }

        var bookMetadataFiles = this.fileSystem.GetFiles(sourcePath, ".lua");
        foreach (var metadata in bookMetadataFiles)
        {
            this.logger.Debug("KOReaderAnnotationProvider: Try parse metadata: '{0}'.", metadata);
            if (!metadata.EndsWith("metadata.epub.lua"))
            {
                this.logger.Error("KOReaderAnnotationProvider: Skip non epub metadata.");
                continue;
            }

            using var lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;
            var metadataDict = GetLuaTable(lua, lua.DoFile(metadata)[0]);

            var documentTable = GetLuaTable(lua, metadataDict["doc_props"]);
            var fileName = Path.GetFileName(metadataDict["doc_path"].ToString()!);
            var document = new DocumentReference
            {
                Title = documentTable.TryGetValue("title", out var title) ? title?.ToString() ?? fileName : fileName,
                Author = documentTable.TryGetValue("authors", out var authors) ? authors?.ToString() ?? string.Empty : string.Empty
            };

            // Annotations are supported from KOReader v2024.07 release onwards. See the notes
            // in https://github.com/koreader/koreader/releases/tag/v2024.07.
            if (!metadataDict.TryGetValue("annotations", out var annotationsObj))
            {
                this.logger.Error("KOReaderAnnotationProvider: Skip older metadata format.");
                continue;
            }

            var annotationTable = GetLuaTable(lua, annotationsObj);

            // Annotations table is organized as follows
            // { [sequenceNumber] = { highlight, ... }, ... }
            //
            // Note that two highlights in same page will be assigned sequence numbers as
            // per their position in the page. This is independent of time of highlighting.
            foreach (var kv in annotationTable)
            {
                var sequenceNumber = Convert.ToInt32(kv.Key);
                var annotationDict = GetLuaTable(lua, kv.Value);

                // Skip non notes or highlights
                if (!annotationDict.ContainsKey("pos0"))
                {
                    continue;
                }

                // ["text"] field is available the highlight.
                // ["notes"] field is available for custom notes.
                var text = annotationDict["text"].ToString()!;
                var highlightDate = DateTime.Parse(annotationDict["datetime"].ToString()!);
                var pos0 = annotationDict["pos0"].ToString()!;
                var pos1 = annotationDict["pos1"].ToString()!;
                var pageNumber = Convert.ToInt32(annotationDict["pageno"]);
                annotationDict.TryGetValue("chapter", out var chapterTitle);

                var epubXPath = new EpubXPathLocation(pos0, pos1, pageNumber, sequenceNumber);
                var context = new AnnotationContext()
                {
                    SerializedLocation = epubXPath.ToString(),
                    DocumentSection = new DocumentSection(chapterTitle?.ToString() ?? string.Empty, 0, 0, null),
                    PageNumber = pageNumber,
                    Location = epubXPath.GetLocation()
                };

                yield return new Annotation(
                        text,
                        document,
                        AnnotationType.Highlight,
                        context,
                        highlightDate);

                // Notes are always attached to a highlight. We emit an extra annotation in this case.
                if (annotationDict.TryGetValue("notes", out var notes) && notes != null &&
                    !string.IsNullOrEmpty(pos0) &&
                    !notes.ToString()!.StartsWith("Page "))
                {
                    yield return new Annotation(
                        notes.ToString()!,
                        document,
                        AnnotationType.Note,
                        context,
                        highlightDate);
                }
            }
        }
    }

    private static Dictionary<object, object> GetLuaTable(Lua lua, object table) => table is LuaTable luaTable ? lua.GetTableDict(luaTable) : [];
}
