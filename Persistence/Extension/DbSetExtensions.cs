using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using Common.Requests;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Context;

namespace Persistence.Extension
{
    public static class DbSetExtensions
    {
        public static IQueryable<T> GetMany<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            Expression<Func<T, object>> @orderby = null,
            int page = 1,
            int pageSize = 10,
            bool isDescending = false,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            if (include != null)
            {
                query = include(query);
            }

            if (orderby != null)
            {
                query = isDescending ? query.OrderByDescending(@orderby) : query.OrderBy(@orderby);
            }

            return query.Pagination(page, pageSize);
        }

        public static string GetTableName<T>(this ApplicationDbContext context, DbSet<T> set)
            where T : class
        {
            var entityType = context.Model.FindEntityType(typeof(T));
            var schema = entityType.GetSchema();
            return entityType.GetTableName();
        }

        public static int ExecuteRawSql(this ApplicationDbContext context, string query)
        {
            using (var cmd = context.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = query;
                cmd.CommandType = System.Data.CommandType.Text;
                context.Database.OpenConnection();
                return cmd.ExecuteNonQuery();
            }
        }

        public static IList<T> ExecuteStoredProcedure<T>(
            this ApplicationDbContext context,
            string procedure,
            string[] paramNames,
            object[] paramValues
        )
            where T : class, IBase
        {
            if (paramValues == null)
            {
                throw new ArgumentNullException(nameof(paramValues));
            }

            if (paramNames == null)
            {
                throw new ArgumentNullException(nameof(paramNames));
            }

            if (paramValues.Length != paramNames.Length)
            {
                throw new ArgumentException(
                    "Length is not same",
                    nameof(paramValues) + "," + nameof(paramNames)
                );
            }

            using (var cmd = context.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = procedure;
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                for (int i = 0; i < paramValues.Length; i++)
                {
                    var parameter = cmd.CreateParameter();
                    parameter.ParameterName = paramNames[i];
                    parameter.Value = paramValues[i];
                    cmd.Parameters.Add(parameter);
                }

                context.Database.OpenConnection();
                using (var result = cmd.ExecuteReader())
                {
                    if (result.HasRows)
                    {
                        return MapToList<T>(result);
                    }
                }
            }

            return new List<T>();
        }

        public static IList<T> MapToList<T>(DbDataReader dr)
            where T : class, IBase
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties().ToList();

            var colMapping = dr.GetColumnSchema()
                .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
                .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    T obj = Activator.CreateInstance<T>();
                    foreach (var prop in props)
                    {
                        if (colMapping.Any(a => a.Key.ToLower() == prop.Name.ToLower()))
                        {
                            var colMap = colMapping[prop.Name.ToLower()];
                            if (colMap != null)
                            {
                                var val = dr.GetValue(colMap.ColumnOrdinal.Value);
                                prop.SetValue(obj, val == DBNull.Value ? null : val);
                            }
                        }
                    }

                    objList.Add(obj);
                }
            }

            return objList;
        }

        public static IQueryable<T> GetMany<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            string @orderby = "CreatedDate",
            int page = 1,
            int pageSize = 10,
            bool isDescending = false,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            if (include != null)
            {
                query = include(query);
            }

            query = query.OrderByCustom(@orderby, isDescending);
            return query.Pagination(page, pageSize);
        }

        public static IQueryable<T> GetMany<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            PageRequestModel requestModel
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            query = query.OrderByCustom(requestModel.OrderBy, requestModel.IsDescending);
            return query.Pagination(requestModel.Page, requestModel.PageSize);
        }

        public static IQueryable<T> GetManyReadOnly<T>(
            this DbSet<T> set,
            PageRequestModel requestModel
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(p => p.IsDeleted == false);
            query = query
                .OrderByCustom(requestModel.OrderBy, requestModel.IsDescending)
                .AsNoTracking();
            return query.Pagination(requestModel.Page, requestModel.PageSize);
        }

        public static IQueryable<T> GetManyReadOnly<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            PageRequestModel requestModel
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            query = query
                .OrderByCustom(requestModel.OrderBy, requestModel.IsDescending)
                .AsNoTracking();
            return query.Pagination(requestModel.Page, requestModel.PageSize);
        }

        public static IQueryable<T> GetManyReadOnly<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            string @orderby = "CreatedDate",
            int page = 1,
            int pageSize = 10,
            bool isDescending = false,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            if (include != null)
            {
                query = include(query);
            }

            query = query.OrderByCustom(@orderby, isDescending).AsNoTracking();
            return query.Pagination(page, pageSize);
        }

        public static IQueryable<T> GetAll<T>(this DbSet<T> set, Expression<Func<T, bool>> @where)
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            return query;
        }

        public static IQueryable<T> GetAllReadOnly<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false))
                .AsNoTracking();
            return query;
        }

        public static IQueryable<T> GetAllReadOnly<T>(this DbSet<T> set)
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(p => p.IsDeleted == false).AsNoTracking();
            return query;
        }

        public static TResult GetByWithSelect<T, TResult>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            Expression<Func<T, TResult>> select,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            if (include != null)
            {
                query = include(query);
            }
            return query.AsNoTracking().Select(select).FirstOrDefault();
        }

        // public static async Task<TResult> GetByWithSelectAsync<T, TResult>(
        //     this DbSet<T> set,
        //     Expression<Func<T, bool>> @where,
        //     Expression<Func<T, TResult>> select,
        //     Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
        //     CancellationToken cancellationToken = default(CancellationToken)
        // )
        //     where T : class, IBase
        // {
        //     IQueryable<T> query = set.Where(where);
        //     if (include != null)
        //     {
        //         query = include(query);
        //     }
        //     return await query.Select(select).FirstOrDefaultAsync(cancellationToken);
        // }

        public static async Task<TResult> GetByWithSelectAsync<T, TResult>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            Expression<Func<T, TResult>> select,
            CancellationToken cancellationToken = default(CancellationToken)
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where);
            return await query.Select(select).FirstOrDefaultAsync(cancellationToken);
        }

        public static async Task<TResult> GetByReadOnlyAsync<T, TResult>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            Expression<Func<T, TResult>> select,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            CancellationToken cancellationToken = default
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where).AsNoTracking();
            if (include != null)
            {
                query = include(query);
            }
            return await query.Select(select).FirstOrDefaultAsync(cancellationToken);
        }

        public static T GetBy<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where);
            if (include != null)
            {
                query = include(query);
            }

            return query.FirstOrDefault();
        }

        public static async Task<T> GetByIdAsync<T>(
            this DbSet<T> set,
            string id,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            CancellationToken cancellationToken = default
        )
            where T : Base
        {
            Expression<Func<T, bool>> where = p => p.Id == id;
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            if (include != null)
            {
                query = include(query);
            }
            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public static async Task<T> GetByAsync<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            CancellationToken cancellationToken = default
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where.AndAlso(p => p.IsDeleted == false));
            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public static T GetByReadOnly<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where).AsNoTracking();
            if (include != null)
            {
                query = include(query);
            }

            return query.FirstOrDefault();
        }

        public static async Task<T> GetByReadOnlyAsync<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null,
            CancellationToken cancellationToken = default
        )
            where T : class, IBase
        {
            IQueryable<T> query = set.Where(where).AsNoTracking();
            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }

        public static Func<T, object> GetSortExpression<T>(string sortExpressionStr)
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "x");
            Expression<Func<T, object>> sortExpression = Expression.Lambda<Func<T, object>>(
                Expression.Convert(Expression.Property(param, sortExpressionStr), typeof(object)),
                param
            );
            return sortExpression.Compile();
        }

        public static IOrderedQueryable<T> OrderByCustom<T>(
            this IQueryable<T> query,
            string sortExpression,
            bool isDescending
        )
        {
            if (string.IsNullOrWhiteSpace(sortExpression))
            {
                sortExpression = "CreatedDate";
            }
            if (isDescending)
            {
                return query.OrderByDescending((sortExpression));
            }

            return query.OrderBy((sortExpression));
        }

        public static IOrderedQueryable<T> OrderBy<T>(
            this IQueryable<T> source,
            string propertyName
        )
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        public static IOrderedQueryable<T> OrderByDescending<T>(
            this IQueryable<T> source,
            string propertyName
        )
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(T));
            string[] members = propertyName.Split('.');
            MemberExpression property = null;
            foreach (string member in members)
            {
                if (property == null)
                {
                    property = Expression.Property(parameter, member);
                }
                else
                {
                    property = Expression.Property(property, member);
                }
            }

            UnaryExpression propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }

        public static IQueryable<T> Pagination<T>(this IQueryable<T> query, int page, int pageSize)
            where T : class, IBase
        {
            if (page < 1)
                page = 1;
            if (pageSize == -1)
            {
                return query;
            }
            if (pageSize < 1)
                pageSize = 10;
            return query.Skip((page - 1) * pageSize).Take(pageSize);
        }

        public static int PageCount<T>(this IQueryable<T> query, int pageSize)
            where T : class, IBase
        {
            if (pageSize < 1)
                pageSize = 10;
            return (int)Math.Ceiling((float)query.Count() / pageSize);
        }

        public static async Task<int> ActiveCount<T>(this DbSet<T> set)
            where T : class, IBase
        {
            return await set.Where(p => p.IsDeleted == false).CountAsync();
        }

        public static async Task<int> ActiveCount<T>(
            this DbSet<T> set,
            CancellationToken cancellationToken
        )
            where T : class, IBase
        {
            return await set.Where(p => p.IsDeleted == false).CountAsync(cancellationToken);
        }

        public static async Task<int> ActiveCount<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where
        )
            where T : class, IBase
        {
            return await set.Where(where.AndAlso(p => p.IsDeleted == false)).CountAsync();
        }

        public static async Task<bool> ActiveAny<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            CancellationToken token = default
        )
            where T : class, IBase
        {
            return await set.AnyAsync(
                where.AndAlso(p => p.IsDeleted == false),
                cancellationToken: token
            );
        }

        public static async Task<int> ActiveCount<T>(
            this DbSet<T> set,
            Expression<Func<T, bool>> @where,
            CancellationToken cancellationToken
        )
            where T : class, IBase
        {
            return await set.Where(where.AndAlso(p => p.IsDeleted == false))
                .CountAsync(cancellationToken);
        }

        public static Expression<Func<T, bool>> AndAlso<T>(
            this Expression<Func<T, bool>> expr1,
            Expression<Func<T, bool>> expr2
        )
        {
            var parameter = Expression.Parameter(typeof(T));

            var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
            var left = leftVisitor.Visit(expr1.Body);

            var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
            var right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }

        public class ReplaceExpressionVisitor : ExpressionVisitor
        {
            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
            {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node)
            {
                if (node == _oldValue)
                    return _newValue;
                return base.Visit(node);
            }
        }
    }
}
