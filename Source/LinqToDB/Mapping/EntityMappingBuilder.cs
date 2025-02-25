﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace LinqToDB.Mapping
{
	using Expressions;
	using Extensions;
	using LinqToDB.Common;
	using Reflection;

	/// <summary>
	/// Fluent mapping entity builder.
	/// </summary>
	/// <typeparam name="TEntity">Entity mapping type.</typeparam>
	[PublicAPI]
	public class EntityMappingBuilder<TEntity>
	{
		#region Init

		/// <summary>
		/// Creates entity mapping builder.
		/// </summary>
		/// <param name="builder">Fluent mapping builder.</param>
		/// <param name="configuration">Optional mapping schema configuration name, for which this entity builder should be taken into account.
		/// <see cref="ProviderName"/> for standard configuration names.</param>
		public EntityMappingBuilder(FluentMappingBuilder builder, string? configuration)
		{
			_builder = builder ?? throw new ArgumentNullException(nameof(builder));

			Configuration = configuration;

			// We'll reset cache here, because there is no need to create builder if you don't want to change something
			_builder.MappingSchema.ResetEntityDescriptor(typeof(TEntity));
		}

		readonly FluentMappingBuilder _builder;

		/// <summary>
		/// Gets mapping schema configuration name, for which this entity builder should be taken into account.
		/// <see cref="ProviderName"/> for standard configuration names.
		/// </summary>
		public string? Configuration { get; }

		#endregion

		#region GetAttributes

		/// <summary>
		/// Returns attributes of specified type, applied to current entity type.
		/// </summary>
		/// <typeparam name="TA">Attribute type.</typeparam>
		/// <returns>Returns list of attributes, applied to current entity type.</returns>
		public TA[] GetAttributes<TA>()
			where TA : Attribute
		{
			return _builder.GetAttributes<TA>(typeof(TEntity));
		}

		/// <summary>
		/// Returns attributes of specified type, applied to specified entity type.
		/// </summary>
		/// <typeparam name="TA">Attribute type.</typeparam>
		/// <param name="type">Entity type.</param>
		/// <returns>Returns list of attributes, applied to specified entity type.</returns>
		public TA[] GetAttributes<TA>(Type type)
			where TA : Attribute
		{
			return _builder.GetAttributes<TA>(type);
		}

		/// <summary>
		/// Returns attributes of specified type, applied to specified entity member.
		/// Member could be inherited from parent classes.
		/// </summary>
		/// <typeparam name="TA">Attribute type.</typeparam>
		/// <param name="memberInfo">Member info object.</param>
		/// <returns>Returns list of attributes, applied to specified entity member.</returns>
		public TA[] GetAttributes<TA>(MemberInfo memberInfo)
			where TA : Attribute
		{
			return _builder.GetAttributes<TA>(typeof(TEntity), memberInfo);
		}

		/// <summary>
		/// Returns attributes of specified type, applied to current entity type and active for current configuration.
		/// </summary>
		/// <typeparam name="TA">Attribute type.</typeparam>
		/// <param name="configGetter">Function to extract configuration name from attribute instance.</param>
		/// <returns>Returns list of attributes.</returns>
		public TA[] GetAttributes<TA>(Func<TA,string?> configGetter)
			where TA : Attribute
		{
			var attrs = GetAttributes<TA>();

			return string.IsNullOrEmpty(Configuration) ?
				attrs.Where(a => string.IsNullOrEmpty(configGetter(a))).ToArray() :
				attrs.Where(a => Configuration ==    configGetter(a)). ToArray();
		}

		/// <summary>
		/// Returns attributes of specified type, applied to specified entity type and active for current configuration.
		/// </summary>
		/// <typeparam name="TA">Attribute type.</typeparam>
		/// <param name="type">Entity type.</param>
		/// <param name="configGetter">Function to extract configuration name from attribute instance.</param>
		/// <returns>Returns list of attributes.</returns>
		public TA[] GetAttributes<TA>(Type type, Func<TA,string?> configGetter)
			where TA : Attribute
		{
			var attrs = GetAttributes<TA>(type);

			return string.IsNullOrEmpty(Configuration) ?
				attrs.Where(a => string.IsNullOrEmpty(configGetter(a))).ToArray() :
				attrs.Where(a => Configuration ==    configGetter(a)). ToArray();
		}

		/// <summary>
		/// Returns attributes of specified type, applied to specified entity member and active for current configuration.
		/// </summary>
		/// <typeparam name="TA">Attribute type.</typeparam>
		/// <param name="memberInfo">Member info object.</param>
		/// <param name="configGetter">Function to extract configuration name from attribute instance.</param>
		/// <returns>Returns list of attributes.</returns>
		public TA[] GetAttributes<TA>(MemberInfo memberInfo, Func<TA,string?> configGetter)
			where TA : Attribute
		{
			var attrs = GetAttributes<TA>(memberInfo);

			return string.IsNullOrEmpty(Configuration) ?
				attrs.Where(a => string.IsNullOrEmpty(configGetter(a))).ToArray() :
				attrs.Where(a => Configuration ==    configGetter(a)). ToArray();
		}

		#endregion

		#region HasAttribute

		/// <summary>
		/// Adds mapping attribute to current entity.
		/// </summary>
		/// <param name="attribute">Mapping attribute to add.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasAttribute(Attribute attribute)
		{
			_builder.HasAttribute(typeof(TEntity), attribute);
			return this;
		}

		/// <summary>
		/// Adds mapping attribute to specified member.
		/// </summary>
		/// <param name="memberInfo">Target member.</param>
		/// <param name="attribute">Mapping attribute to add to specified member.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasAttribute(MemberInfo memberInfo, Attribute attribute)
		{
			_builder.HasAttribute(memberInfo, attribute);
			return this;
		}

		/// <summary>
		/// Adds mapping attribute to a member, specified using lambda expression.
		/// </summary>
		/// <param name="func">Target member, specified using lambda expression.</param>
		/// <param name="attribute">Mapping attribute to add to specified member.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasAttribute(LambdaExpression func, Attribute attribute)
		{
			_builder.HasAttribute(func, attribute);
			return this;
		}

		/// <summary>
		/// Adds mapping attribute to a member, specified using lambda expression.
		/// </summary>
		/// <param name="func">Target member, specified using lambda expression.</param>
		/// <param name="attribute">Mapping attribute to add to specified member.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasAttribute(Expression<Func<TEntity,object?>> func, Attribute attribute)
		{
			_builder.HasAttribute(func, attribute);
			return this;
		}

		#endregion

		/// <summary>
		/// Creates entity builder for specified mapping type.
		/// </summary>
		/// <typeparam name="TE">Mapping type.</typeparam>
		/// <param name="configuration">Optional mapping schema configuration name, for which this entity builder should be taken into account.
		/// <see cref="ProviderName"/> for standard configuration names.</param>
		/// <returns>Returns new fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TE> Entity<TE>(string? configuration = null)
		{
			return _builder.Entity<TE>(configuration);
		}

		/// <summary>
		/// Adds column mapping to current entity.
		/// </summary>
		/// <param name="func">Column mapping property or field getter expression.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public PropertyMappingBuilder<TEntity, TProperty> Property<TProperty>(Expression<Func<TEntity,TProperty>> func)
		{
			return new PropertyMappingBuilder<TEntity, TProperty>(this, func).IsColumn();
		}

		/// <summary>
		/// Adds member mapping to current entity.
		/// </summary>
		/// <param name="func">Column mapping property or field getter expression.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public PropertyMappingBuilder<TEntity, TProperty> Member<TProperty>(Expression<Func<TEntity,TProperty>> func)
		{
			return new PropertyMappingBuilder<TEntity, TProperty>(this, func);
		}

		/// <summary>
		/// Adds association mapping to current entity.
		/// </summary>
		/// <typeparam name="TProperty">Association member type.</typeparam>
		/// <typeparam name="TThisKey">This association side key type.</typeparam>
		/// <typeparam name="TOtherKey">Other association side key type.</typeparam>
		/// <param name="prop">Association member getter expression.</param>
		/// <param name="thisKey">This association key getter expression.</param>
		/// <param name="otherKey">Other association key getter expression.</param>
		/// <param name="canBeNull">Defines type of join. True - left join, False - inner join.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public PropertyMappingBuilder<TEntity, TProperty> Association<TProperty, TThisKey, TOtherKey>(
			Expression<Func<TEntity, TProperty>>   prop,
			Expression<Func<TEntity, TThisKey>>    thisKey,
			Expression<Func<TProperty, TOtherKey>> otherKey,
			bool                                   canBeNull = true)
		{
			if (prop     == null) throw new ArgumentNullException(nameof(prop));
			if (thisKey  == null) throw new ArgumentNullException(nameof(thisKey));
			if (otherKey == null) throw new ArgumentNullException(nameof(otherKey));

			var thisKeyName  = MemberHelper.GetMemberInfo(thisKey).Name;
			var otherKeyName = MemberHelper.GetMemberInfo(otherKey).Name;

			return Property( prop ).HasAttribute( new AssociationAttribute { ThisKey = thisKeyName, OtherKey = otherKeyName, CanBeNull = canBeNull } );
		}

		/// <summary>
		/// Adds association mapping to current entity.
		/// </summary>
		/// <typeparam name="TPropElement">Association member type.</typeparam>
		/// <typeparam name="TThisKey">This association side key type.</typeparam>
		/// <typeparam name="TOtherKey">Other association side key type.</typeparam>
		/// <param name="prop">Association member getter expression.</param>
		/// <param name="thisKey">This association key getter expression.</param>
		/// <param name="otherKey">Other association key getter expression.</param>
		/// <param name="canBeNull">Defines type of join. True - left join, False - inner join.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public PropertyMappingBuilder<TEntity, IEnumerable<TPropElement>> Association<TPropElement, TThisKey, TOtherKey>(
			Expression<Func<TEntity, IEnumerable<TPropElement>>> prop,
			Expression<Func<TEntity, TThisKey>>                  thisKey,
			Expression<Func<TPropElement, TOtherKey>>            otherKey,
			bool                                                 canBeNull = true)
		{
			if (prop     == null) throw new ArgumentNullException(nameof(prop));
			if (thisKey  == null) throw new ArgumentNullException(nameof(thisKey));
			if (otherKey == null) throw new ArgumentNullException(nameof(otherKey));

			var thisKeyName  = MemberHelper.GetMemberInfo(thisKey).Name;
			var otherKeyName = MemberHelper.GetMemberInfo(otherKey).Name;

			return Property( prop ).HasAttribute( new AssociationAttribute { ThisKey = thisKeyName, OtherKey = otherKeyName, CanBeNull = canBeNull } );
		}

		/// <summary>
		/// Adds one-to-many association mapping to current entity.
		/// </summary>
		/// <typeparam name="TOther">Other association side type</typeparam>
		/// <param name="prop">Association member getter expression.</param>
		/// <param name="predicate">Predicate expression.</param>
		/// <param name="canBeNull">Defines type of join. True - left join, False - inner join.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public PropertyMappingBuilder<TEntity, IEnumerable<TOther>> Association<TOther>(
			Expression<Func<TEntity, IEnumerable<TOther>>> prop,
			Expression<Func<TEntity, TOther, bool>>        predicate,
			bool                                           canBeNull = true)
		{
			if (prop      == null) throw new ArgumentNullException(nameof(prop));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			return Property( prop ).HasAttribute( new AssociationAttribute { Predicate = predicate, CanBeNull = canBeNull, Relationship = Relationship.OneToMany } );
		}

		/// <summary>
		/// Adds one-to-one association mapping to current entity.
		/// </summary>
		/// <typeparam name="TOther">Other association side type</typeparam>
		/// <param name="prop">Association member getter expression.</param>
		/// <param name="predicate">Predicate expression</param>
		/// <param name="canBeNull">Defines type of join. True - left join, False - inner join.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public PropertyMappingBuilder<TEntity, TOther> Association<TOther>(
			Expression<Func<TEntity, TOther>>       prop,
			Expression<Func<TEntity, TOther, bool>> predicate,
			bool                                    canBeNull = true)
		{
			if (prop      == null) throw new ArgumentNullException(nameof(prop));
			if (predicate == null) throw new ArgumentNullException(nameof(predicate));

			return Property( prop ).HasAttribute( new AssociationAttribute { Predicate = predicate, CanBeNull = canBeNull } );
		}

		/// <summary>
		/// Adds one-to-many association mapping to current entity.
		/// </summary>
		/// <typeparam name="TOther">Other association side type</typeparam>
		/// <param name="prop">Association member getter expression.</param>
		/// <param name="queryExpression">Query expression.</param>
		/// <param name="canBeNull">Defines type of join. True - left join, False - inner join.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public PropertyMappingBuilder<TEntity, IEnumerable<TOther>> Association<TOther>(
			Expression<Func<TEntity, IEnumerable<TOther>>>              prop,
			Expression<Func<TEntity, IDataContext, IQueryable<TOther>>> queryExpression,
			bool                                                        canBeNull = true)
		{
			if (prop            == null) throw new ArgumentNullException(nameof(prop));
			if (queryExpression == null) throw new ArgumentNullException(nameof(queryExpression));

			return Property( prop ).HasAttribute( new AssociationAttribute { QueryExpression = queryExpression, CanBeNull = canBeNull, Relationship = Relationship.OneToMany } );
		}

		/// <summary>
		/// Adds one-to-one association mapping to current entity.
		/// </summary>
		/// <typeparam name="TOther">Other association side type</typeparam>
		/// <param name="prop">Association member getter expression.</param>
		/// <param name="queryExpression">Query expression.</param>
		/// <param name="canBeNull">Defines type of join. True - left join, False - inner join.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public PropertyMappingBuilder<TEntity, TOther> Association<TOther>(
			Expression<Func<TEntity, TOther>>                           prop,
			Expression<Func<TEntity, IDataContext, IQueryable<TOther>>> queryExpression,
			bool                                                        canBeNull = true)
		{
			if (prop            == null) throw new ArgumentNullException(nameof(prop));
			if (queryExpression == null) throw new ArgumentNullException(nameof(queryExpression));

			return Property( prop ).HasAttribute( new AssociationAttribute { QueryExpression = queryExpression, CanBeNull = canBeNull } );
		}

		/// <summary>
		/// Adds primary key mapping to current entity.
		/// </summary>
		/// <param name="func">Primary key getter expression.</param>
		/// <param name="order">Primary key field order.
		/// When multiple fields specified by getter expression, fields will be ordered from first menthioned
		/// field to last one starting from provided order with step <c>1</c>.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasPrimaryKey<TProperty>(Expression<Func<TEntity,TProperty>> func, int order = -1)
		{
			var n = 0;

			return SetAttribute(
				func,
				true,
				m => new PrimaryKeyAttribute(Configuration, order + n++ + (m && order == -1 ? 1 : 0)),
				(m,a) => a.Order = order + n++ + (m && order == -1 ? 1 : 0),
				a => a.Configuration);
		}

		/// <summary>
		/// Adds identity column mappping to current entity.
		/// </summary>
		/// <param name="func">Identity field getter expression.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasIdentity<TProperty>(Expression<Func<TEntity,TProperty>> func)
		{
			return SetAttribute(
				func,
				false,
				 _    => new IdentityAttribute(Configuration),
				(_,a) => {},
				a => a.Configuration);
		}

		/// <summary>
		/// Adds column mapping to current entity.
		/// </summary>
		/// <param name="func">Column member getter expression.</param>
		/// <param name="order">Unused.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasColumn(Expression<Func<TEntity,object?>> func, int order = -1)
		{
			return SetAttribute(
				func,
				true,
				 _    => new ColumnAttribute() { Configuration = Configuration, Order = order },
				(_,a) => a.IsColumn = true,
				a => a.Configuration);
		}

		/// <summary>
		/// Instruct LINQ to DB to not incude specified member into mapping.
		/// </summary>
		/// <param name="func">Member getter expression.</param>
		/// <param name="order">Unused.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> Ignore(Expression<Func<TEntity,object?>> func, int order = -1)
		{
			return SetAttribute(
				func,
				true,
				 _    => new NotColumnAttribute { Configuration = Configuration, Order = order },
				(_,a) => a.IsColumn = false,
				a => a.Configuration);
		}

		/// <summary>
		/// Adds option for skipping values for column on current entity during insert.
		/// </summary>
		/// <param name="func">Column member getter expression.</param>
		/// <param name="values">Values that should be skipped during insert.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasSkipValuesOnInsert(Expression<Func<TEntity, object?>> func, params object?[] values)
		{
			return SetAttribute(
				func,
				true,
				_ => new SkipValuesOnInsertAttribute(values) { Configuration = Configuration },
				(_, a) => { },
				a => a.Configuration);
		}

		/// <summary>
		/// Adds option for skipping values for column on current entity during update.
		/// </summary>
		/// <param name="func">Column member getter expression.</param>
		/// <param name="values">Values that should be skipped during update.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasSkipValuesOnUpdate(Expression<Func<TEntity, object?>> func, params object?[] values)
		{
			return SetAttribute(
				func,
				true,
				_ => new SkipValuesOnUpdateAttribute(values) { Configuration = Configuration },
				(_, a) => { },
				a => a.Configuration);
		}

		/// <summary>
		/// Sets database table name for current entity.
		/// </summary>
		/// <param name="tableName">Table name.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasTableName(string tableName)
		{
			return SetTable(a => a.Name = tableName);
		}

		/// <summary>
		/// Sets if it is required to use <see cref="PropertyMappingBuilder{TEntity, TProperty}.IsColumn"/> to treat property or field as column
		/// </summary>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> IsColumnRequired()
		{
			return SetTable(a => a.IsColumnAttributeRequired = true);
		}

		/// <summary>
		/// Sets if it is not required to use <see cref="PropertyMappingBuilder{TEntity, TProperty}.IsColumn"/> - all public fields and properties are treated as columns
		/// This is the default behaviour
		/// </summary>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> IsColumnNotRequired()
		{
			return SetTable(a => a.IsColumnAttributeRequired = false);
		}

		/// <summary>
		/// Sets database schema/owner name for current entity, to override default name.
		/// See <see cref="LinqExtensions.SchemaName{T}(ITable{T}, string)"/> method for support information per provider.
		/// </summary>
		/// <param name="schemaName">Schema/owner name.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasSchemaName(string schemaName)
		{
			return SetTable(a => a.Schema = schemaName);
		}

		/// <summary>
		/// Sets database name, to override default database name.
		/// See <see cref="LinqExtensions.DatabaseName{T}(ITable{T}, string)"/> method for support information per provider.
		/// </summary>
		/// <param name="databaseName">Database name.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasDatabaseName(string databaseName)
		{
			return SetTable(a => a.Database = databaseName);
		}

		/// <summary>
		/// Sets linked server name.
		/// See <see cref="LinqExtensions.ServerName{T}(ITable{T}, string)"/> method for support information per provider.
		/// </summary>
		/// <param name="serverName">Linked server name.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasServerName(string serverName)
		{
			return SetTable(a => a.Server = serverName);
		}

		/// <summary>
		/// Sets linked server name.
		/// See <see cref="TableExtensions.IsTemporary{T}(ITable{T},bool)"/> method for support information per provider.
		/// </summary>
		/// <param name="isTemporary">Linked server name.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasIsTemporary(bool isTemporary = true)
		{
			return SetTable(a => a.IsTemporary = isTemporary);
		}

		/// <summary>
		/// Sets Table options.
		/// See <see cref="TableExtensions.TableOptions{T}(ITable{T},TableOptions)"/> method for support information per provider.
		/// </summary>
		/// <param name="tableOptions">Table options.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> HasTableOptions(TableOptions tableOptions)
		{
			return SetTable(a =>
			{
				if ((tableOptions & TableOptions.None) != 0)
					a.TableOptions = tableOptions;
				else
					a.TableOptions |= tableOptions;
			});
		}

		/// <summary>
		/// Adds inheritance mapping for specified discriminator value.
		/// </summary>
		/// <typeparam name="TS">Discriminator value type.</typeparam>
		/// <param name="key">Discriminator member getter expression.</param>
		/// <param name="value">Discriminator value.</param>
		/// <param name="type">Mapping type, used with specified discriminator value.</param>
		/// <param name="isDefault">If <c>true</c>, current mapping type used by default.</param>
		/// <returns>Returns current fluent entity mapping builder.</returns>
		public EntityMappingBuilder<TEntity> Inheritance<TS>(Expression<Func<TEntity, TS>> key, TS value, Type type, bool isDefault = false)
		{
			HasAttribute(new InheritanceMappingAttribute {Code = value, Type = type, IsDefault = isDefault});
			var objProp = Expression.Lambda<Func<TEntity, object?>>(Expression.Convert(key.Body, typeof(object)), key.Parameters);
			Property(objProp).IsDiscriminator();

			return this;
		}

		/// <summary>
		///     Specifies a LINQ <see cref="IQueryable{T}" /> function that will automatically be applied to any queries targeting
		///     this entity type.
		/// </summary>
		/// <param name="filterFunc">The LINQ predicate expression. </param>
		/// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
		public EntityMappingBuilder<TEntity> HasQueryFilter(Func<IQueryable<TEntity>, IDataContext, IQueryable<TEntity>> filterFunc)
		{
			return HasQueryFilter<IDataContext>(filterFunc);
		}

		/// <summary>
		///     Specifies a LINQ <see cref="IQueryable{T}" /> function that will automatically be applied to any queries targeting
		///     this entity type.
		/// </summary>
		/// <param name="filterFunc"> The LINQ predicate expression. </param>
		/// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
		public EntityMappingBuilder<TEntity> HasQueryFilter<TDataContext>(Func<IQueryable<TEntity>, TDataContext, IQueryable<TEntity>> filterFunc)
			where TDataContext : IDataContext
		{
			HasAttribute(new QueryFilterAttribute { FilterFunc = filterFunc });
			return this;
		}

		/// <summary>
		///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
		///     this entity type.
		/// </summary>
		/// <param name="filter"> The LINQ predicate expression. </param>
		/// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
		public EntityMappingBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, IDataContext, bool>> filter)
		{
			return HasQueryFilter<IDataContext>(filter);
		}

		/// <summary>
		///     Specifies a LINQ predicate expression that will automatically be applied to any queries targeting
		///     this entity type.
		/// </summary>
		/// <param name="filter"> The LINQ predicate expression. </param>
		/// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
		public EntityMappingBuilder<TEntity> HasQueryFilter<TDataContext>(Expression<Func<TEntity, TDataContext, bool>> filter)
			where TDataContext : IDataContext
		{
			var queryParam   = Expression.Parameter(typeof(IQueryable<TEntity>), "q");
			var dcParam      = Expression.Parameter(typeof(TDataContext), "dc");
			var replaceParam = filter.Parameters[1];
			var filterBody   = filter.Body.Replace(replaceParam, dcParam);
			var filterLambda = Expression.Lambda(filterBody, filter.Parameters[0]);
			var body         = Expression.Call(Methods.Queryable.Where.MakeGenericMethod(typeof(TEntity)), queryParam, filterLambda);
			var lambda       = Expression.Lambda<Func<IQueryable<TEntity>, TDataContext, IQueryable<TEntity>>>(body, queryParam, dcParam);

			return HasQueryFilter(lambda.CompileExpression());
		}

		#region Dynamic Properties
		/// <summary>
		/// Adds dynamic columns store dictionary member mapping to current entity.
		/// </summary>
		/// <param name="func">Column mapping property or field getter expression.</param>
		/// <returns>Returns fluent property mapping builder.</returns>
		public EntityMappingBuilder<TEntity> DynamicColumnsStore(Expression<Func<TEntity, object?>> func)
		{
			Member(func)
				.HasAttribute(new DynamicColumnsStoreAttribute() { Configuration = Configuration });

			return this;
		}

		public EntityMappingBuilder<TEntity> DynamicPropertyAccessors(
			Expression<Func<TEntity, string, object, object>> getter,
			Expression<Action<TEntity, string, object>>       setter)
		{
			_builder.HasAttribute(
				typeof(TEntity),
				new DynamicColumnAccessorAttribute()
				{
					GetterExpression = getter,
					SetterExpression = setter,
					Configuration = Configuration
				});

			return this;
		}

		#endregion

		EntityMappingBuilder<TEntity> SetTable(Action<TableAttribute> setColumn)
		{
			return SetAttribute(
				() =>
				{
					var a = new TableAttribute { Configuration = Configuration, IsColumnAttributeRequired = false };
					setColumn(a);
					return a;
				},
				setColumn,
				a => a.Configuration,
				a => new TableAttribute
				{
					Configuration             = a.Configuration,
					Name                      = a.Name,
					Schema                    = a.Schema,
					Database                  = a.Database,
					Server                    = a.Server,
					TableOptions              = a.TableOptions,
					IsColumnAttributeRequired = a.IsColumnAttributeRequired,
				});
		}

		EntityMappingBuilder<TEntity> SetAttribute<TA>(
			Func<TA>         getNew,
			Action<TA>       modifyExisting,
			Func<TA,string?> configGetter,
			Func<TA,TA>      overrideAttribute)
			where TA : Attribute
		{
			var attrs = GetAttributes(typeof(TEntity), configGetter);

			if (attrs.Length == 0)
			{
				var attr = _builder.MappingSchema.GetAttribute(typeof(TEntity), configGetter);

				if (attr != null)
				{
					var na = overrideAttribute(attr);

					modifyExisting(na);
					_builder.HasAttribute(typeof(TEntity), na);

					return this;
				}

				_builder.HasAttribute(typeof(TEntity), getNew());
			}
			else
				modifyExisting(attrs[0]);

			return this;
		}

		internal EntityMappingBuilder<TEntity> SetAttribute<TA>(
			Func<TA>                   getNew,
			Action<TA>                 modifyExisting,
			Func<TA, string?>          configGetter,
			Func<IEnumerable<TA>, TA?> existingGetter)
			where TA : Attribute
		{
			var attr = existingGetter(GetAttributes(typeof(TEntity), configGetter));

			if (attr == null)
			{
				_builder.HasAttribute(typeof(TEntity), getNew());
			}
			else
			{
				modifyExisting(attr);
			}

			return this;
		}

		internal EntityMappingBuilder<TEntity> SetAttribute<TProperty, TA>(
			Expression<Func<TEntity,TProperty>> func,
			bool                                processNewExpression,
			Func<bool,TA>                       getNew,
			Action<bool,TA>                     modifyExisting,
			Func<TA,string?>                    configGetter,
			Func<TA,TA>?                        overrideAttribute = null,
			Func<IEnumerable<TA>, TA?>?         existingGetter    = null
			)
			where TA : Attribute
		{
			var ex = func.Body;

			if (ex is UnaryExpression expression)
				ex = expression.Operand;

			if (existingGetter == null)
				existingGetter = GetExisting;

			void SetAttr(Expression e, bool m)
			{
				var memberInfo = MemberHelper.GetMemberInfo(e);

				if (e is MemberExpression && memberInfo.ReflectedType != typeof(TEntity)) memberInfo = typeof(TEntity).GetMemberEx(memberInfo)!;

				if (memberInfo == null) throw new ArgumentException($"'{e}' cant be converted to a class member.");

				var attr = existingGetter!(GetAttributes(memberInfo, configGetter));

				if (attr == null)
				{
					if (overrideAttribute != null)
					{
						attr = existingGetter(_builder.MappingSchema.GetAttributes(typeof(TEntity), memberInfo, configGetter));

						if (attr != null)
						{
							var na = overrideAttribute(attr);

							modifyExisting(m, na);
							_builder.HasAttribute(memberInfo, na);

							return;
						}
					}

					_builder.HasAttribute(memberInfo, getNew(m));
				}
				else
					modifyExisting(m, attr);
			}

			if (processNewExpression && ex.NodeType == ExpressionType.New)
			{
				var nex = (NewExpression)ex;

				if (nex.Arguments.Count > 0)
				{
					foreach (var arg in nex.Arguments)
						SetAttr(arg, true);
					return this;
				}
			}

			SetAttr(ex, false);

			return this;
		}

		private TA? GetExisting<TA>(IEnumerable<TA> attrs)
			where TA : Attribute
		{
			return attrs.FirstOrDefault();
		}
	}
}
