# Changelog

## Unreleased

## [3.0.1](https://github.com/codito/noted/compare/v3.0.0...v3.0.1) (2024-12-29)


### Bug Fixes

* publish on release create ([cd35157](https://github.com/codito/noted/commit/cd351573ed02f6e275884bb88f87e720947b1525))

## [3.0.0](https://github.com/codito/noted/compare/v2.1.0...v3.0.0) (2024-12-28)


### ⚠ BREAKING CHANGES

* update to latest koreader format. Deprecate KOReader versions older than v2024.07.

### Features

* update to latest koreader format. Deprecate KOReader versions older than v2024.07. ([3eab795](https://github.com/codito/noted/commit/3eab7958e644d1cf521687f7ff428434e4109458))


### Bug Fixes

* quote titles in YAML frontmatter to escape special chars. ([35fc3f9](https://github.com/codito/noted/commit/35fc3f9ad1af398da88f69c7e521f01b5b283331))

## v2.1.0 - 2024-01-06

- Fix: add annotation start and end time for documents in KOReader library.
- Feature: page numbers for annotations in KOReader library.

## v2.0.0 - 2023-12-25

- Support KOReader library for annotations.
- Fix: extract Markdown for documents with annotations.
- Fix: document section and annotation alignment to output only sections with annotations.

## v1.0.1 - 2021-04-17

- Fix: `--toc` command argument should be true by default.
- Fix: overwrite existing markdown file.

## v1.0.0

First release of Noted \o/

- Extract annotations, context and chapters for documents.
- Support pdf and mobi file readers.
- Support markdown output.
