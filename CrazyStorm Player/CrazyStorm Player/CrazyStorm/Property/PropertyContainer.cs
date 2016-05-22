using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CrazyStorm_Player.CrazyStorm
{
    public class PropertyContainer
    {
        IDictionary<string, VMInstruction[]> propertyExpressions;
        IDictionary<string, VMInstruction[]> PropertyExpressions { get { return propertyExpressions; } }
        public PropertyContainer()
        {
            propertyExpressions = new Dictionary<string, VMInstruction[]>();
        }
        public void BuildFromPlayData(BinaryReader reader)
        {
            using (BinaryReader listReader = PlayDataHelper.GetBlockReader(reader))
            {
                while (!PlayDataHelper.EndOfReader(listReader))
                {
                    using (BinaryReader expressionReader = PlayDataHelper.GetBlockReader(listReader))
                    {
                        string propertyName = PlayDataHelper.ReadString(expressionReader);
                        int bytesLength = (int)expressionReader.BaseStream.Length - propertyName.Length - 1;
                        propertyExpressions[propertyName] = VM.Decode(expressionReader.ReadBytes(bytesLength));
                    }
                }
            }
        }
    }
}
