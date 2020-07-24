using System;
using System.Collections.Generic;
using Tides.Core;

namespace Tides.Activities
{
    public class Question: ActivityObject
    {
        public Question(string type = null) : base(type ?? nameof(Question)) { }

        public List<BaseObject> oneOf { get; set; }
        public List<BaseObject> anyOf { get; set; }
        public DateTime? closed { get; set; }
    }
}