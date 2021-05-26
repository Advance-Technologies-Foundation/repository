namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using Terrasoft.Core;

	public interface IRepository
	{
		bool UseAdminRight { get; set; }

		UserConnection UserConnection { set; }

		IChangeTracker ChangeTracker { get; }

		T GetItem<T>(Guid id) where T : BaseModel, new();

		List<T> GetItems<T>(string filterPropertyName, Guid filterValue) where T : BaseModel, new();

		T CreateItem<T>() where T : BaseModel, new();

		void DeleteItem<T>(T model) where T : BaseModel;

		void Save();

	}

}
