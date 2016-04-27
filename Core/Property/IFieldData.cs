using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    interface IFieldData
    {
        void SetField(int fieldIndex, ValueType value);
        ValueType GetField(int fieldIndex);
    }
}
