namespace ATF.Repository
{
	using System;
	using System.Collections.Generic;
	using Terrasoft.Core;
	public interface IRepository
	{
		UserConnection UserConnection { set; }

		T GetItem<T>(Guid id) where T : BaseModel;

		List<T> GetItems<T>(string filterPropertyName, Guid filterValue) where T : BaseModel;

		T CreateItem<T>() where T : BaseModel;

		void DeleteItem<T>(T model) where T : BaseModel;

		void Save();

	}
}
