using Query.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Query.Managers
{
    internal class QueryBuilder
    {
        private DatabaseModel _database;
        public QueryBuilder(DatabaseModel database)
        {
            _database = database;
        }
        public string QueryBuild(List<Helpers.Attribute> checkedDttributes, List<Condition> conditions)
        {
            string select = $"SELECT {string.Join(", ", checkedDttributes.Select(x => $"{x.TableName}.{x.Name}"))}";

            List<string> usedTables = checkedDttributes.Select(x => x.TableName).Distinct().ToList();
            string from = "FROM " + TableJoinBuild(usedTables);

            if (conditions.Count == 0)
            {
                return $"{select}\n{from}";
            }
            string where = "WHERE " + ConditionsBuilder(conditions);

            return $"{select}\n{from}\n{where}";
        }

        private string ConditionsBuilder(List<Condition> conditions)
        {
            Condition first = conditions.First();
            string res = $"{first.Table}.{first.Attribute} {first.Operator} {first.Value}";
            foreach (var item in conditions.Skip(1))
            {
                res += $"\n{item.Operation} {item.Table}.{item.Attribute} {item.Operator} {item.Value}";
            }
            return res;
        }

        private string TableJoinBuild(List<string> usedTables)
        {
            switch (usedTables.Count)
            {
                case 0: return "";
                case 1: return usedTables.First();
                default:
                    {
                        var pathPairs = new List<ForeignKey>();
                        foreach (var item in usedTables.Skip(1))
                        {
                            var path = GetPathFK(usedTables.First(), item);
                            if (path is null)
                            {
                                return $"Нет связей между таблицами: {usedTables.First()},{item}";
                            }
                            pathPairs.AddRange(path);
                        }

                        var res = string.Empty;

                        var storeTables = new HashSet<string>();
                        foreach (var item in pathPairs.Distinct())
                        {
                            if (string.IsNullOrEmpty(res))
                            {
                                res = item.TableTo;
                                storeTables.Add(item.TableTo);
                            }

                            var tableToJoin = storeTables.Contains(item.TableFrom) ? item.TableTo : item.TableFrom;

                            res += $"\n JOIN {tableToJoin} ON {item.TableFrom}.{item.AttributeFrom} = {item.TableTo}.{item.AttributeTo}";
                            storeTables.Add(tableToJoin);
                        }
                        return res;
                    }
            }
        }

        private List<ForeignKey> GetPathFK(string tableFrom, string tableTo, HashSet<string> usedTables = null)
        {
            usedTables = usedTables ?? new HashSet<string>();

            usedTables.Add(tableFrom);
            var fk = _database.GetForeignKey(tableFrom, tableTo);
            if (fk != null)
            {
                List<ForeignKey> foreignKeys1 = new List<ForeignKey>();
                foreignKeys1.Add(fk);
                return foreignKeys1;
            }

            var neighbors = _database.GetNeighbors(tableFrom);
            foreach (var neighbor in neighbors)
            {
                if (!usedTables.Contains(neighbor))
                {
                    var list = GetPathFK(neighbor, tableTo, usedTables);
                    if (list != null)
                    {
                        var fkey = _database.GetForeignKey(tableFrom, neighbor);
                        list.Add(fkey);
                        return list;
                    }
                }
            }
            return null;
        }
    }
}
