using System;
using System.Collections.Generic;
using Estuary.Core;

namespace Estuary.Activities
{
    public class Question: ActivityObject
    {
        public Question() : base(nameof(Question), null) { }

        public List<BaseObject> oneOf { get; set; }
        public List<BaseObject> anyOf { get; set; }
        public DateTime? closed { get; set; }
    }
}