namespace ATF.Repository.Mock.Internal
{
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using System.Text.RegularExpressions;
	using Terrasoft.Common;

	internal interface IPathItem
	{
	}

	internal class DetailPathItem: IPathItem
	{
		public string Path { get; set; }
		public DataTable MasterDataTable { get; set; }
		public DataColumn MasterDataColumn { get; set; }
		public DataTable DetailDataTable { get; set; }
		public DataColumn DetailDataColumn { get; set; }
	}

	internal class ColumnPathItem: IPathItem
	{
		public DataTable DataTable { get; set; }
		public DataColumn DataColumn { get; set; }
		public LookupColumnMetaData LookupColumnMetaData { get; set; }

		public bool IsLookup() {
			return LookupColumnMetaData != null;
		}

		public DataTable LookupTable =>
			IsLookup() && DataTable.DataSet.Tables.Contains(LookupColumnMetaData.ReferenceSchemaName)
				? DataTable.DataSet.Tables[LookupColumnMetaData.ReferenceSchemaName]
				: null;
	}

	internal class ColumnPath
	{
		private string _path;
		public string Path =>
			!string.IsNullOrEmpty(_path)
				? _path
				: (_path = ComparePath());

		public readonly List<ColumnPathItem> PathItems;

		internal ColumnPath(List<ColumnPathItem> pathItems) {
			PathItems = pathItems;
		}
		private string ComparePath() {
			return string.Join(PathParser.PathSeparator, PathItems.Select(x => x.DataColumn.ColumnName));
		}
	}

	internal class SchemaPath
	{
		private readonly List<IPathItem> _pathItems;

		private ColumnPath _first;
		public ColumnPath First => _first ?? (_first = CreateFirst());

		private DetailPathItem _detailPart;
		public DetailPathItem DetailPart => _detailPart ?? (_detailPart = GetDetailPathItem());

		private ColumnPath _last;
		public ColumnPath Last => _last ?? (_last = CreateLast());

		private string _fullDetailPath;

		public string FullDetailPath => string.IsNullOrEmpty(_fullDetailPath)
			? (_fullDetailPath = GetFullDetailPath())
			: _fullDetailPath;


		internal SchemaPath(List<IPathItem> pathItems) {
			_pathItems = pathItems;
		}

		private ColumnPath CreateLast() {
			var pathItems = new List<ColumnPathItem>();
			_pathItems.SkipWhile(x => x is ColumnPathItem).SkipWhile(x=>x is DetailPathItem).ForEach(x => {
				if (x is ColumnPathItem columnPathItem) {
					pathItems.Add(columnPathItem);
				}
			});

			return pathItems.Any()
				? new ColumnPath(pathItems)
				: null;
		}

		private string GetFullDetailPath() {
			var list = new List<string>();
			if (First != null && !string.IsNullOrEmpty(First.Path)) {
				list.Add(First.Path);
			}

			if (DetailPart != null && !string.IsNullOrEmpty(DetailPart.Path)) {
				list.Add(DetailPart.Path);
			}
			return string.Join(PathParser.PathSeparator, list);
		}

		private ColumnPath CreateFirst() {
			var pathItems = new List<ColumnPathItem>();
			_pathItems.TakeWhile(x => x is ColumnPathItem).ForEach(x => {
				if (x is ColumnPathItem columnPathItem) {
					pathItems.Add(columnPathItem);
				}
			});
			return new ColumnPath(pathItems);
		}

		private DetailPathItem GetDetailPathItem() {
			var item = _pathItems.FirstOrDefault(x => x is DetailPathItem);
			return item as DetailPathItem;
		}

	}

	internal static class PathParser
	{
		private const string DetailRegex = @"\[([a-zA-Z]+)+:+([a-zA-Z]+)+:*([a-zA-Z]+)*\]";

		internal const string PathSeparator = ".";

		private static bool TryGetDetailPathItem(DataTable dataTable, string path, out DetailPathItem detailPathItem) {
			detailPathItem = new DetailPathItem() {
				MasterDataTable = dataTable
			};

			var math = Regex.Match(path, DetailRegex, RegexOptions.IgnoreCase);
			if (!math.Success || math.Groups.Count != 4) {
				return false;
			}
			var detailTableName = math.Groups[1].Value;
			var detailColumnName = math.Groups[2].Value;
			var masterColumnName = !string.IsNullOrEmpty(math.Groups[3].Value)
				? math.Groups[2].Value
				: DataStore.DefaultPrimaryValueColumnName;
			if (string.IsNullOrEmpty(detailTableName) || string.IsNullOrEmpty(detailColumnName) ||
				!dataTable.DataSet.Tables.Contains(detailTableName) || !dataTable.Columns.Contains(masterColumnName)) {
				return false;
			}
			detailPathItem.MasterDataColumn = dataTable.Columns[masterColumnName];
			detailPathItem.DetailDataTable = dataTable.DataSet.Tables[detailTableName];
			if (!detailPathItem.DetailDataTable.Columns.Contains(detailColumnName)) {
				return false;
			}
			detailPathItem.DetailDataColumn = detailPathItem.DetailDataTable.Columns[detailColumnName];
			detailPathItem.Path = path;
			return true;
		}

		private static bool TryGetColumnPathItem(DataTable dataTable, string path, out ColumnPathItem columnPathItem) {
			columnPathItem = new ColumnPathItem() {
				DataTable = dataTable
			};
			if (!dataTable.Columns.Contains(path)) {
				return false;
			}
			columnPathItem.DataColumn = dataTable.Columns[path];
			columnPathItem.LookupColumnMetaData = dataTable.GetLookupRelationship(path);
			return true;
		}

		public static SchemaPath Parse(DataTable dataTable, string path) {
			var list = new List<IPathItem>();
			var contextTable = dataTable;
			path.Split(PathSeparator[0]).ForEach(pathPart => {
				if (TryGetColumnPathItem(contextTable, pathPart, out var columnPathItem)) {
					list.Add(columnPathItem);
					if (columnPathItem.IsLookup()) {
						contextTable = columnPathItem.LookupTable;
					}
				} else if (TryGetDetailPathItem(contextTable, pathPart, out var detailPathItem)) {
					contextTable = detailPathItem.DetailDataTable;
					list.Add(detailPathItem);
				}
			});

			return new SchemaPath(list);
		}
	}
}
