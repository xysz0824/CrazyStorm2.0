using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyStorm.Core
{
    public class EventGroup
    {
        #region Private Members
        string name;
        string condition;
        List<string> events;
        #endregion

        #region Public Members
        public string Name 
        { 
            get { return name; }
            set { name = value; }
        }
        public string Condition
        {
            get { return condition; }
            set { condition = value; }
        }
        public IList<string> Events { get { return events; } }
        #endregion

        #region Constructor
        public EventGroup()
        {
            name = "NewEventGroup";
            events = new List<string>();
        }
        #endregion
    }
}
