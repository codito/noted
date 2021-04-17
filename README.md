# Noted

**TL;DR** noted is a command line app to liberate your highlights and notes.

Noted extracts annotations embedded into documents (pdf), or collects them from
readers (kindle). It tries to align them with the chapters and context to
produce a plain text markdown file.

## Features

**✓** Extracts annotations (highlights and notes) for documents and books  
**✓** Extracts _context_ and _chapter headings_ along with the annotations  
**✓** Supports `pdf` and `mobi` files  
**✓** Detects kindle `My Clippings.txt` files  
**✓** Saves all the extracted information in markdown text

## Installation

If you've .NET Core 5.0 or above already installed, simply download the
`Noted.<version>.Portable.<os>.zip` file from the [latest release][release].

If you do not wish to install .NET Core 5.0, try the self contained app from
[latest release][release].

- Windows: `Noted.<version>.win-x64.zip`
- Linux: `Noted.<version>.linux-x64.zip`

[release]: https://github.com/codito/noted/releases

## Usage

```
Noted:
  Extracts highlights and notes from documents and save them as markdown

Usage:
  Noted [options] <sourcePath> <outputPath>

Arguments:
  <sourcePath>    Source document or directory of documents to extract annotations
  <outputPath>    Destination file or directory

Options:
  -c, --context     extract the paragraph containing an annotation [default: False]
  -t, --toc         extract table of contents and align annotations [default: True]
  -v, --verbose     enable verbose logging [default: False]
  --version         Show version information
  -?, -h, --help    Show help and usage information
```

### Examples

**Extract a kindle library**

First, connect your kindle over usb and provide the path to `documents`
directory which contains `My Clippings.txt` file.

```
$ ./Noted /run/media/arun/Kindle/documents /tmp/kindle

> Extracting /run/media/arun/Kindle/documents/Epictetus/Enchiridion, The - Epictetus.mobi
 ✓ The Enchiridion by Epictetus
 ✓ 49 annotations in 58 sections
 ✓ Saved to /tmp/kindle/Enchiridion, The - Epictetus.md

Completed in 14.11s.
```

Now open the output file `/tmp/kindle/Enchiridion, The - Epictetus.md` and enjoy
your notes.

**Extract highlights from a pdf**

```
$ ./Noted /home/arun/papers/Chambliss_1989_The\ Mundanity\ of\ Excellence.pdf /tmp/kindle

> Extracting /home/arun/papers/Chambliss_1989_The Mundanity of Excellence.pdf
 ✓ The Mundanity of Excellence: An Ethnographic Report on Stratification and Olympic Swimmers
 ✓ 6 annotations
 ✓ Saved to /tmp/kindle/Chambliss_1989_The Mundanity of Excellence.md

Completed in 1.71s.
```

## Roadmap

- Support for `kfx` and `azw3` files in kindle
- Support for additional readers like `koreader`, `kobo` etc.

Contributions in any form e.g. bug reports, feature requests or PRs are most
welcome!

## License

MIT
