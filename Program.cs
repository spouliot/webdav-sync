using WebDav;

namespace WebDavSync;

static class Program {

	/// <summary>
	/// A Simple WebDAV Sync Tool
	/// </summary>
	/// <param name="remoteUrl">Remote URL to synchronize locally.</param>
	/// <param name="username">Username for authentication. If not provided the `.netrc` file will be used.</param>
	/// <param name="password">Password for authentication.</param>
	/// <param name="netrc">(Optional) Location of the `.netrc` file to use. Any `user-name` and `password` (if supplied) will have priority over this argument.</param>
	/// <param name="output">Local directory where files missing files will be downloaded. Existing files won't be updated or deleted.</param>
	static async Task<int> Main (string remoteUrl, string? username, string? password, string? netrc, string output = ".")
	{
		if (!Directory.Exists (output)) {
			Console.WriteLine ($"Output directory '{output}' does not exist.");
			return 1;
		}

		try {
			Uri uri = new (remoteUrl);

			WebDavClientParams parameters = new () {
				BaseAddress = uri,
				Credentials = Credentials.Get (uri.Host, username, password, netrc),
			};

			return await Synchronize (uri, parameters, output);
		}
		catch (Exception e) {
			Console.WriteLine ($"FATAL EXCEPTION: {e}");
			return 1;
		}
	}

	static async Task<int> Synchronize (Uri uri, WebDavClientParams parameters, string output)
	{
		using WebDavClient client = new (parameters);

		List<string> files = new ();
		Queue<string> dirs = new ();
		dirs.Enqueue (uri.AbsolutePath);

		while (dirs.Count > 0) {
			var dir = dirs.Dequeue ();
			var result = await client.Propfind (dir);
			if (result.IsSuccessful) {
				int n = 0;
				foreach (var item in result.Resources) {
					// first entry will be the requested directory
					if (n++ == 0)
						continue;
					if (item.IsCollection) {
						dirs.Enqueue (item.Uri);
					} else {
						files.Add (item.Uri);
					}
				}
			} else {
				Console.WriteLine ($"Error #{result.StatusCode} querying {dir}");
			}
		}

		foreach (var file in files) {
			// check if file exists locally, unescaping the path (e.g. %20 -> ' ')
			var path = Uri.UnescapeDataString (Path.Combine (output, file [1..]));
			if (File.Exists (path)) {
				Console.WriteLine ($"{path} exists locally");
			} else {
				var dir = Path.GetDirectoryName (path);
				if (dir is not null)
					Directory.CreateDirectory (dir);
				using var response = await client.GetRawFile (file);
				response.Stream.CopyTo (File.OpenWrite (path));
				Console.WriteLine ($"{path} downloaded");
			}
		}
		return 0;
	}
}
