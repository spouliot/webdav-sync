using System.Net;
using WebDav;

var uri = new Uri (args [0]);

var parameters = new WebDavClientParams {
	BaseAddress = uri,
	Credentials = new NetworkCredential (args [1], args [2])
};

var local = args [3];
if (!Directory.Exists (local)) {
	return 1;
}

using var client = new WebDavClient (parameters);

var files = new List<string> ();
var dirs = new Queue<string> ();
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
	var path = Uri.UnescapeDataString (Path.Combine (local, file [1..]));
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