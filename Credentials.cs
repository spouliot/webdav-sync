using System.Net;

namespace WebDavSync;

public class Credentials {

	static public NetworkCredential? Get (string host, string? userSuppliedUsername, string? userSuppliedPassword, string? userSuppliedNetrcFile)
	{
		if (userSuppliedUsername is not null)
			return new NetworkCredential (userSuppliedUsername, userSuppliedPassword);

		if (userSuppliedNetrcFile is not null)
			return FromNetrcFile (host, userSuppliedNetrcFile);

		var netrcFile = Environment.GetEnvironmentVariable ("NETRC");
		if (netrcFile is not null)
			return FromNetrcFile (host, netrcFile);

		var home = Environment.GetEnvironmentVariable ("HOME");
		if (home is not null)
			return FromNetrcFile (host, Path.Combine (home, ".netrc"));

		return null;
	}

	static NetworkCredential? FromNetrcFile (string host, string file)
	{
		if (!File.Exists (file))
			return null;

		var content = File.ReadAllText (file);
		var tokens = content.Split (new char [] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < tokens.Length; i++) {
			if (tokens [i++] == "machine") {
				if (tokens [i++] == host) {
					string? login = null;
					string? password = null;
					for (; i < tokens.Length; i++) {
						switch (tokens [i]) {
						case "machine":
							return null;
						case "login":
							login = tokens [++i];
							break;
						case "password":
							password = tokens [++i];
							break;
						}
						if (login is not null && password is not null)
							return new NetworkCredential (login, password);
					}
				}
			}
		}
		return null;
	}
}
