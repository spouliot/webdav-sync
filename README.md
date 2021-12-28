# Simple WebDAV sync tool

Ensure a local directory is synced with a remote [WebDAV](http://www.webdav.org) directory.

## Usage

```bash
$ dotnet run -- url username password local-directory
```

## Behaviours

* Any missing (non-existing) local files will be downloaded from the server.
* Existing files will be ignored; IOW
    * edited files will not be overwritten; and
    * you can avoid downloads by creating 0 length files.
* Extra local files are ignored - IOW they are not deleted, nor uploaded.
