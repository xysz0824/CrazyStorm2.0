using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrazyStorm.Core
{
    public interface ICopyable<T>
    {
        void CopyTo(T copyable);
    }
}
