using System;
using System.Collections.Generic;

namespace Tides.Core
{
    public class BaseObject
    {
        public string type { get; }

        public BaseObject(string type = null)
        {
            this.type = string.IsNullOrEmpty(type) ? "Object" : type;
        }

        public string id { get; set; }
        public List<BaseObject> attachment { get; set; }
        public List<BaseObject> attributedTo { get; set; }
        public List<BaseObject> audience { get; set; }
        public string content { get; set; }
        public List<BaseObject> context { get; set; }
        public string name { get; set; }
        public DateTime? endTime { get; set; }
        public BaseObject generator { get; set; }
        public List<BaseObject> icon { get; set; }
        public List<BaseObject> image { get; set; }
        public List<BaseObject> inReplyTo { get; set; }
        public List<BaseObject> location { get; set; }
        public BaseObject preview { get; set; }
        public DateTime? published { get; set; }
        public CollectionObject replies { get; set; }
        public DateTime? startTime { get; set; }
        public string summary { get; set; }
        public List<BaseObject> tag { get; set; }
        public DateTime? updated { get; set; }
        public string url { get; set; }
        public List<BaseObject> to { get; set; }
        public List<BaseObject> bto { get; set; }
        public List<BaseObject> cc { get; set; }
        public List<BaseObject> bcc { get; set; }
        public string mediaType { get; set; }
        public TimeSpan? duration { get; set; }
    }
}