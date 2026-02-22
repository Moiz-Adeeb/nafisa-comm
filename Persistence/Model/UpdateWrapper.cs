using System.Linq.Expressions;

namespace Persistence.Model
{
    public class UpdateWrapper<T>
    {
        public Expression<Func<T, object>> Expression { get; set; }
        public object Value { get; set; }
    }
}
