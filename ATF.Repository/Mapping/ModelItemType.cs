namespace ATF.Repository.Mapping
{
	using System;

	internal enum ModelItemType
	{
		Column,
		Detail,
		Lookup,

		[Obsolete("Will be removed in 1.3.0")]
		Reference
	}
}
