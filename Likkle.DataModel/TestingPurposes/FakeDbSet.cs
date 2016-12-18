﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace Likkle.DataModel.TestingPurposes
{
    public class FakeDbSet<T> : IDbSet<T>
    where T : class
    {
        ObservableCollection<T> _data;
        IQueryable _query;

        public FakeDbSet()
        {
            _data = new ObservableCollection<T>();
            _query = _data.AsQueryable();
        }

        public virtual T Find(params object[] keyValues)
        {
            throw new NotImplementedException("Derive from FakeDbSet<T> and override Find");
        }

        public T Add(T item)
        {
            var idProperty = item.GetType().GetProperty("Id");

            if (idProperty.GetValue(item) == null)
                idProperty.SetValue(item, Guid.NewGuid(), null);

            _data.Add(item);
            return item;
        }

        /// <summary>
        /// Adds the predefined values that have id already set up.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public T AddPredefined(T item)
        {
            _data.Add(item);
            return item;
        }

        public T Remove(T item)
        {
            _data.Remove(item);
            return item;
        }

        public T Attach(T item)
        {
            _data.Add(item);
            return item;
        }

        public T Detach(T item)
        {
            _data.Remove(item);
            return item;
        }

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public ObservableCollection<T> Local
        {
            get { return _data; }
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _query.Provider; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}

/*
 There isn’t really a good way to generically implement Find, so I’ve left it as a virtual method that throws if called. If our application makes use of the Find method we can create an implementation specific to each type.

 public class FakeDepartmentSet : FakeDbSet<Department>
{
    public override Department Find(params object[] keyValues)
    {
        return this.SingleOrDefault(d => d.DepartmentId == (int)keyValues.Single());
    }
}

public class FakeEmployeeSet : FakeDbSet<Employee>
{
    public override Employee Find(params object[] keyValues)
    {
        return this.SingleOrDefault(e => e.EmployeeId == (int)keyValues.Single());
    }
}    
*/
