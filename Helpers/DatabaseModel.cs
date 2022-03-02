using Query.Managers;
using System.Collections.Generic;
using System.Linq;

namespace Query.Helpers
{
    internal class DatabaseModel
    {
        public List<Helpers.Attribute> Attributes { get; }
        public List<ForeignKey> ForeignKeys { get; }
        public DatabaseModel()
        {
            Attributes = DataProvider.Instance.GetAttributes();
            ForeignKeys = DataProvider.Instance.GetForeignKeys();
        }
        public ForeignKey GetForeignKey(string tableFrom, string tableTo)
        {
            return ForeignKeys.FirstOrDefault(fk => fk.TableFrom == tableFrom && fk.TableTo == tableTo || fk.TableFrom == tableTo && fk.TableTo == tableFrom);
        }
        
        internal List<string> GetNeighbors(string table)
        {
            return ForeignKeys.Where(fk => fk.TableTo == table).Select(fk => fk.TableFrom).Union(
                ForeignKeys.Where(fk => fk.TableFrom == table).Select(fk => fk.TableTo)).ToList();
        }
    }
}
