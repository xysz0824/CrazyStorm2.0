using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace CrazyStorm.Core
{
#if _NET_4
    public class GenericContainer<T> : ObservableCollection<T>
    {
    }
#else
    public class GenericContainer<T> : List<T>
    {
    }
#endif
}
