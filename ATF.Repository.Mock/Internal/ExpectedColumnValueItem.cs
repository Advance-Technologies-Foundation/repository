namespace ATF.Repository.Mock.Internal
{
	internal class ExpectedColumnValueItem
	{
		public string Name { get; set; }
		public object Value { get; set; }

		internal ExpectedColumnValueItem() {
			Name = string.Empty;
		}

		public bool Equals(ExpectedColumnValueItem obj) {
			return Equals(obj.Name, Name) && Equals(obj.Value, Value);
		}
	}
}
