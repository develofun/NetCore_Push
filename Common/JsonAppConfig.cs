using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace NetCore_PushServer
{
	public class JsonAppConfig
	{
		public static T Load<T>(string filePath, int retryParentDirectory = 5)
		{
			var appDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
			string path = Path.Combine(appDirectory, filePath);

			path = SearchParentDirectory(path, retryParentDirectory);
			if (path == null) return default(T);

			var sb = new StringBuilder();

			string[] lines = File.ReadAllLines(path);
			foreach (var line in lines)
			{
				var text = line.RemoveComment();
				sb.AppendLine(text);
			}

			string json = sb.ToString();
			if (json == null)
				throw new FileNotFoundException(filePath);

			return JsonConvert.DeserializeObject<T>(json);
		}

		public static string SearchParentDirectory(string filePath, int retryParentDirectory = 5)
		{
			if (File.Exists(filePath))
				return filePath;

			string dir = Path.GetDirectoryName(filePath);
			string fileName = Path.GetFileName(filePath);

			for (int i = 1; i <= retryParentDirectory; i++)
			{
				dir = Path.Combine(dir, "..");
				filePath = Path.Combine(dir, fileName);

				if (File.Exists(filePath))
					return filePath;
			}

			return null;
		}
	}
}
