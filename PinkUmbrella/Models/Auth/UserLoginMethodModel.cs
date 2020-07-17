using System;

namespace PinkUmbrella.Models.Auth
{
    public class UserLoginMethodModel
    {
        public long Id { get; set; }
        public DateTime WhenCreated { get; set; }
        public int UserId { get; set; }
        public UserLoginMethod Method { get; set; }
        public bool Enabled { get; set; }
    }
}