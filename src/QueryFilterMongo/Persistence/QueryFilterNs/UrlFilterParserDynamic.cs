using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Driver;

namespace QueryFilterMongo.Persistence.QueryFilterNs
{
    public class UrlFilterParserDynamic<TEntity>
    {
        private const string Limit = "limit";
        private const string Offset = "offset";
        private const string Sort = "sort";
        private const string Desc = "desc";
        
        private readonly Regex _filterParamRegex = new("^((?<cond>eq|neq|cont|ends|sts|gt|gte|lt|lte|in|nin|ncont|nends|nsts|eqci|neqci)\\:)?(?<value>.+)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
        
        private readonly List<string> _timePeriods = new List<string>(new string[] { "today", "yesterday", "lastSevenDays", "lastThirtyDays", "prevMonth", "prevWeek" });

        private readonly List<string> _ignoredParams = new(new[] { Limit, Offset, Sort });

        private readonly Dictionary<string, Type> _knownProperties = new(StringComparer.InvariantCultureIgnoreCase);

        public UrlFilterParserDynamic(Dictionary<string, Type> knownFields = null)
        {
            if (knownFields is not { Count: > 0 })
            {
                return;
            }

            foreach (var (key, value) in knownFields.Where(pair => !_knownProperties.ContainsKey(pair.Key)))
            {
                _knownProperties.Add(key, value);
            }
        }

        private enum Operator
        {
            Eq,
            Neq,
            Cont,
            Ends,
            Sts,
            Gt,
            Gte,
            Lt,
            Lte,
            In,
            Nin,
            NCont,
            NEnds,
            NSts,
            EqCi,
            NeqCi
        }

        public UrlFilterParserDynamic<TEntity> IgnoreParams(params string[] list)
        {
            foreach (var param in list)
            {
                if (!string.IsNullOrWhiteSpace(param))
                {
                    _ignoredParams.Add(param.ToLower());
                }
            }

            return this;
        }

        public DynamicQueryParams<TEntity> Parse(IDictionary<string, StringValues> queryString, FilterDefinition<TEntity> defaultFilter = null)
        {
            var result = new DynamicQueryParams<TEntity>();

            result.Filter = ParseFilter(result.UnknownParams, queryString);

            result.FindOptions = new FindOptions<TEntity>() { Collation = new Collation("cs", strength: CollationStrength.Secondary) };
            ParseSorting(result.FindOptions, queryString);
            ParsePaging(result, queryString);

            return result;
        }

        private void ParsePaging(DynamicQueryParams<TEntity> result, IDictionary<string, StringValues> queryString)
        {
            if (queryString is not { Count: > 0 })
            {
                return;
            }

            foreach (var key in queryString.Keys)
            {
                if (key.Equals(Limit, StringComparison.InvariantCultureIgnoreCase) && queryString[key].Count > 0 && int.TryParse(queryString[key][0], out var limit))
                {
                    result.FindOptions.Limit = limit;
                    result.Limit = limit;
                }

                if (!key.Equals(Offset, StringComparison.InvariantCultureIgnoreCase) || queryString[key].Count <= 0 || !int.TryParse(queryString[key][0], out var offset))
                {
                    continue;
                }

                result.FindOptions.Skip = offset;
                result.Offset = offset;
            }
        }

        private void ParseSorting(FindOptions<TEntity> options, IDictionary<string, StringValues> queryString)
        {
            if (queryString is not { Count: > 0 })
            {
                return;
            }

            SortDefinition<TEntity> sort = null;

            foreach (var key in queryString.Keys)
            {
                if (!key.Equals(Sort, StringComparison.InvariantCultureIgnoreCase) || queryString[key].Count <= 0)
                {
                    continue;
                }

                var fields = queryString[key][0].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var field in fields.Where(p => !string.IsNullOrWhiteSpace(p)))
                {
                    var tokens = field.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length <= 0)
                    {
                        continue;
                    }

                    var fieldName = tokens[0].Trim();
                    var propName = GetPropertyName(fieldName);

                    if (propName != null)
                    {
                        SortDefinition<TEntity> oneSort;
                        if (tokens.Length > 1 && Desc.Equals(tokens[1],
                            StringComparison.InvariantCultureIgnoreCase))
                        {
                            oneSort = Builders<TEntity>.Sort.Descending(propName);
                        }
                        else
                        {
                            oneSort = Builders<TEntity>.Sort.Ascending(propName);
                        }

                        sort = sort == null ? oneSort : Builders<TEntity>.Sort.Combine(sort, oneSort);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown field for sort: {fieldName}.");
                    }
                }
            }

            if (sort != null)
            {
                options.Sort = sort;
            }
        }

        private FilterDefinition<TEntity> ParseFilter(IDictionary<string, StringValues> unknownParams, IDictionary<string, StringValues> queryString)
        {
            var filters = new List<FilterDefinition<TEntity>>();

            if (queryString is not { Count: > 0 })
            {
                return filters.Count > 0 ? Builders<TEntity>.Filter.And(filters) : Builders<TEntity>.Filter.Empty;
            }

            foreach (var pair in queryString)
            {
                if (_ignoredParams.Contains(pair.Key.ToLower()))
                {
                    continue;
                }

                var propType = GetPropertyType(pair.Key);
                var propName = GetPropertyName(pair.Key);

                if (propType != null)
                {
                    filters.AddRange(pair.Value.Select(value => ParseFilterParam(new PropInfo
                        {
                            Name = propName,
                            PropType = propType
                        }, value))
                        .Where(filter => filter != null));
                }
                else
                {
                    unknownParams.Add(pair);
                }
            }

            return filters.Count > 0 ? Builders<TEntity>.Filter.And(filters) : Builders<TEntity>.Filter.Empty;
        }

        private Type GetPropertyType(string propertyName)
        {
            if (_knownProperties.ContainsKey(propertyName))
            {
                return _knownProperties[propertyName];
            }

            var properties = propertyName.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var type = typeof(TEntity);

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var last = i == (properties.Length - 1);

                var propInfo = type.GetProperty(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (propInfo != null)
                {
                    if (last)
                    {
                        return propInfo.PropertyType;
                    }

                    type = propInfo.PropertyType;
                }
                else
                {
                    var fieldInfo = type.GetField(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (fieldInfo == null)
                    {
                        continue;
                    }

                    if (last)
                    {
                        return fieldInfo.FieldType;
                    }

                    type = fieldInfo.FieldType;
                }
            }

            return null;
        }

        private string GetPropertyName(string propertyName)
        {
            if (_knownProperties.ContainsKey(propertyName))
            {
                return _knownProperties.FirstOrDefault(p => p.Key.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)).Key;
            }

            var propertyTokens = new List<string>();

            var properties = propertyName.Split(new[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var type = typeof(TEntity);

            for (var i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                var last = i == (properties.Length - 1);

                var propInfo = type.GetProperty(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (propInfo != null)
                {
                    if (last)
                    {
                        propertyTokens.Add(propInfo.Name);
                        break;
                    }

                    propertyTokens.Add(propInfo.Name);
                    type = propInfo.PropertyType;
                }
                else
                {
                    var fieldInfo = type.GetField(property, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (fieldInfo == null)
                    {
                        continue;
                    }

                    if (last)
                    {
                        propertyTokens.Add(fieldInfo.Name);
                        break;
                    }

                    propertyTokens.Add(fieldInfo.Name);
                    type = fieldInfo.FieldType;
                }
            }

            return propertyTokens.Count <= 0 
                ? null 
                : propertyTokens.Aggregate("", (current, token) => current + ((current.Length > 0 ? "." : "") + token));
        }
        
        private FilterDefinition<TEntity> ParseFilterParam(PropInfo propInfo, string value)
        {
            var fieldName = propInfo.Name;

            var m = _filterParamRegex.Match(value);
            if (!m.Success)
            {
                return AddCondition(fieldName, propInfo.PropType, Operator.Eq, PrepareValue(propInfo.PropType, value));
            }

            var oper = FindOperator(fieldName, m.Groups["cond"]);
            return AddCondition(fieldName, propInfo.PropType, oper, PrepareValue(propInfo.PropType, m.Groups["value"].Value));

        }

        private FilterDefinition<TEntity> AddCondition(string fieldName, Type fieldType, Operator oper, object[] values)
        {
            var filters = new List<FilterDefinition<TEntity>>();
            FilterDefinition<TEntity> filter = null;

            if (oper is Operator.In or Operator.Nin)
            {
                switch (oper)
                {
                    case Operator.In:
                        filter = Builders<TEntity>.Filter.In(fieldName, values);
                        break;
                    case Operator.Nin:
                        filter = Builders<TEntity>.Filter.Nin(fieldName, values);
                        break;
                }

                if (filter != null)
                {
                    filters.Add(filter);
                }
            }
            else
            {
                foreach (var value in values)
                {
                    if (oper.Equals(Operator.Eq) && (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?)) &&
                        value is string && _timePeriods.Contains(value as string, StringComparer.OrdinalIgnoreCase))
                    {
                        // Casove obdobi
                        var dnesniPulnoc = DateTime.Now.Date;
                        DateTime? casOd = null;
                        DateTime? casDo = null;
                        switch ((value as string).ToLower())
                        {
                            // today, yesterday, lastSevenDays, lastThirtyDays, prevMonth a prevWeek
                            case "today":
                                casOd = dnesniPulnoc;
                                casDo = dnesniPulnoc.AddDays(1);
                                break;
                            case "yesterday":
                                var vcerejsiPulnoc = DateTime.Now.Date.Subtract(TimeSpan.FromDays(1));
                                casOd = vcerejsiPulnoc;
                                casDo = vcerejsiPulnoc.AddDays(1);
                                break;
                            case "lastsevendays":
                                var predSestiDny = dnesniPulnoc.Subtract(TimeSpan.FromDays(6));
                                casOd = predSestiDny;
                                casDo = dnesniPulnoc.AddDays(1);
                                break;
                            case "lastthirtydays":
                                var pred29Dny = dnesniPulnoc.Subtract(TimeSpan.FromDays(29));
                                casOd = pred29Dny;
                                casDo = dnesniPulnoc.AddDays(1);
                                break;
                            case "prevmonth":
                                var zacatekMinulehoMesice =
                                    new DateTime(dnesniPulnoc.Year, dnesniPulnoc.Month, 1).AddMonths(-1);
                                casOd = zacatekMinulehoMesice;
                                casDo = zacatekMinulehoMesice.AddMonths(1);
                                break;
                            case "prevweek":
                                var zacatekMinulehoTydne = dnesniPulnoc.AddDays(-(int)dnesniPulnoc.DayOfWeek - 6);
                                casOd = zacatekMinulehoTydne;
                                casDo = zacatekMinulehoTydne.AddDays(7);
                                break;
                        }

                        if (casOd.HasValue)
                        {
                            filter = Builders<TEntity>.Filter.And(
                                Builders<TEntity>.Filter.Gte(fieldName, casOd.Value),
                                Builders<TEntity>.Filter.Lt(fieldName, casDo.Value)
                            );
                        }
                    }
                    else
                    {
                        // Ostatni

                        switch (oper)
                        {
                            case Operator.Eq:
                                filter = Builders<TEntity>.Filter.Eq(fieldName, value);
                                break;
                            case Operator.Neq:
                                filter = Builders<TEntity>.Filter.Ne(fieldName, value);
                                break;
                            case Operator.EqCi:
                                if (fieldType == typeof(string) && value is string valEqCi)
                                {
                                    var regexFilter = Regex.Escape(valEqCi);
                                    filter = Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression(regexFilter, "i"));
                                }
                                else
                                {
                                    filter = Builders<TEntity>.Filter.Eq(fieldName, value);
                                }

                                break;
                            case Operator.NeqCi:
                                if (fieldType == typeof(string) && value is string valNeqCi)
                                {
                                    var regexFilter = Regex.Escape(valNeqCi);
                                    filter = Builders<TEntity>.Filter.Not(Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression(regexFilter, "i")));
                                }
                                else
                                {
                                    filter = Builders<TEntity>.Filter.Ne(fieldName, value);
                                }

                                break;
                            case Operator.Cont:
                                if (fieldType == typeof(string) && value is string valCont)
                                {
                                    var regexFilter = Regex.Escape(valCont);
                                    filter = Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression(regexFilter, "i"));
                                }
                                else
                                {
                                    filter = Builders<TEntity>.Filter.AnyEq(fieldName, value);
                                }

                                break;
                            case Operator.NCont:
                                if (fieldType == typeof(string) && value is string valNcont)
                                {
                                    var regexFilter = Regex.Escape(valNcont);
                                    filter = Builders<TEntity>.Filter.Not(Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression(regexFilter, "i")));
                                }
                                else
                                {
                                    filter = Builders<TEntity>.Filter.Not(Builders<TEntity>.Filter.AnyEq(fieldName, value));
                                }

                                break;
                            case Operator.Ends:
                                if (fieldType == typeof(string) && value is string valEnds)
                                {
                                    var regexFilter = Regex.Escape(valEnds);
                                    filter = Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression($"{regexFilter}$", "i"));
                                }

                                break;
                            case Operator.NEnds:
                                if (fieldType == typeof(string) && value is string valNends)
                                {
                                    var regexFilter = Regex.Escape(valNends);
                                    filter = Builders<TEntity>.Filter.Not(Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression($"{regexFilter}$", "i")));
                                }

                                break;
                            case Operator.Sts:
                                if (fieldType == typeof(string) && value is string valSts)
                                {
                                    var regexFilter = Regex.Escape(valSts);
                                    filter = Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression($"^{regexFilter}", "i"));
                                }

                                break;
                            case Operator.NSts:
                                if (fieldType == typeof(string) && value is string valNsts)
                                {
                                    var regexFilter = Regex.Escape(valNsts);
                                    filter = Builders<TEntity>.Filter.Not(Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression($"^{regexFilter}", "i")));
                                }

                                break;
                            case Operator.Gt:
                                filter = Builders<TEntity>.Filter.Gt(fieldName, value);
                                break;
                            case Operator.Gte:
                                filter = Builders<TEntity>.Filter.Gte(fieldName, value);
                                break;
                            case Operator.Lt:
                                filter = Builders<TEntity>.Filter.Lt(fieldName, value);
                                break;
                            case Operator.Lte:
                                filter = Builders<TEntity>.Filter.Lte(fieldName, value);
                                break;
                        }
                    }

                    if (filter != null)
                        filters.Add(filter);
                }
            }

            if (filters.Count > 0)
            {
                return oper == Operator.Nin ? Builders<TEntity>.Filter.And(filters) : Builders<TEntity>.Filter.Or(filters);
            }

            return null;
        }

        private object[] PrepareValue(Type propertyType, string value)
        {
            var tokens = Regex.Split(value, "(?<!\\\\),(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            var values = new object[tokens.Length];

            for (var i = 0; i < tokens.Length; i++)
            {
                tokens[i] = tokens[i].Trim().Trim('"').Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\\,", ",").Replace("\\:", ":");

                var propType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                try
                {
                    values[i] = Convert.ChangeType(tokens[i], propType);
                }
                catch (Exception ex)
                {
                    if (ex is FormatException or InvalidCastException)
                    {
                        values[i] = tokens[i];
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return values;
        }

        private Operator FindOperator(string field, Group group)
        {
            if (!group.Success)
            {
                return Operator.Eq;
            }

            if (Enum.TryParse<Operator>(group.Value, true, out var oper))
            {
                return oper;
            }

            throw new ArgumentException($"Unknown condition by field {field}: {@group.Value}.");

        }
    }
}