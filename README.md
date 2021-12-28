# Simple WebDAV sync tool

Ensure a local directory is synced with a remote [WebDAV](http://www.webdav.org) directory.

## Usage

```bash
$ dotnet run -- -h

Description:
  A Simple WebDAV Sync Tool

Usage:
  webdav-sync [options]

Options:
  --remote-url <remote-url>  Remote URL to synchronize locally.
  --username <username>      Username for authentication. If not provided the `.netrc` file will be used.
  --password <password>      Password for authentication.
  --netrc <netrc>            (Optional) Location of the `.netrc` file to use. Any `username` and `password` (if supplied) will have priority over this argument.
  --output <output>          Local directory where files missing files will be downloaded. Existing files won't be updated or deleted. [default: .]
  --version                  Show version information
  -?, -h, --help             Show help and usage information
```

## Behaviours

* Any missing (non-existing) local files will be downloaded from the server.
* Existing files will be ignored; IOW
    * edited files will not be overwritten; and
    * you can avoid downloads by creating 0 length files.
* Extra local files are ignored - IOW they are not deleted, nor uploaded.
