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
        private List<string> ignoredParams=new List<string>(new string[] {"limit","offset","sort"});

        private Dictionary<string, Type> knownProperties =
            new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase);

        public UrlFilterParserDynamic(Dictionary<string, Type> known=null)
        {
            if (known != null && known.Count > 0)
            {
                foreach (var pair in known)
                {
                    if(!knownProperties.ContainsKey(pair.Key))
                        knownProperties.Add(pair.Key, pair.Value);
                }
            }
        }

        private enum Operator
        {
            Eq, Neq, Cont, Ends, Sts, Gt, Gte, Lt, Lte, In, Nin, NCont, NEnds, NSts, EqCI, NeqCI
        }

        public UrlFilterParserDynamic<TEntity> IgnoreParams(params string[] list)
        {
            foreach (var param in list)
            {
                if(!string.IsNullOrWhiteSpace(param))
                    ignoredParams.Add(param.ToLower());
            }

            return this;
        }

        public DynamicQueryParams<TEntity> Parse(IDictionary<string, StringValues> queryString, FilterDefinition<TEntity> defaultFilter = null)
        {
            DynamicQueryParams<TEntity> result=new DynamicQueryParams<TEntity>();

            result.Filter = parseFilter(result.UnknownParams, queryString);

            result.FindOptions = new FindOptions<TEntity>() {Collation = new Collation("cs", strength: CollationStrength.Secondary)};
            parseSorting(result.FindOptions, queryString);
            parsePaging(result, queryString);

            return result;
        }

        private void parsePaging(DynamicQueryParams<TEntity> result, IDictionary<string, StringValues> queryString)
        {
            if (queryString != null && queryString.Count > 0)
            {
                foreach (var key in queryString.Keys)
                {
                    int limit;
                    if (key.Equals("limit", StringComparison.InvariantCultureIgnoreCase) && queryString[key].Count>0 && int.TryParse(queryString[key][0], out limit))
                    {
                        result.FindOptions.Limit = limit;
                        result.Limit = limit;
                    }

                    int offset;
                    if (key.Equals("offset", StringComparison.InvariantCultureIgnoreCase) && queryString[key].Count > 0 && int.TryParse(queryString[key][0], out offset))
                    {
                        result.FindOptions.Skip = offset;
                        result.Offset = offset;
                    }
                }
            }
        }

        private void parseSorting(FindOptions<TEntity> options, IDictionary<string, StringValues> queryString)
        {
            if (queryString != null && queryString.Count > 0)
            {
                SortDefinition<TEntity> sort = null;

                foreach (var key in queryString.Keys)
                {
                    if (key.Equals("sort", StringComparison.InvariantCultureIgnoreCase) && queryString[key].Count>0)
                    {
                        string[] fields = queryString[key][0]
                            .Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);

                        foreach (var field in fields)
                        {
                            if(string.IsNullOrWhiteSpace(field))
                                continue;
                            
                            string[] tokens=field.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                            if (tokens.Length > 0)
                            {
                                string fieldName = tokens[0].Trim();
                                string propName = getPropertyName(fieldName);

                                if (propName != null)
                                {
                                    SortDefinition<TEntity> oneSort;
                                    if (tokens.Length > 1 && "desc".Equals(tokens[1],
                                            StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        oneSort = Builders<TEntity>.Sort.Descending(propName);
                                    }
                                    else
                                    {
                                        oneSort = Builders<TEntity>.Sort.Ascending(propName);
                                    }

                                    if (sort == null)
                                    {
                                        sort = oneSort;
                                    }
                                    else
                                    {
                                        sort = Builders<TEntity>.Sort.Combine(sort, oneSort);
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException($"Nezname pole pro razeni zaznamu: {fieldName}.");
                                }
                            }
                        }
                    }
                }

                if (sort != null)
                    options.Sort = sort;
            }
        }

        private FilterDefinition<TEntity> parseFilter(IDictionary<string, StringValues> unknownParams, IDictionary<string, StringValues> queryString)
        {
            List<FilterDefinition<TEntity>> filters = new List<FilterDefinition<TEntity>>();

            if (queryString != null && queryString.Count > 0)
            {
                foreach (KeyValuePair<string, StringValues> pair in queryString)
                {
                    if(ignoredParams.Contains(pair.Key.ToLower()))
                        continue;

                    var propType = getPropertyType(pair.Key);
                    var propName = getPropertyName(pair.Key);

                    if (propType != null)
                    {
                        foreach (var value in pair.Value)
                        {
                            var filter = parseFilterParam(new PropInfo() {Name = propName, PropType = propType}, value);
                            if (filter != null)
                                filters.Add(filter);
                        }
                    }
                    else
                    {
                        unknownParams.Add(pair);
                    }
                }
            }

            if (filters.Count > 0)
            {
                return Builders<TEntity>.Filter.And(filters);
            }
            else
            {
                return Builders<TEntity>.Filter.Empty;
            }
        }

        private Type getPropertyType(string propertyName)
        {
            if (knownProperties.ContainsKey(propertyName))
                return knownProperties[propertyName];

            var properties = propertyName.Split(new char[] {'.','/'}, StringSplitOptions.RemoveEmptyEntries);
            var type = typeof(TEntity);

            for (int i=0;i<properties.Length;i++)
            {
                var property = properties[i];
                bool last = i == (properties.Length - 1);

                System.Reflection.PropertyInfo propInfo = type.GetProperty(property,
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (propInfo != null)
                {
                    if (last)
                        return propInfo.PropertyType;
                    else
                        type = propInfo.PropertyType;
                }
                else
                {
                    System.Reflection.FieldInfo fieldInfo = type.GetField(property,
                        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (fieldInfo != null)
                    {
                        if (last)
                            return fieldInfo.FieldType;
                        else
                            type = fieldInfo.FieldType;
                    }
                }
            }

            return null;
        }

        private string getPropertyName(string propertyName)
        {
            if (knownProperties.ContainsKey(propertyName))
                return knownProperties.FirstOrDefault(p=>p.Key.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase)).Key;

            List<string> propertyTokens = new List<string>();

            var properties = propertyName.Split(new char[] { '.', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var type = typeof(TEntity);

            for (int i = 0; i < properties.Length; i++)
            {
                var property = properties[i];
                bool last = i == (properties.Length - 1);

                System.Reflection.PropertyInfo propInfo = type.GetProperty(property,
                    System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (propInfo != null)
                {
                    if (last)
                    {
                        propertyTokens.Add(propInfo.Name);
                        break;
                    }
                    else
                    {
                        propertyTokens.Add(propInfo.Name);
                        type = propInfo.PropertyType;
                    }
                }
                else
                {
                    System.Reflection.FieldInfo fieldInfo = type.GetField(property,
                        System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                    if (fieldInfo != null)
                    {
                        if (last)
                        {
                            propertyTokens.Add(fieldInfo.Name);
                            break;
                        }
                        else
                        {
                            propertyTokens.Add(fieldInfo.Name);
                            type = fieldInfo.FieldType;
                        }
                    }
                }
            }

            if (propertyTokens.Count > 0)
            {
                string result = "";
                foreach (var token in propertyTokens)
                {
                    result += (result.Length > 0 ? "." : "") + token;
                }

                return result;
            }

            return null;
        }

        private Regex filterParamRegex=new Regex("^((?<cond>eq|neq|cont|ends|sts|gt|gte|lt|lte|in|nin|ncont|nends|nsts|eqci|neqci)\\:)?(?<value>.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);

        private FilterDefinition<TEntity> parseFilterParam(PropInfo propInfo, string value)
        {
            string fieldName = propInfo.Name;

            Match m = filterParamRegex.Match(value);
            if (m.Success)
            {
                Operator oper = findOperator(fieldName, m.Groups["cond"]);
                return addCondition(fieldName, propInfo.PropType, oper, prepareValue(propInfo.PropType, m.Groups["value"].Value));
                
            }
            else
            {
                return addCondition(fieldName, propInfo.PropType, Operator.Eq, prepareValue(propInfo.PropType, value));
            }
        }

        private List<string> timePeriods=new List<string>(new string[] {"today", "yesterday", "lastSevenDays", "lastThirtyDays", "prevMonth", "prevWeek" });

        private FilterDefinition<TEntity> addCondition(string fieldName, Type fieldType, Operator oper, object[] values)
        {
            List<FilterDefinition<TEntity>> filters = new List<FilterDefinition<TEntity>>();
            FilterDefinition<TEntity> filter = null;

            if (oper == Operator.In || oper == Operator.Nin)
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
                    filters.Add(filter);
            }
            else
            {

                foreach (var value in values)
                {

                    if (oper.Equals(Operator.Eq) && (fieldType == typeof(DateTime) || fieldType == typeof(DateTime?)) &&
                        value is string &&
                        timePeriods.Contains(value as string, StringComparer.OrdinalIgnoreCase))
                    {
                        // Casove obdobi
                        DateTime dnesniPulnoc = DateTime.Now.Date;
                        DateTime? casOd=null;
                        DateTime? casDo=null;
                        switch ((value as string).ToLower())
                        {
                            // today, yesterday, lastSevenDays, lastThirtyDays, prevMonth a prevWeek
                            case "today":
                                casOd=dnesniPulnoc;
                                casDo=dnesniPulnoc.AddDays(1);
                                break;
                            case "yesterday":
                                DateTime vcerejsiPulnoc = DateTime.Now.Date.Subtract(TimeSpan.FromDays(1));
                                casOd=vcerejsiPulnoc;
                                casDo=vcerejsiPulnoc.AddDays(1);
                                break;
                            case "lastsevendays":
                                DateTime predSestiDny = dnesniPulnoc.Subtract(TimeSpan.FromDays(6));
                                casOd=predSestiDny;
                                casDo=dnesniPulnoc.AddDays(1);
                                break;
                            case "lastthirtydays":
                                DateTime pred29Dny = dnesniPulnoc.Subtract(TimeSpan.FromDays(29));
                                casOd=pred29Dny;
                                casDo=dnesniPulnoc.AddDays(1);
                                break;
                            case "prevmonth":
                                var zacatekMinulehoMesice =
                                    new DateTime(dnesniPulnoc.Year, dnesniPulnoc.Month, 1).AddMonths(-1);
                                casOd=zacatekMinulehoMesice;
                                casDo=zacatekMinulehoMesice.AddMonths(1);
                                break;
                            case "prevweek":
                                var zacatekMinulehoTydne = dnesniPulnoc.AddDays(-(int) dnesniPulnoc.DayOfWeek - 6);
                                casOd=zacatekMinulehoTydne;
                                casDo=zacatekMinulehoTydne.AddDays(7);
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
                            case Operator.EqCI:
                                if (fieldType == typeof(string) && value is string valEqCi)
                                {
                                    var regexFilter = Regex.Escape(valEqCi);
                                    filter = Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression(regexFilter, "i"));
                                    break;
                                }
                                else
                                {
                                    filter = Builders<TEntity>.Filter.Eq(fieldName, value);
                                }

                                break;
                            case Operator.NeqCI:
                                if (fieldType == typeof(string) && value is string valNeqCi)
                                {
                                    var regexFilter = Regex.Escape(valNeqCi);
                                    filter = Builders<TEntity>.Filter.Not(Builders<TEntity>.Filter.Regex(fieldName,
                                        new BsonRegularExpression(regexFilter, "i")));
                                    break;
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
                                    break;
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
                                    break;
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
                if(oper == Operator.Nin)
                    return Builders<TEntity>.Filter.And(filters);
                else
                    return Builders<TEntity>.Filter.Or(filters);
            }
            else
            {
                return null;
            }

        }

        private object[] prepareValue(Type propertyType, string value)
        {
            var tokens=Regex.Split(value, "(?<!\\\\),(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            object[] values=new object[tokens.Length];

            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = tokens[i].Trim().Trim('"').Replace("\\\\", "\\").Replace("\\\"", "\"").Replace("\\,", ",").Replace("\\:", ":");

                Type propType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                try
                {
                    values[i] = Convert.ChangeType(tokens[i], propType);
                }
                catch (Exception ex)
                {
                    if (ex is FormatException || ex is InvalidCastException)
                        values[i] = tokens[i];
                    else
                        throw;
                }
            }

            return values;
        }

        private Operator findOperator(string field, Group group)
        {
            if (group.Success)
            {
                Operator oper;
                if(Enum.TryParse< Operator>(group.Value, true, out oper))
                {
                    return oper;
                }
                else
                {
                    throw new ArgumentException($"Neznama podminka filtru podle sloupce {field}: {group.Value}.");
                }
            }
            else
            {
                return Operator.Eq;
            }
        }
    }
}