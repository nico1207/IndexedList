using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace IndexedList
{
    public class IndexedList<T>: ICollection<T>, IList<T>
	{
		private readonly List<T> list;
		private readonly Dictionary<Func<T, object>, MultiValueDictionary<object, T>> propertyDictionaries;
		private readonly Dictionary<string, Func<T, object>> accessorMap;

		/// <summary>
		/// Create new indexed list.
		/// </summary>
		/// <param name="indices">Properties that will be used as indices</param>
		/// <exception cref="ArgumentException">Throws argument exception if a given expression is not a valid property</exception>
		public IndexedList(params Expression<Func<T, object>>[] indices)
		{
			list = new List<T>();
			propertyDictionaries = new Dictionary<Func<T, object>, MultiValueDictionary<object, T>>();
			accessorMap = new Dictionary<string, Func<T, object>>();

			foreach (var expression in indices)
			{
				if (expression.Body is UnaryExpression unaryExpression)
				{
					var memberExpression = (MemberExpression)unaryExpression.Operand;
					var propertyInfo = (PropertyInfo) memberExpression.Member;
					var lambda = expression.Compile();
					propertyDictionaries.Add(lambda, new MultiValueDictionary<object, T>());
					accessorMap.Add(propertyInfo.Name, lambda);
				}
				else if (expression.Body is MemberExpression memberExpression)
				{
					var propertyInfo = (PropertyInfo) memberExpression.Member;
					var lambda = expression.Compile();
					propertyDictionaries.Add(expression.Compile(), new MultiValueDictionary<object, T>());
					accessorMap.Add(propertyInfo.Name, lambda);
				}
				else
				{
					throw new ArgumentException("Expression is not a valid property!");
				}
			}
		}

		public IEnumerator<T> GetEnumerator() => list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();

		public void Add(T item)
		{
			list.Add(item);

			foreach (var keyValuePair in propertyDictionaries)
			{
				keyValuePair.Value.Add(keyValuePair.Key(item), item);
			}
		}

		public IReadOnlyCollection<T> WhereIndexed<T2>(Expression<Func<T, T2>> property, T2 value)
		{
			if (property.Body is UnaryExpression unaryExpression)
			{
				var memberExpression = (MemberExpression)unaryExpression.Operand;
				var propertyInfo = (PropertyInfo) memberExpression.Member;
				if (!accessorMap.TryGetValue(propertyInfo.Name, out Func<T, object> lambda))
				{
					throw new ArgumentException("Property is not indexed!");
				}
				return propertyDictionaries[lambda][value];
			}
			else if (property.Body is MemberExpression memberExpression)
			{
				var propertyInfo = (PropertyInfo) memberExpression.Member;
				if (!accessorMap.TryGetValue(propertyInfo.Name, out Func<T, object> lambda))
	            {
	                throw new ArgumentException("Property is not indexed!");
	            }
				return propertyDictionaries[lambda][value];
			}
			else
			{
				throw new ArgumentException("Expression is not a valid property");
			}
		}

		public void Clear()
		{
			list.Clear();
			foreach (var keyValuePair in propertyDictionaries)
			{
				keyValuePair.Value.Clear();
			}
		}

		public bool Contains(T item) => list.Contains(item);

		public void CopyTo(T[] array, int arrayIndex) => list.CopyTo(array, arrayIndex);

		public bool Remove(T item)
		{
			foreach (var keyValuePair in propertyDictionaries)
			{
				keyValuePair.Value.Remove(keyValuePair.Key(item), item);
			}
			
			return list.Remove(item);
		}

		public int Count => list.Count;
		public bool IsReadOnly => false;

		public int IndexOf(T item) => list.IndexOf(item);

		public void Insert(int index, T item)
		{
			list.Insert(index, item);
			
			foreach (var keyValuePair in propertyDictionaries)
			{
				keyValuePair.Value.Add(keyValuePair.Key(item), item);
			}
		}

		public void RemoveAt(int index)
		{
			var item = list[index];
			list.RemoveAt(index);
			
			foreach (var keyValuePair in propertyDictionaries)
			{
				keyValuePair.Value.Remove(keyValuePair.Key(item), item);
			}
		}

		public T this[int index]
		{
			get => list[index];
			set {
				// Remove and insert to update property indices
				RemoveAt(index);
				Insert(index, value);
			}
		}
	}
}
