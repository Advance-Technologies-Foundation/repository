namespace ATF.Repository.Mock.Internal
{
	using System.IO;
	using System.Text;

	#region Class: BaseDataFileParser

	internal abstract class BaseDataFileParser
	{
		#region Fields: Private

		private static readonly Encoding DefaultEncoding = new UTF8Encoding(false, true);

		#endregion

		#region Methods: Protected

		protected string GetFileContent(string path, Encoding encoding = null) {
			var content = File.ReadAllBytes(path);
			using (var ms = new MemoryStream(content)) {
				using (var sr = new StreamReader(ms, encoding ?? DefaultEncoding)) {
					return sr.ReadToEnd();
				}
			}
		}

		#endregion
	}

	#endregion
}