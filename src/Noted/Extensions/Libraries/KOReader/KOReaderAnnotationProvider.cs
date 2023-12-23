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
        if (!this.IsAvailable(sourcePath))
        {
            yield break;
        }

        var annotationFiles = this.fileSystem.GetFiles(sourcePath, ".lua");
        foreach (var annotation in annotationFiles)
        {
            using var lua = new Lua();
            lua.State.Encoding = Encoding.UTF8;
            var annotationTable = GetLuaTable(lua, lua.DoFile(annotation)[0]);
            if (!annotationTable.TryGetValue("bookmarks", out var bookmarkNode) || bookmarkNode == null)
            {
                continue;
            }

            var bookmarksTable = GetLuaTable(lua, bookmarkNode);
            var highlightTable = GetLuaTable(lua, annotationTable["highlight"]);
            var highlights = highlightTable.Values
                .SelectMany(h => GetLuaTable(lua, h).Values)
                .Select(h => GetLuaTable(lua, h)["pos0"].ToString())
                .ToHashSet();
            var documentTable = GetLuaTable(lua, annotationTable["doc_props"]);
            var document = new DocumentReference
            {
                Title = documentTable["title"].ToString() ?? Path.GetFileName(annotationTable["doc_path"].ToString()!),
                Author = documentTable["authors"].ToString() ?? string.Empty
            };

            // Highlights are keyed to the page numbers on the device used for reading.
            // Sort them by page numbers to preserve the reading order of annotations.
            foreach (var bookmark in bookmarksTable.Values)
            {
                var bookmarkDict = GetLuaTable(lua, bookmark);
                if (!bookmarkDict.TryGetValue("highlighted", out var highlighted) || highlighted is bool == false)
                {
                    // Skip non-highlighted bookmarks
                    continue;
                }

                // ["notes"] field is available for both notes and highlights.
                // ["text"] field is available only for custom text attached to the note.
                var notes = bookmarkDict["notes"].ToString()!;
                var highlightDate = DateTime.Parse(bookmarkDict["datetime"].ToString()!);
                var pos0 = bookmarkDict["pos0"].ToString();
                var pos1 = bookmarkDict["pos1"].ToString();
                bookmarkDict.TryGetValue("chapter", out var chapterTitle);
                var epubXPath = new EpubXPathLocation(pos0!, pos1!);
                var context = new AnnotationContext()
                {
                    SerializedLocation = epubXPath.ToString(),
                    DocumentSection = new DocumentSection(chapterTitle?.ToString() ?? string.Empty, 0, 0, null)
                };
                yield return new Annotation(
                        notes,
                        document,
                        AnnotationType.Highlight,
                        context,
                        highlightDate);

                // Notes are always attached to a highlight. We emit an extra annotation in this case.
                if (bookmarkDict.TryGetValue("text", out var text) && text != null && highlights.Contains(pos0) && !text.ToString()!.StartsWith("Page "))
                {
                    yield return new Annotation(
                        text.ToString()!,
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