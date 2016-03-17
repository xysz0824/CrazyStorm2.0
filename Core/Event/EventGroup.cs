/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2015 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace CrazyStorm.Core
{
    public class EventGroup
    {
        #region Private Members
        string name;
        string condition;
        ObservableCollection<string> events;
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
        public ObservableCollection<string> Events { get { return events; } }
        #endregion

        #region Constructor
        public EventGroup()
        {
            name = "NewEventGroup";
            events = new ObservableCollection<string>();
        }
        #endregion
    }
}
